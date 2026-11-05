using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.Domain.Users;

public sealed class User : Entity
{
	private readonly List<Role> _roles = [];

	private User()
	{
	}

	public Guid Id { get; private set; }
	public string Email { get; private set; } = null!;
	public string FirstName { get; private set; } = null!;
	public string LastName { get; private set; } = null!;
	public string IdentityId { get; private set; } = null!;
	public IReadOnlyCollection<Role> Roles => [.. _roles];

	public static User Create(
		string email,
		string firstName,
		string lastName,
		string identityId)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			Email = email,
			FirstName = firstName,
			LastName = lastName,
			IdentityId = identityId,
		};

		user._roles.Add(Role.Member);

		user.RecordDomainEvent(new UserRegisteredDomainEvent(user.Id));

		return user;
	}

	public void Update(string firstName, string lastName)
	{
		if (FirstName == firstName && LastName == lastName)
		{
			return;
		}

		FirstName = firstName;
		LastName = lastName;

		RecordDomainEvent(new UserProfileUpdatedDomainEvent(Id, FirstName, LastName));
	}
}
