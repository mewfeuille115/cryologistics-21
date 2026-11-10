namespace Cryologistics21.Services.Users.Application.Abstractions.Identity;

public sealed record LoginModel(
	string Email,
	string Password);
