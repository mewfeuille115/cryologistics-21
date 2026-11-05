using Cryologistics21.Common.Application.Messaging;

namespace Cryologistics21.Services.Users.Application.Users.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserResponse>;
