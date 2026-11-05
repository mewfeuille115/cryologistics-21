using Confluent.Kafka;
using Cryologistics21.Common.Application.Caching;
using Cryologistics21.Common.Application.Clock;
using Cryologistics21.Common.Application.Data;
using Cryologistics21.Common.Application.EventBus;
using Cryologistics21.Common.Infrastructure.Authentication;
using Cryologistics21.Common.Infrastructure.Authorization;
using Cryologistics21.Common.Infrastructure.Caching;
using Cryologistics21.Common.Infrastructure.Clock;
using Cryologistics21.Common.Infrastructure.Data;
using Cryologistics21.Common.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Quartz;
using StackExchange.Redis;

namespace Cryologistics21.Common.Infrastructure;

public static class InfrastructureConfiguration
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		string databaseConnectionString,
		string redisConnectionString,
		string kafkaBootstrapServers)
	{
		services.AddAuthenticationInternal();

		services.AddAuthorizationInternal();

		NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
		services.TryAddSingleton(npgsqlDataSource);

		services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

		services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

		services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

		services.AddQuartz(quartz =>
		{
			quartz.ConfigureScheduler(options =>
			{
				options.InstanceName = "Cryologistics21";
				options.GenerateInstanceId = true;
			});
		});
		services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

		try
		{
			IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
			services.TryAddSingleton(connectionMultiplexer);

			services.AddStackExchangeRedisCache(options =>
			{
				options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
			});
		}
		catch (RedisConnectionException)
		{
			services.AddDistributedMemoryCache();
		}

		services.TryAddSingleton<ICacheService, CacheService>();

		services.TryAddScoped<IEventBus, EventBus.EventBus>();

		services.TryAddSingleton<IProducer<string, string>>(sp =>
		{
			ProducerConfig config = new()
			{
				BootstrapServers = kafkaBootstrapServers
			};
			return new ProducerBuilder<string, string>(config).Build();
		});

		return services;
	}
}
