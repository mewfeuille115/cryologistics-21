using Cryologistics21.Common.Domain;
using Cryologistics21.Common.Infrastructure.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Cryologistics21.Common.Infrastructure.Outbox;

public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
	public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		if (eventData.Context is not null)
		{
			InsertOutboxMessages(eventData.Context);
		}

		return await base.SavingChangesAsync(eventData, result, cancellationToken);
	}

	private static void InsertOutboxMessages(DbContext context)
	{
		var outboxMessages = context
			.ChangeTracker
			.Entries<Entity>()
			.Select(entry => entry.Entity)
			.SelectMany(entity =>
			{
				IReadOnlyCollection<IDomainEvent> domainEvents = entity.DomainEvents;

				entity.ClearDomainEvents();

				return domainEvents;
			})
			.Select(domainEvent => new OutboxMessage
			{
				Id = Guid.NewGuid(),
				Type = domainEvent.GetType().Name,
				Content = JsonConvert.SerializeObject(domainEvent, SerializerSettings.Instance),
				OccurredOnUtc = domainEvent.OccurredOnUtc,
			})
			.ToList();

		context.Set<OutboxMessage>().AddRange(outboxMessages);
	}
}
