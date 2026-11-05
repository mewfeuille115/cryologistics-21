using Cryologistics21.Common.Application.Authorization;
using Cryologistics21.Common.Application.Messaging;

namespace Cryologistics21.Services.Users.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(string IdentityId) : IQuery<PermissionsResponse>;
