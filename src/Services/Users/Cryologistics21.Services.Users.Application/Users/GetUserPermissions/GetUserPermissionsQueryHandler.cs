using System.Data.Common;
using Cryologistics21.Common.Application.Authorization;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Domain.Users;
using Dapper;

namespace Cryologistics21.Services.Users.Application.Users.GetUserPermissions;

internal sealed class GetUserPermissionsQueryHandler(
		IDbConnectionFactory dbConnectionFactory
	) : IQueryHandler<GetUserPermissionsQuery, PermissionsResponse>
{
	public async Task<Result<PermissionsResponse>> Handle(
		GetUserPermissionsQuery request,
		CancellationToken cancellationToken)
	{
		DbConnection connection = await dbConnectionFactory
			.OpenConnectionAsync(cancellationToken)
			;

		await using (connection)
		{
			const string sql =
			$"""
			SELECT DISTINCT
				u.id AS {nameof(UserPermission.UserId)},
				rp.permission_code AS {nameof(UserPermission.Permission)}
			FROM users.users u
			JOIN users.user_roles ur ON ur.user_id = u.id
			JOIN users.role_permissions rp ON rp.role_name = ur.role_name
			WHERE u.identity_id = @IdentityId
			""";

			List<UserPermission> permissions = (await connection
					.QueryAsync<UserPermission>(sql, request)
					)
				.AsList();

			if (permissions.Count == 0)
			{
				return Result.Failure<PermissionsResponse>(UserErrors.NotFound(request.IdentityId));
			}

			return Result.Success(
				new PermissionsResponse(
					permissions[0].UserId,
					[.. permissions.Select(p => p.Permission)]));
		}
	}

	internal sealed class UserPermission
	{
		internal Guid UserId { get; init; }
		internal string Permission { get; init; } = null!;
	}
}
