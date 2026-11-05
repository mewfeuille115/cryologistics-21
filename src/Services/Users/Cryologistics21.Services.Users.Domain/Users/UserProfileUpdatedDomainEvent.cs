using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.Domain.Users;

public sealed class UserProfileUpdatedDomainEvent(Guid userId, string firstName, string lastName) : DomainEvent
{
	public Guid UserId { get; init; } = userId;
	public string FirstName { get; init; } = firstName;
	public string LastName { get; init; } = lastName;
}
