using System.Diagnostics;
using Cryologistics21.Common.Application.Messaging;
using Cryologistics21.Common.Domain;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Cryologistics21.Common.Application.Behaviors;

internal static partial class LoggingDecorator
{
	private static string GetModuleName(string requestName) => requestName.Split('.')[2];

	internal sealed partial class CommandHandler<TCommand>(
			ICommandHandler<TCommand> inner,
			ILogger<CommandHandler<TCommand>> logger
		) : ICommandHandler<TCommand>
		where TCommand : ICommand
	{
		public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
		{
			string moduleName = GetModuleName(typeof(TCommand).FullName!);
			string commandName = typeof(TCommand).Name;

			Activity.Current?.SetTag("request.module", moduleName);
			Activity.Current?.SetTag("request.name", commandName);

			LogProcessingCommand(commandName);

			Result result = await inner.Handle(command, cancellationToken);

			if (result.IsSuccess)
			{
				LogCompletedCommand(commandName);
			}
			else
			{
				using (LogContext.PushProperty("Error", result.Error, true))
				{
					LogCompletedCommandtWithError(commandName);
				}
			}

			return result;
		}

		[LoggerMessage(Level = LogLevel.Information, Message = "Processing command {CommandName}")]
		private partial void LogProcessingCommand(string commandName);

		[LoggerMessage(Level = LogLevel.Information, Message = "Completed command {CommandName}")]
		private partial void LogCompletedCommand(string commandName);

		[LoggerMessage(Level = LogLevel.Error, Message = "Completed command {CommandName} with error")]
		private partial void LogCompletedCommandtWithError(string commandName);
	}

	internal sealed partial class CommandHandlerT<TCommand, TResponse>(
			ICommandHandler<TCommand, TResponse> inner,
			ILogger<CommandHandlerT<TCommand, TResponse>> logger
		) : ICommandHandler<TCommand, TResponse>
		where TCommand : ICommand<TResponse>
	{
		public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
		{
			string moduleName = GetModuleName(typeof(TCommand).FullName!);
			string commandName = typeof(TCommand).Name;

			Activity.Current?.SetTag("request.module", moduleName);
			Activity.Current?.SetTag("request.name", commandName);

			LogProcessingCommand(commandName);

			Result<TResponse> result = await inner.Handle(command, cancellationToken);

			if (result.IsSuccess)
			{
				LogCompletedCommand(commandName);
			}
			else
			{
				using (LogContext.PushProperty("Error", result.Error, true))
				{
					LogCompletedCommandtWithError(commandName);
				}
			}

			return result;
		}

		[LoggerMessage(Level = LogLevel.Information, Message = "Processing command {CommandName}")]
		private partial void LogProcessingCommand(string commandName);

		[LoggerMessage(Level = LogLevel.Information, Message = "Completed command {CommandName}")]
		private partial void LogCompletedCommand(string commandName);

		[LoggerMessage(Level = LogLevel.Error, Message = "Completed command {CommandName} with error")]
		private partial void LogCompletedCommandtWithError(string commandName);
	}

	internal sealed partial class QueryHandler<TQuery, TResponse>(
			IQueryHandler<TQuery, TResponse> innerHandler,
			ILogger<QueryHandler<TQuery, TResponse>> logger
		) : IQueryHandler<TQuery, TResponse>
		where TQuery : IQuery<TResponse>
	{
		public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
		{
			string moduleName = GetModuleName(typeof(TQuery).FullName!);
			string queryName = typeof(TQuery).Name;

			Activity.Current?.SetTag("request.module", moduleName);
			Activity.Current?.SetTag("request.name", queryName);

			LogProcessingQuery(queryName);

			Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

			if (result.IsSuccess)
			{
				LogCompletedQuery(queryName);
			}
			else
			{
				using (LogContext.PushProperty("Error", result.Error, true))
				{
					LogCompletedQueryWithError(queryName);
				}
			}

			return result;
		}

		[LoggerMessage(Level = LogLevel.Information, Message = "Processing query {QueryName}")]
		private partial void LogProcessingQuery(string queryName);

		[LoggerMessage(Level = LogLevel.Information, Message = "Completed query {QueryName}")]
		private partial void LogCompletedQuery(string queryName);

		[LoggerMessage(Level = LogLevel.Error, Message = "Completed query {QueryName} with error")]
		private partial void LogCompletedQueryWithError(string queryName);
	}
}
