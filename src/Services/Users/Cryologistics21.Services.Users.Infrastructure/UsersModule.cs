using Cryologistics21.Common.Application.Authorization;
using Cryologistics21.Common.Application.EventBus;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Infrastructure.Outbox;
using Cryologistics21.Common.Presentation.Endpoints;
using Cryologistics21.Services.Users.Application.Abstractions.Data;
using Cryologistics21.Services.Users.Application.Abstractions.Identity;
using Cryologistics21.Services.Users.Domain.Users;
using Cryologistics21.Services.Users.Infrastructure.Authorization;
using Cryologistics21.Services.Users.Infrastructure.Database;
using Cryologistics21.Services.Users.Infrastructure.Identity;
using Cryologistics21.Services.Users.Infrastructure.Inbox;
using Cryologistics21.Services.Users.Infrastructure.Outbox;
using Cryologistics21.Services.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Quartz;

namespace Cryologistics21.Services.Users.Infrastructure;

public static class UsersModule
{
	public static IServiceCollection AddUsersModule(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		services.AddDomainEventHandlers();

		services.AddIntegrationEventHandlers();

		services.AddInfrastructure(configuration);

		services.AddEndpoints(Presentation.AssemblyReference.Assembly);

		return services;
	}

	private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddScoped<IPermissionService, PermissionService>();

		services.Configure<KeyCloakOptions>(configuration.GetSection("KeyCloak"));

		services.AddTransient<KeyCloakAuthDelegatingHandler>();

		services
			.AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
			{
				KeyCloakOptions keyCloakOptions = serviceProvider
					.GetRequiredService<IOptions<KeyCloakOptions>>().Value;

				httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
			})
			.AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

		services.AddTransient<IIdentityProviderService, IdentityProviderService>();
		services.AddScoped<IRealmUserSeeder, RealmUserSeeder>();

		services.AddDbContext<UsersDbContext>((sp, options) =>
			options
				.UseNpgsql(
					configuration.GetConnectionString("users-db"),
					npgsqlOptions => npgsqlOptions
						.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
				.AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>())
				.UseSnakeCaseNamingConvention());

		services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

		services.AddScoped<IUserRepository, UserRepository>();

		IConfigurationSection outboxSection = configuration.GetSection("Outbox");
		OutboxOptions outboxOptions = new();
		outboxSection.Bind(outboxOptions);
		services.Configure<OutboxOptions>(outboxSection);

		IConfigurationSection inboxSection = configuration.GetSection("Inbox");
		InboxOptions inboxOptions = new();
		inboxSection.Bind(inboxOptions);
		services.Configure<InboxOptions>(inboxSection);

		services.AddQuartz(quartz =>
		{
			ConfigureProcessOutboxJob.Configure(
				quartz,
				outboxOptions.IntervalInSeconds);

			ConfigureProcessInboxJob.Configure(
				quartz,
				inboxOptions.IntervalInSeconds);
		});
	}

	private static void AddDomainEventHandlers(this IServiceCollection services)
	{
		Type[] domainEventHandlers = [.. Application.AssemblyReference.Assembly
			.GetTypes()
			.Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
		];

		foreach (Type domainEventHandler in domainEventHandlers)
		{
			services.TryAddScoped(domainEventHandler);

			Type domainEvent = domainEventHandler
				.GetInterfaces()
				.Single(i => i.IsGenericType)
				.GetGenericArguments()
				.Single();

			Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

			services.Decorate(domainEventHandler, closedIdempotentHandler);
		}
	}

	private static void AddIntegrationEventHandlers(this IServiceCollection services)
	{
		Type[] integrationEventHandlers = [.. Presentation.AssemblyReference.Assembly
			.GetTypes()
			.Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
		];

		foreach (Type integrationEventHandler in integrationEventHandlers)
		{
			services.TryAddScoped(integrationEventHandler);

			Type integrationEvent = integrationEventHandler
				.GetInterfaces()
				.Single(i => i.IsGenericType)
				.GetGenericArguments()
				.Single();

			Type closedIdempotentHandler =
				typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

			services.Decorate(integrationEventHandler, closedIdempotentHandler);
		}
	}
}
