using Cryologistics21.Common.Application.Messaging;

namespace Cryologistics21.Services.Users.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(
	string Email,
	string Password,
	string FirstName,
	string LastName) : ICommand<Guid>;
