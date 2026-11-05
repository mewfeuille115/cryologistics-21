IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RedisResource> redis = builder.AddRedis("Cache")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<KafkaServerResource> kafka = builder.AddKafka("Queue")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<SeqResource> seq = builder.AddSeq("Seq");

IResourceBuilder<KeycloakResource> keycloak = builder.AddKeycloak("identity", 18080)
		.WithDataVolume()
		.WithLifetime(ContainerLifetime.Persistent);

#pragma warning disable S125 // Sections of code should not be commented out
//.WithRealmImport(Path.Combine(builder.AppHostDirectory, ".files"));
#pragma warning restore S125 // Sections of code should not be commented out

IResourceBuilder<PostgresServerResource> usersPostgres = builder.AddPostgres("users-postgres")
	.WithDataVolume()
	.WithLifetime(ContainerLifetime.Persistent);

IResourceBuilder<PostgresDatabaseResource> usersDb = usersPostgres.AddDatabase("users-db");

builder.AddProject<Projects.Cryologistics21_Services_Users_Api>("users-api")
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

await builder.Build().RunAsync();
