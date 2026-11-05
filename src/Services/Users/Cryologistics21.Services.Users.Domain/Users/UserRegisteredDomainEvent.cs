using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.Domain.Users;

public sealed class UserRegisteredDomainEvent(Guid userId) : DomainEvent
{
	public Guid UserId { get; init; } = userId;
}
