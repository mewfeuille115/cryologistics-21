using System.Data;
using System.Data.Common;
using Cryologistics21.Common.Application.Clock;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Infrastructure.Outbox;
using Cryologistics21.Common.Infrastructure.Serialization;
using Cryologistics21.Services.Users.Application;
using Cryologistics21.Services.Users.Infrastructure.Outbox;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace Cryologistics21.Modules.Users.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed partial class ProcessOutboxJob(
		IDbConnectionFactory dbConnectionFactory,
		IServiceScopeFactory serviceScopeFactory,
		IDateTimeProvider dateTimeProvider,
		IOptions<OutboxOptions> outboxOptions,
		ILogger<ProcessOutboxJob> logger
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

		IReadOnlyList<OutboxMessageResponse> outboxMessages = await GetOutboxMessagesAsync(connection, transaction, cancellationToken);

		foreach (OutboxMessageResponse outboxMessage in outboxMessages)
		{
			Exception? exception = null;
			try
			{
				IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
					outboxMessage.Content,
					SerializerSettings.Instance)!;

				using IServiceScope scope = serviceScopeFactory.CreateScope();

				IEnumerable<IDomainEventHandler> domainEventHandlers = DomainEventHandlersFactory.GetHandlers(
					domainEvent.GetType(),
					scope.ServiceProvider,
					AssemblyReference.Assembly);

				foreach (IDomainEventHandler domainEventHandler in domainEventHandlers)
				{
					await domainEventHandler.Handle(domainEvent, cancellationToken);
				}
			}
			catch (Exception caughtException)
			{
				logger.LogError(
					caughtException,
					"{Module} - Exception while processing outbox message {MessageId}",
					ModuleName,
					outboxMessage.Id);

				exception = caughtException;
			}

			await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
		}

		await transaction.CommitAsync(cancellationToken);

		LogCompletedProcessing(logger, ModuleName);
	}

	private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
		IDbConnection connection,
		IDbTransaction transaction,
		CancellationToken cancellationToken)
	{
		string sql =
			$"""
			SELECT
				id AS {nameof(OutboxMessageResponse.Id)},
				content AS {nameof(OutboxMessageResponse.Content)}
			FROM users.outbox_messages
			WHERE processed_on_utc IS NULL
			ORDER BY occurred_on_utc
			LIMIT @BatchSize
			FOR UPDATE
			""";

		CommandDefinition command = new(
			sql,
			parameters: new { outboxOptions.Value.BatchSize },
			transaction: transaction,
			cancellationToken: cancellationToken);

		IEnumerable<OutboxMessageResponse> outboxMessages =
			await connection.QueryAsync<OutboxMessageResponse>(command);

		return [.. outboxMessages];
	}

	private async Task UpdateOutboxMessageAsync(
		IDbConnection connection,
		IDbTransaction transaction,
		OutboxMessageResponse outboxMessage,
		Exception? exception)
	{
		const string sql =
			"""
			UPDATE users.outbox_messages
			SET processed_on_utc = @ProcessedOnUtc,
				error = @Error
			WHERE id = @Id
			""";

		await connection.ExecuteAsync(
			sql,
			new
			{
				outboxMessage.Id,
				ProcessedOnUtc = dateTimeProvider.UtcNow,
				Error = exception?.ToString()
			},
			transaction: transaction);
	}

	[LoggerMessage(LogLevel.Information, "{Module} - Beginning to process outbox messages")]
	private static partial void LogBeginProcessing(ILogger logger, string module);

	[LoggerMessage(LogLevel.Information, "{Module} - Completed processing outbox messages")]
	private static partial void LogCompletedProcessing(ILogger logger, string module);

	internal sealed record OutboxMessageResponse(Guid Id, string Content);
}
