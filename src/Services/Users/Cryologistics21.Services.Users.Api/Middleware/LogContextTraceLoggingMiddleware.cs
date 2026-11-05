using System.Diagnostics;
using Serilog.Context;

namespace Cryologistics21.Services.Users.Api.Middleware;

internal sealed class LogContextTraceLoggingMiddleware(RequestDelegate next)
{
	public Task Invoke(HttpContext context)
	{
		string traceId = Activity.Current?.TraceId.ToString();

		using (LogContext.PushProperty("TraceId", traceId))
		{
			return next.Invoke(context);
		}
	}
}
