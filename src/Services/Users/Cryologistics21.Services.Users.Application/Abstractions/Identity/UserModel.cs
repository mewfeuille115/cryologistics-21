namespace Cryologistics21.Services.Users.Application.Abstractions.Identity;

public sealed record UserModel(
	string Email,
	string Password,
	string FirstName,
	string LastName);
