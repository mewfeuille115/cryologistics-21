using System.Reflection;
using Cryologistics21.Common.Application.Behaviors;
using Cryologistics21.Common.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Cryologistics21.Common.Application;

public static class ApplicationConfiguration
{
	public static IServiceCollection AddApplication(this IServiceCollection services, Assembly[] moduleAssemblies)
	{
		services.Scan(scan => scan.FromAssemblies(moduleAssemblies)
			.AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
				.AsImplementedInterfaces()
				.WithScopedLifetime()
			.AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
				.AsImplementedInterfaces()
				.WithScopedLifetime()
			.AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
				.AsImplementedInterfaces()
				.WithScopedLifetime());

		// Decorators run in reverse order of registration
		// Register: Validation -> Logging -> ExceptionHandling
		// Runtime: ExceptionHandling -> Logging -> Validation

		bool hasCommandHandlerT = services.Any(s => s.ServiceType.IsGenericType &&
			s.ServiceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));

		bool hasCommandHandler = services.Any(s => s.ServiceType.IsGenericType &&
			s.ServiceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>));

		bool hasQueryHandler = services.Any(s => s.ServiceType.IsGenericType &&
			s.ServiceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

		if (hasCommandHandlerT)
		{
			services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandlerT<,>));
			services.Decorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandlerT<,>));
			services.Decorate(typeof(ICommandHandler<,>), typeof(ExceptionHandlingDecorator.CommandHandlerT<,>));
		}

		if (hasCommandHandler)
		{
			services.Decorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandHandler<>));
			services.Decorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandler<>));
			services.Decorate(typeof(ICommandHandler<>), typeof(ExceptionHandlingDecorator.CommandHandler<>));
		}

		if (hasQueryHandler)
		{
			services.Decorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
			services.Decorate(typeof(IQueryHandler<,>), typeof(ExceptionHandlingDecorator.QueryHandler<,>));
		}

		services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);

		return services;
	}
}
