using System.Data.Common;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.EventBus;
using Cryologistics21.Common.Infrastructure.Inbox;
using Cryologistics21.Common.Infrastructure.Serialization;
using Dapper;
using Newtonsoft.Json;

namespace Cryologistics21.Services.Users.Infrastructure.Inbox;

public abstract class IntegrationEventConsumer<TIntegrationEvent>(
		IDbConnectionFactory dbConnectionFactory
	) where TIntegrationEvent : IntegrationEvent
{
	public async Task ConsumeAsync(TIntegrationEvent integrationEvent)
	{
		await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

		var inboxMessage = new InboxMessage
		{
			Id = integrationEvent.Id,
			Type = integrationEvent.GetType().Name,
			Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
			OccurredOnUtc = integrationEvent.OccurredOnUtc
		};

		const string sql =
			"""
			INSERT INTO users.inbox_messages(id, type, content, occurred_on_utc)
			VALUES (@Id, @Type, @Content::json, @OccurredOnUtc)
			""";

		await connection.ExecuteAsync(sql, inboxMessage);
	}
}
