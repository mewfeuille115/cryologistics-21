using Cryologistics21.Common.Application.Authorization;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Cryologistics21.Services.Users.Application.Users.GetUserPermissions;

namespace Cryologistics21.Services.Users.Infrastructure.Authorization;

internal sealed class PermissionService(
		IQueryHandler<GetUserPermissionsQuery, PermissionsResponse> handler
	) : IPermissionService
{
	public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
	{
		return await handler.Handle(
			new GetUserPermissionsQuery(identityId),
			CancellationToken.None);
	}
}
