using Cryologistics21.Common.Application;
using Cryologistics21.Common.Infrastructure;
using Cryologistics21.Common.Infrastructure.Configuration;
using Cryologistics21.Common.Presentation.Endpoints;
using Cryologistics21.Common.ServiceDefaults;
using Cryologistics21.Services.Users.Api.Extensions;
using Cryologistics21.Services.Users.Api.Middleware;
using Cryologistics21.Services.Users.Infrastructure;
using JasperFx.Resources;
using Serilog;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.Postgresql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSeqEndpoint("Seq");

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddApplication([
	Cryologistics21.Services.Users.Application.AssemblyReference.Assembly,
]);

string databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("users-db");
string redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache");
string kafkaBootstrapServers = builder.Configuration.GetConnectionStringOrThrow("Queue");

builder.Services.AddInfrastructure(
	databaseConnectionString,
	redisConnectionString,
	kafkaBootstrapServers);

builder.Host.UseWolverine(options =>
{
	options.PersistMessagesWithPostgresql(databaseConnectionString);

	options.UseKafka(kafkaBootstrapServers)
		.AutoProvision();

	options.Services.AddResourceSetupOnStartup();

	options.Policies.DisableConventionalLocalRouting();

	options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;
	options.Durability.MessageIdentity = MessageIdentity.IdAndDestination;
	options.Durability.Mode = DurabilityMode.Solo;
});

builder.Services.AddUsersModule(builder.Configuration);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

	await app.ApplyMigrationsAsync();
}

app.UseExceptionHandler();

app.UseLogContextTraceLogging();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

app.MapEndpoints();

await app.RunAsync();
