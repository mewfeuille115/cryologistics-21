namespace Cryologistics21.Services.Users.Application.Abstractions.Identity;

public sealed class UserTokenResponse
{
	public string AccessToken { get; init; } = string.Empty;
	public int ExpiresIn { get; init; }
	public int RefreshExpiresIn { get; init; }
	public string RefreshToken { get; init; } = string.Empty;
}
