using System.Data.Common;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Infrastructure.Outbox;
using Dapper;

namespace Cryologistics21.Services.Users.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
		IDomainEventHandler<TDomainEvent> decorated,
		IDbConnectionFactory dbConnectionFactory
	) : DomainEventHandler<TDomainEvent>
		where TDomainEvent : IDomainEvent
{
	public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
	{
		await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

		var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, decorated.GetType().Name);

		if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer))
		{
			return;
		}

		await decorated.Handle(domainEvent, cancellationToken);

		await InsertOutboxConsumerAsync(connection, outboxMessageConsumer);
	}

	private static async Task<bool> OutboxConsumerExistsAsync(
		DbConnection dbConnection,
		OutboxMessageConsumer outboxMessageConsumer)
	{
		const string sql =
			"""
			SELECT EXISTS(
				SELECT 1
				FROM users.outbox_message_consumers
				WHERE outbox_message_id = @OutboxMessageId AND
					  name = @Name
			)
			""";

		return await dbConnection.ExecuteScalarAsync<bool>(sql, outboxMessageConsumer);
	}

	private static async Task InsertOutboxConsumerAsync(
		DbConnection dbConnection,
		OutboxMessageConsumer outboxMessageConsumer)
	{
		const string sql =
			"""
			INSERT INTO users.outbox_message_consumers(outbox_message_id, name)
			VALUES (@OutboxMessageId, @Name)
			""";

		await dbConnection.ExecuteAsync(sql, outboxMessageConsumer);
	}
}
