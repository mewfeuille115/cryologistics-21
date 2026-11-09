namespace Cryologistics21.Services.Users.Application.Users.LoginUser;

public sealed record LoginUserResponse(
	string AccessToken,
	int ExpiresIn,
	string RefreshToken,
	int RefreshExpiresIn);
