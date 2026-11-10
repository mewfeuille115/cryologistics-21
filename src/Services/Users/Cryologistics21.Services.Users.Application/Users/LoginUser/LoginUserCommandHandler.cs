using System.Data.Common;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Application.Abstractions.Identity;
using Cryologistics21.Services.Users.Domain.Users;
using Dapper;

namespace Cryologistics21.Services.Users.Application.Users.LoginUser;

internal sealed class LoginUserCommandHandler(
	IDbConnectionFactory dbConnectionFactory,
	IIdentityProviderService identityProviderService
) : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
	public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
	{
		Result<UserTokenResponse> result = await identityProviderService
			.LoginUserAsync(
				new LoginModel(
					request.Email,
					request.Password
				),
				cancellationToken);

		if (result.IsFailure)
		{
			if (result.Error == IdentityProviderErrors.InvalidCredentials)
			{
				return Result.Failure<LoginUserResponse>(UserErrors.InvalidEmailOrPassword(request.Email));
			}

			return Result.Failure<LoginUserResponse>(result.Error);
		}

		DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

		await using (connection)
		{
			const string sql =
				$"""
				SELECT
					id AS {nameof(ValidUser.Id)},
					email AS {nameof(ValidUser.Email)}
				FROM users.users
				WHERE email = @Email
				""";

			ValidUser? user = await connection.QuerySingleOrDefaultAsync<ValidUser>(sql, request);

			if (user is null)
			{
				return Result.Failure<LoginUserResponse>(UserErrors.InvalidEmailOrPassword(request.Email));
			}

			return Result.Success(new LoginUserResponse(
				result.Value.AccessToken,
				result.Value.ExpiresIn,
				result.Value.RefreshToken,
				result.Value.RefreshExpiresIn));
		}
	}

	internal sealed class ValidUser
	{
		internal Guid Id { get; init; }
		internal string Email { get; init; } = null!;
	}
}
