using System.Diagnostics;
using Serilog.Context;

namespace Cryologistics21.Gateway.Middleware;

internal sealed class LogContextTraceLoggingMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		string? traceId = Activity.Current?.TraceId.ToString();
		using (LogContext.PushProperty("TraceId", traceId))
		{
			await next.Invoke(context);
		}
	}
}
