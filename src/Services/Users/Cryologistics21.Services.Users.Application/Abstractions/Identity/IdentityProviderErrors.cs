using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
	public static readonly DomainError EmailIsNotUnique = DomainError.Conflict(
		"Identity.EmailIsNotUnique",
		"The specified email is not unique.");
}
