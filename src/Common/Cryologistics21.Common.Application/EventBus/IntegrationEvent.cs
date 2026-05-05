namespace Cryologistics21.Common.Application.EventBus;

public abstract class IntegrationEvent : IIntegrationEvent
{
	public Guid Id { get; init; }
	public DateTime OccurredOnUtc { get; init; }

	protected IntegrationEvent(Guid id, DateTime occurredOnUtc)
	{
		Id = id;
		OccurredOnUtc = occurredOnUtc;
	}
}
