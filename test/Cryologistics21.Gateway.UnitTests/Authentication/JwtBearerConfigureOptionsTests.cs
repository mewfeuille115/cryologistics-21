using Cryologistics21.Gateway.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace Cryologistics21.Gateway.UnitTests.Authentication;

public sealed class JwtBearerConfigureOptionsTests
{
	[Fact]
	public void Configure_ShouldBindAuthenticationSection()
	{
		IConfigurationRoot configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Authentication:Audience"] = "cryologistics-21",
				["Authentication:MetadataAddress"] = "http://localhost/.well-known/openid-configuration",
				["Authentication:RequireHttpsMetadata"] = "false",
				["Authentication:TokenValidationParameters:ValidIssuers:0"] = "https://issuer"
			})
			.Build();
		var sut = new JwtBearerConfigureOptions(configuration);
		var options = new JwtBearerOptions();

		sut.Configure(JwtBearerDefaults.AuthenticationScheme, options);

		Assert.Equal("cryologistics-21", options.Audience);
		Assert.False(options.RequireHttpsMetadata);
		Assert.Equal("http://localhost/.well-known/openid-configuration", options.MetadataAddress);
		Assert.Contains("https://issuer", options.TokenValidationParameters.ValidIssuers);
	}
}
