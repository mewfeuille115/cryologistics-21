using System.Text.Json.Serialization;

namespace Cryologistics21.Services.Users.Infrastructure.Identity;

internal sealed record KeyCloakRealmUser(
	[property: JsonPropertyName("id")] string? Id,
	[property: JsonPropertyName("email")] string? Email,
	[property: JsonPropertyName("firstName")] string? FirstName,
	[property: JsonPropertyName("lastName")] string? LastName,
	[property: JsonPropertyName("enabled")] bool Enabled);
