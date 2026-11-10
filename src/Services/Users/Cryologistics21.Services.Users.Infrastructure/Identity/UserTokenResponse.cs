using System.Text.Json.Serialization;

namespace Cryologistics21.Services.Users.Infrastructure.Identity;

internal sealed class UserTokenResponse
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; init; } = string.Empty;

	[JsonPropertyName("expires_in")]
	public int ExpiresIn { get; init; }

	[JsonPropertyName("refresh_expires_in")]
	public int RefreshExpiresIn { get; init; }

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; init; } = string.Empty;
}
