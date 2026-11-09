using Cryologistics21.Gateway.Authentication;
using Cryologistics21.Gateway.Middleware;
using Cryologistics21.Gateway.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddSeqEndpoint("Seq");

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddServiceDiscovery();

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
	.AddServiceDiscoveryDestinationResolver();

builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService(DiagnosticsConfig.ServiceName))
	.WithTracing(tracing =>
	{
		tracing
			.AddAspNetCoreInstrumentation()
			.AddHttpClientInstrumentation()
			.AddSource("Yarp.ReverseProxy");

		tracing.AddOtlpExporter();
	});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();

WebApplication app = builder.Build();

app.UseLogContextTraceLogging();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

app.MapReverseProxy();

await app.RunAsync();
