using System.Net;
using Cryologistics21.Gateway.IntegrationTests.Abstractions;

namespace Cryologistics21.Gateway.IntegrationTests.Routing;

public sealed class UsersRoutingTests(IntegrationTestWebAppFactory factory) : IClassFixture<IntegrationTestWebAppFactory>
{
	[Fact]
	public async Task ProtectedRoute_WithoutToken_ShouldReturnUnauthorizedAsync()
	{
		HttpClient client = factory.CreateClient();

		HttpResponseMessage response = await client.GetAsync("/users/profile", CancellationToken.None);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task RegisterRoute_WithoutToken_ShouldNotRequireAuthenticationAsync()
	{
		HttpClient client = factory.CreateClient();

		HttpResponseMessage response = await client.PostAsync("/users/register", null, CancellationToken.None);

		Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}
}
