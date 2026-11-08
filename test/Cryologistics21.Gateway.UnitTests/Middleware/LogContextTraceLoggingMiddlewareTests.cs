using System.Diagnostics;
using Cryologistics21.Gateway.Middleware;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Cryologistics21.Gateway.UnitTests.Middleware;

public sealed class LogContextTraceLoggingMiddlewareTests
{
	[Fact]
	public async Task Invoke_ShouldKeepTraceIdInLogContext_AfterAsyncContinuationAsync()
	{
		var sink = new CollectingSink();
		using Logger logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(sink).CreateLogger();
		using Activity activity = new("test");
		activity.Start();
		var middleware = new LogContextTraceLoggingMiddleware(async _ =>
		{
			await Task.Yield();
			logger.Information("inside pipeline");
		});

		await middleware.InvokeAsync(new DefaultHttpContext());

		LogEvent logEvent = Assert.Single(sink.Events);
		Assert.True(logEvent.Properties.ContainsKey("TraceId"));
		Assert.Contains(activity.TraceId.ToString(), logEvent.Properties["TraceId"].ToString());
	}

	[Fact]
	public async Task Invoke_ShouldCallNextAsync()
	{
		bool called = false;
		var middleware = new LogContextTraceLoggingMiddleware(_ =>
		{
			called = true;
			return Task.CompletedTask;
		});

		await middleware.InvokeAsync(new DefaultHttpContext());

		Assert.True(called);
	}

	private sealed class CollectingSink : ILogEventSink
	{
		public List<LogEvent> Events { get; } = [];

		public void Emit(LogEvent logEvent) => Events.Add(logEvent);
	}
}
