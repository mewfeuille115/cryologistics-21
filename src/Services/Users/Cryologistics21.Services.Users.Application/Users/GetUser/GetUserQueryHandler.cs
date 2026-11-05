using System.Data.Common;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Domain.Users;
using Dapper;

namespace Cryologistics21.Services.Users.Application.Users.GetUser;

internal sealed class GetUserQueryHandler(
		IDbConnectionFactory dbConnectionFactory
	) : IQueryHandler<GetUserQuery, UserResponse>
{
	public async Task<Result<UserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
	{
		DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

		await using (connection)
		{
			const string sql =
				$"""
				SELECT
					id AS {nameof(UserResponse.Id)},
					email AS {nameof(UserResponse.Email)},
					first_name AS {nameof(UserResponse.FirstName)},
					last_name AS {nameof(UserResponse.LastName)}
				FROM users.users
				WHERE id = @UserId
				""";

			UserResponse? user = await connection.QuerySingleOrDefaultAsync<UserResponse>(sql, request);

			if (user is null)
			{
				return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
			}

			return Result.Success(user);
		}
	}
}
