IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

string repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", ".."));
string envFile = Path.Combine(repoRoot, ".env");

if (File.Exists(envFile))
{
	DotNetEnv.Env.Load(envFile);
}

IResourceBuilder<RedisResource> redis = builder.AddRedis("Cache")
	.WithImageTag("8")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<KafkaServerResource> kafka = builder.AddKafka("Queue")
	.WithImageTag("8.4.0")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<SeqResource> seq = builder.AddSeq("Seq")
	.WithImageTag("2026");

string apiClientSecret = RequireEnv("CRYOLOGISTICS21_API_CLIENT_SECRET");
string adminClientSecret = RequireEnv("CRYOLOGISTICS21_ADMIN_CLIENT_SECRET");
string demoAdminPassword = RequireEnv("DEMO_ADMIN_PASSWORD");
string demoOperatorPassword = RequireEnv("DEMO_OPERATOR_PASSWORD");
string demoQaPassword = RequireEnv("DEMO_QA_PASSWORD");

IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak("identity", 18080)
	.WithImageTag("26.8")
	.WithDataVolume()
		.WithRealmImport(Path.Combine(repoRoot, ".files"))
	.WithEnvironment("CRYOLOGISTICS21_API_CLIENT_SECRET", apiClientSecret)
	.WithEnvironment("CRYOLOGISTICS21_ADMIN_CLIENT_SECRET", adminClientSecret)
	.WithEnvironment("DEMO_ADMIN_PASSWORD", demoAdminPassword)
	.WithEnvironment("DEMO_OPERATOR_PASSWORD", demoOperatorPassword)
	.WithEnvironment("DEMO_QA_PASSWORD", demoQaPassword)
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresServerResource> usersPostgres = builder.AddPostgres("users-postgres")
	.WithImageTag("18.6")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> usersDb = usersPostgres.AddDatabase("users-db");

IResourceBuilder<ProjectResource> usersApi = builder.AddProject<Projects.Cryologistics21_Services_Users_Api>("users-api")
	.WithEnvironment("Keycloak__ClientSecret", apiClientSecret)
	.WithEnvironment("Keycloak__AdminClientSecret", adminClientSecret)
	.WithReference(redis)
	.WithReference(kafka)
	.WithReference(seq)
	.WithReference(keycloak)
	.WithReference(usersDb)
	.WaitFor(redis)
	.WaitFor(kafka)
	.WaitFor(seq)
	.WaitFor(keycloak)
	.WaitFor(usersDb);

builder.AddViteApp("frontend", "../../UI/cryologistics21")
	.WithReference(usersApi)
	.WaitFor(usersApi)
	.WithEnvironment("NODE_ENV", "development");

await builder.Build().RunAsync();

static string RequireEnv(string name) =>
	Environment.GetEnvironmentVariable(name)
		?? throw new InvalidOperationException($"Environment variable '{name}' is not set.");
