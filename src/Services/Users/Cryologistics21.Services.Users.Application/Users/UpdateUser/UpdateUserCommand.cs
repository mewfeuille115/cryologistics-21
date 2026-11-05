using Cryologistics21.Common.Application.Messaging;

namespace Cryologistics21.Services.Users.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
	Guid UserId,
	string FirstName,
	string LastName) : ICommand;
