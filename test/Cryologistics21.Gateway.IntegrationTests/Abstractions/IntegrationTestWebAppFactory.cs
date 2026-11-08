using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cryologistics21.Gateway.IntegrationTests.Abstractions;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");
		builder.UseSetting("ConnectionStrings:Seq", "http://localhost:5341");
		builder.UseSetting("Authentication:Audience", "test-audience");
		builder.UseSetting("Authentication:MetadataAddress", "http://localhost:1/.well-known/openid-configuration");
		builder.UseSetting("Authentication:TokenValidationParameters:ValidIssuers:0", "https://test-issuer");
	}
}
