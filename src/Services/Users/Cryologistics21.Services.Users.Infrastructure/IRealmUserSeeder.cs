namespace Cryologistics21.Services.Users.Infrastructure;

public interface IRealmUserSeeder
{
	Task<int> SeedAsync(CancellationToken cancellationToken = default);
}
