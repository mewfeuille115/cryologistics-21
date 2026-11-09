using System.Net;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace Cryologistics21.Services.Users.Infrastructure.Identity;

internal sealed class IdentityProviderService(
		KeyCloakClient keyCloakClient,
		ILogger<IdentityProviderService> logger
	) : IIdentityProviderService
{
	private const string PasswordCredentialType = "password";

	// POST /admin/realms/{realm}/users
	public async Task<Result<string>> RegisterUserAsync(UserModel user, CancellationToken cancellationToken = default)
	{
		var userRepresentation = new UserRepresentation(
			user.Email,
			user.Email,
			user.FirstName,
			user.LastName,
			true,
			true,
			[new CredentialRepresentation(PasswordCredentialType, user.Password, false)]);

		try
		{
			string identityId = await keyCloakClient.RegisterUserAsync(userRepresentation, cancellationToken);

			return Result.Success(identityId);
		}
		catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
		{
			logger.LogError(exception, "User registration failed");

			return Result.Failure<string>(IdentityProviderErrors.EmailIsNotUnique);
		}
	}

	// POST /realms/{realm}/protocol/openid-connect/token
	async Task<Result<Application.Abstractions.Identity.UserTokenResponse>> IIdentityProviderService.LoginUserAsync(
		LoginModel user,
		CancellationToken cancellationToken)
	{
		var loginUser = new UserLogin(
			user.Email,
			user.Password);

		try
		{
			UserTokenResponse token = await keyCloakClient.AccessTokenAsync(loginUser, cancellationToken);
			var result = new Application.Abstractions.Identity.UserTokenResponse
			{
				AccessToken = token.AccessToken,
				ExpiresIn = token.ExpiresIn,
				RefreshExpiresIn = token.RefreshExpiresIn,
				RefreshToken = token.RefreshToken
			};

			return Result.Success(result);
		}
		catch (HttpRequestException exception) when (
			exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
		{
			logger.LogError(exception, "User login failed");
			return Result.Failure<Application.Abstractions.Identity.UserTokenResponse>(IdentityProviderErrors.InvalidCredentials);
		}
	}
}
