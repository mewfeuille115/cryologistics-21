using Cryologistics21.Services.Users.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;
using Testcontainers.Kafka;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Cryologistics21.Services.Users.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	private static readonly IntegrationTestSecrets Secrets = LoadSecrets();

	private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18.6-alpine3.23")
		.WithDatabase("cryologistics21")
		.WithUsername("postgres")
		.WithPassword("postgres")
		.Build();

	private readonly RedisContainer _redisContainer = new RedisBuilder("redis:8.4.0").Build();

	private readonly KafkaContainer _kafkaContainer = new KafkaBuilder("confluentinc/confluent-local:8.4.0").Build();

	private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder("quay.io/keycloak/keycloak:26.8")
		.WithResourceMapping(
			GetRealmFile(),
			new FileInfo("/opt/keycloak/data/import/realm.json"))
		.WithEnvironment("CRYOLOGISTICS21_API_CLIENT_SECRET", Secrets.ApiClientSecret)
		.WithEnvironment("CRYOLOGISTICS21_ADMIN_CLIENT_SECRET", Secrets.AdminClientSecret)
		.WithEnvironment("DEMO_ADMIN_PASSWORD", Secrets.DemoAdminPassword)
		.WithEnvironment("DEMO_OPERATOR_PASSWORD", Secrets.DemoOperatorPassword)
		.WithEnvironment("DEMO_QA_PASSWORD", Secrets.DemoQaPassword)
		.WithCommand("--import-realm")
		.Build();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		Environment.SetEnvironmentVariable("ConnectionStrings:users-db", _dbContainer.GetConnectionString());
		Environment.SetEnvironmentVariable("ConnectionStrings:Cache", _redisContainer.GetConnectionString());
		Environment.SetEnvironmentVariable("ConnectionStrings:Queue", _kafkaContainer.GetBootstrapAddress());

		string keycloakAddress = _keycloakContainer.GetBaseAddress();
		string keyCloakRealmUrl = $"{keycloakAddress}realms/cryologistics21";

		Environment.SetEnvironmentVariable("Authentication:Audience", "cryologistics-21");
		Environment.SetEnvironmentVariable(
			"Authentication:MetadataAddress",
			$"{keyCloakRealmUrl}/.well-known/openid-configuration");
		Environment.SetEnvironmentVariable(
			"Authentication:TokenValidationParameters:ValidIssuers:0",
			keyCloakRealmUrl);

		builder.ConfigureTestServices(services =>
		{
			services.Configure<KeyCloakOptions>(o =>
			{
				o.AdminUrl = $"{keycloakAddress}admin/realms/cryologistics21/";
				o.TokenUrl = $"{keyCloakRealmUrl}/protocol/openid-connect/token";
				o.ConfidentialClientId = "cryologistics-21-admin";
				o.ConfidentialClientSecret = Secrets.AdminClientSecret;
				o.PublicClientId = "cryologistics-21";
				o.PublicClientSecret = Secrets.ApiClientSecret;
			});
		});
	}

	public async ValueTask InitializeAsync()
	{
		await _dbContainer.StartAsync();
		await _redisContainer.StartAsync();
		await _kafkaContainer.StartAsync();
		await _keycloakContainer.StartAsync();
	}

	public new async ValueTask DisposeAsync()
	{
		await base.DisposeAsync();
		await _dbContainer.DisposeAsync();
		await _redisContainer.DisposeAsync();
		await _kafkaContainer.DisposeAsync();
		await _keycloakContainer.DisposeAsync();
	}

	private static FileInfo GetRealmFile()
	{
		string realmPath = Path.Combine(GetSolutionRoot().FullName, ".files", "cryologistics21-realm.json");

		if (!File.Exists(realmPath))
		{
			throw new FileNotFoundException("Could not locate the Keycloak realm import file.", realmPath);
		}

		return new FileInfo(realmPath);
	}

	private static IntegrationTestSecrets LoadSecrets()
	{
		string envFile = Path.Combine(GetSolutionRoot().FullName, ".env");
		if (File.Exists(envFile))
		{
			Env.Load(envFile);
		}

		return new IntegrationTestSecrets(
			RequireEnvironmentVariable("CRYOLOGISTICS21_API_CLIENT_SECRET"),
			RequireEnvironmentVariable("CRYOLOGISTICS21_ADMIN_CLIENT_SECRET"),
			RequireEnvironmentVariable("DEMO_ADMIN_PASSWORD"),
			RequireEnvironmentVariable("DEMO_OPERATOR_PASSWORD"),
			RequireEnvironmentVariable("DEMO_QA_PASSWORD"));
	}

	private static string RequireEnvironmentVariable(string name) =>
		Environment.GetEnvironmentVariable(name)
		?? throw new InvalidOperationException($"Environment variable '{name}' is not set.");

	private static DirectoryInfo GetSolutionRoot()
	{
		DirectoryInfo? directory = new(AppContext.BaseDirectory);

		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Cryologistics21.slnx")))
		{
			directory = directory.Parent;
		}

		return directory
			?? throw new DirectoryNotFoundException("Could not locate the Cryologistics21 solution root.");
	}

	private sealed record IntegrationTestSecrets(
		string ApiClientSecret,
		string AdminClientSecret,
		string DemoAdminPassword,
		string DemoOperatorPassword,
		string DemoQaPassword);
}
