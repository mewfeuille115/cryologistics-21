namespace Cryologistics21.Services.Users.Infrastructure.Identity;

internal sealed record UserLogin(
	string Username,
	string Password
);
