using Cryologistics21.Common.Domain;

namespace Cryologistics21.Common.Application.Messaging;

public interface IDomainEventHandler<in TDomainEvent> : IDomainEventHandler
	where TDomainEvent : IDomainEvent
{
	Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler
{
	Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
