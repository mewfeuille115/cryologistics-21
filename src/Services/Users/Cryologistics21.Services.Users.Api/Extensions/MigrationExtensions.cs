using Cryologistics21.Services.Users.Infrastructure;
using Cryologistics21.Services.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Cryologistics21.Services.Users.Api.Extensions;

internal static class MigrationExtensions
{
	internal static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
	{
		using IServiceScope scope = app.ApplicationServices.CreateScope();

		await ApplyMigrationAsync<UsersDbContext>(scope);
		await scope.ServiceProvider.GetRequiredService<IRealmUserSeeder>().SeedAsync();
	}

	private static async Task ApplyMigrationAsync<TDbContext>(IServiceScope scope)
		where TDbContext : DbContext
	{
		TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

		await context.Database.MigrateAsync();
	}
}
