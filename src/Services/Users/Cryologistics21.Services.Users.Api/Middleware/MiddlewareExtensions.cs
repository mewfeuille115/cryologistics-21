namespace Cryologistics21.Services.Users.Api.Middleware;

internal static class MiddlewareExtensions
{
	internal static IApplicationBuilder UseLogContextTraceLogging(this IApplicationBuilder app)
	{
		app.UseMiddleware<LogContextTraceLoggingMiddleware>();

		return app;
	}
}
