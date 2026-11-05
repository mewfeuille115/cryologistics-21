using Cryologistics21.Common.Domain;

namespace Cryologistics21.Services.Users.Application.Abstractions.Identity;

public interface IIdentityProviderService
{
	Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default);
}
