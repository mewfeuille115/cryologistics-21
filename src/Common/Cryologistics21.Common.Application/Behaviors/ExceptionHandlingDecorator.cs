using Cryologistics21.Common.Application.Exceptions;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Cryologistics21.Common.Application.Behaviors;

internal static partial class ExceptionHandlingDecorator
{
	internal sealed class CommandHandler<TCommand>(
			ICommandHandler<TCommand> inner,
			ILogger<CommandHandler<TCommand>> logger
		) : ICommandHandler<TCommand>
		where TCommand : ICommand
	{
		public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
		{
			try
			{
				return await inner.Handle(command, cancellationToken);
			}
			catch (Exception exception)
			{
				LogUnhandledException(
					logger,
					exception,
					typeof(TCommand).Name);

				throw new Cryologistics21Exception(typeof(TCommand).Name, innerException: exception);
			}
		}
	}

	internal sealed class CommandHandlerT<TCommand, TResponse>(
			ICommandHandler<TCommand, TResponse> inner,
			ILogger<CommandHandlerT<TCommand, TResponse>> logger
		) : ICommandHandler<TCommand, TResponse>
	   where TCommand : ICommand<TResponse>
	{
		public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
		{
			try
			{
				return await inner.Handle(command, cancellationToken);
			}
			catch (Exception exception)
			{
				LogUnhandledException(
					logger,
					exception,
					typeof(TCommand).Name);

				throw new Cryologistics21Exception(typeof(TCommand).Name, innerException: exception);
			}
		}
	}

	internal sealed class QueryHandler<TQuery, TResponse>(
			IQueryHandler<TQuery, TResponse> inner,
			ILogger<QueryHandler<TQuery, TResponse>> logger
		) : IQueryHandler<TQuery, TResponse>
		where TQuery : IQuery<TResponse>
	{
		public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
		{
			try
			{
				return await inner.Handle(query, cancellationToken);
			}
			catch (Exception exception)
			{
				LogUnhandledException(
					logger,
					exception,
					typeof(TQuery).Name);

				throw new Cryologistics21Exception(typeof(TQuery).Name, innerException: exception);
			}
		}
	}

	[LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for {HandlerName}.")]
	private static partial void LogUnhandledException(
	   ILogger logger,
	   Exception exception,
	   string handlerName);
}
