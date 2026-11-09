using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.Domain.Users;

public static class UserErrors
{
	public static DomainError NotFound(Guid userId) =>
		DomainError.NotFound("Users.NotFound", $"The user with the identifier {userId} not found");

	public static DomainError NotFound(string identityId) =>
		DomainError.NotFound("Users.NotFound", $"The user with the IDP identifier {identityId} not found");

	public static DomainError InvalidEmailOrPassword(string email) =>
		DomainError.NotFound("Users.InvalidEmailOrPassword", $"The email or password is invalid for the user with the email {email}");
}
