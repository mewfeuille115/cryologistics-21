using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Cryologistics21.Services.Users.Infrastructure.Identity;

internal sealed partial class KeyCloakClient(HttpClient httpClient, IOptions<KeyCloakOptions> options)
{
	private readonly KeyCloakOptions _options = options.Value;

	internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
			"users",
			user,
			cancellationToken);

		httpResponseMessage.EnsureSuccessStatusCode();

		return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
	}

	private static string ExtractIdentityIdFromLocationHeader(HttpResponseMessage httpResponseMessage)
	{
		const string usersSegmentName = "users/";

		string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

		if (locationHeader is null)
		{
			throw new InvalidOperationException("Location header is null");
		}

		int userSegmentValueIndex = locationHeader.IndexOf(
			usersSegmentName,
			StringComparison.InvariantCultureIgnoreCase);

		string identityId = locationHeader[(userSegmentValueIndex + usersSegmentName.Length)..];

		return identityId;
	}

	internal async Task<UserTokenResponse> AccessTokenAsync(UserLogin user, CancellationToken cancellationToken = default)
	{
		var requestParameters = new KeyValuePair<string, string>[]
		{
			new("grant_type", "password"),
			new("client_id", _options.PublicClientId),
			new("client_secret", _options.PublicClientSecret),
			new("username", user.Username),
			new("password", user.Password)
		};

		using var requestContent = new FormUrlEncodedContent(requestParameters);
		using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
		{
			Content = requestContent
		};

		using HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(request, cancellationToken);
		httpResponseMessage.EnsureSuccessStatusCode();

		UserTokenResponse? tokenResponse = await httpResponseMessage.Content
			.ReadFromJsonAsync<UserTokenResponse>(cancellationToken: cancellationToken);

		if (tokenResponse is null)
		{
			throw new InvalidOperationException("Token response is null");
		}

		return tokenResponse;
	}
}
