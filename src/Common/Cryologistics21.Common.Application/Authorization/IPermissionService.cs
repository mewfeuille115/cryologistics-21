using Cryologistics21.Common.Domain;

namespace Cryologistics21.Common.Application.Authorization;

public interface IPermissionService
{
	Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}
