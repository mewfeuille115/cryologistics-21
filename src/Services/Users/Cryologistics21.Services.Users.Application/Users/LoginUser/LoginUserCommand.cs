using Cryologistics21.Common.Application.Messaging;

namespace Cryologistics21.Services.Users.Application.Users.LoginUser;

public sealed record LoginUserCommand(
	string Email,
	string Password) : ICommand<LoginUserResponse>;
