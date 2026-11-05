using Microsoft.OpenApi;

namespace Cryologistics21.Services.Users.Api.Extensions;

internal static class SwaggerExtensions
{
	internal static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
	{
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Cryologistics21 Users API",
				Version = "v1",
				Description = "Cryologistics21 Users API - Manages user accounts and permissions for the Cryologistics21 system."
			});

			options.CustomSchemaIds(t => t.FullName?.Replace("+", "."));
		});

		return services;
	}
}
