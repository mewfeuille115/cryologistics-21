using Cryologistics21.Common.Application.EventBus;
using Wolverine;

namespace Cryologistics21.Common.Infrastructure.EventBus;

internal sealed class EventBus(IMessageBus bus) : IEventBus
{
	public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
		where T : IIntegrationEvent
	{
		await bus.PublishAsync(integrationEvent);
	}
}
