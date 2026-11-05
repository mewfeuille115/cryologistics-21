using System.Data;
using System.Data.Common;
using Cryologistics21.Common.Application.Clock;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.EventBus;
using Cryologistics21.Common.Infrastructure.Inbox;
using Cryologistics21.Common.Infrastructure.Serialization;
using Cryologistics21.Services.Users.Infrastructure.Inbox;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace Cryologistics21.Modules.Users.Infrastructure.Inbox;

[DisallowConcurrentExecution]
internal sealed partial class ProcessInboxJob(
		IDbConnectionFactory dbConnectionFactory,
		IServiceScopeFactory serviceScopeFactory,
		IDateTimeProvider dateTimeProvider,
		IOptions<InboxOptions> inboxOptions,
		ILogger<ProcessInboxJob> logger
	) : IJob
{
	private const string ModuleName = "Users";

	public async ValueTask Execute(
		IJobExecutionContext context,
		CancellationToken cancellationToken = default)
	{
		LogBeginProcessing(logger, ModuleName);

		await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);
		await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

		IReadOnlyList<InboxMessageResponse> inboxMessages = await GetInboxMessagesAsync(
			connection,
			transaction,
			cancellationToken);

		foreach (InboxMessageResponse inboxMessage in inboxMessages)
		{
			Exception? exception = null;

			try
			{
				IIntegrationEvent integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
					inboxMessage.Content,
					SerializerSettings.Instance)!;

				using IServiceScope scope = serviceScopeFactory.CreateScope();

				IEnumerable<IIntegrationEventHandler> handlers = IntegrationEventHandlersFactory.GetHandlers(
					integrationEvent.GetType(),
					scope.ServiceProvider,
					Services.Users.Presentation.AssemblyReference.Assembly);

				foreach (IIntegrationEventHandler integrationEventHandler in handlers)
				{
					await integrationEventHandler.Handle(integrationEvent, cancellationToken);
				}
			}
			catch (Exception caughtException)
			{
				logger.LogError(
					caughtException,
					"{Module} - Exception while processing inbox message {MessageId}",
					ModuleName,
					inboxMessage.Id);

				exception = caughtException;
			}

			await UpdateInboxMessageAsync(connection, transaction, inboxMessage, exception);
		}

		await transaction.CommitAsync(cancellationToken);

		LogCompletedProcessing(logger, ModuleName);
	}

	private async Task<IReadOnlyList<InboxMessageResponse>> GetInboxMessagesAsync(
		IDbConnection connection,
		IDbTransaction transaction,
		CancellationToken cancellationToken)
	{
		const string sql =
			"""
			SELECT
				id AS Id,
				content AS Content
			FROM users.inbox_messages
			WHERE processed_on_utc IS NULL
			ORDER BY occurred_on_utc
			LIMIT @BatchSize
			FOR UPDATE
			""";

		CommandDefinition command = new(
			sql,
			parameters: new { inboxOptions.Value.BatchSize },
			transaction: transaction,
			cancellationToken: cancellationToken);

		IEnumerable<InboxMessageResponse> inboxMessages =
			await connection.QueryAsync<InboxMessageResponse>(command);

		return inboxMessages.AsList();
	}

	private async Task UpdateInboxMessageAsync(
		IDbConnection connection,
		IDbTransaction transaction,
		InboxMessageResponse inboxMessage,
		Exception? exception)
	{
		const string sql =
			"""
			UPDATE users.inbox_messages
			SET processed_on_utc = @ProcessedOnUtc,
				error = @Error
			WHERE id = @Id
			""";

		await connection.ExecuteAsync(
			sql,
			new
			{
				inboxMessage.Id,
				ProcessedOnUtc = dateTimeProvider.UtcNow,
				Error = exception?.ToString()
			},
			transaction: transaction);
	}

	[LoggerMessage(LogLevel.Information, "{Module} - Beginning to process inbox messages")]
	private static partial void LogBeginProcessing(ILogger logger, string module);

	[LoggerMessage(LogLevel.Information, "{Module} - Completed processing inbox messages")]
	private static partial void LogCompletedProcessing(ILogger logger, string module);

	internal sealed record InboxMessageResponse(Guid Id, string Content);
}
