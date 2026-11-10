using Cryologistics21.Services.Users.Domain.Users;
using Cryologistics21.Services.Users.Infrastructure.Database;
using Cryologistics21.Services.Users.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cryologistics21.Services.Users.Infrastructure.Users;

internal sealed class RealmUserSeeder(
  KeyCloakClient keyCloakClient,
  UsersDbContext dbContext,
  ILogger<RealmUserSeeder> logger) : IRealmUserSeeder
{
	public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
	{
		IReadOnlyCollection<KeyCloakRealmUser> realmUsers = await keyCloakClient.GetUsersAsync(cancellationToken);
		KeyCloakRealmUser[] usersToSeed = [.. realmUsers.Where(user =>
			user.Enabled
			&& !string.IsNullOrWhiteSpace(user.Id)
			&& !string.IsNullOrWhiteSpace(user.Email)
			&& !string.IsNullOrWhiteSpace(user.FirstName)
			&& !string.IsNullOrWhiteSpace(user.LastName))];

		if (usersToSeed.Length == 0)
		{
			return 0;
		}

		string[] identityIds = [.. usersToSeed.Select(user => user.Id!)];
		string[] emails = [.. usersToSeed.Select(user => user.Email!)];
		List<ExistingUser> existingUsers = await dbContext.Users
		  .AsNoTracking()
		  .Where(user => identityIds.Contains(user.IdentityId) || emails.Contains(user.Email))
		  .Select(user => new ExistingUser(user.IdentityId, user.Email))
		  .ToListAsync(cancellationToken);

		var existingIdentityIds = existingUsers.Select(user => user.IdentityId).ToHashSet(StringComparer.Ordinal);
		var existingEmails = existingUsers.Select(user => user.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
		int addedCount = 0;

		foreach (KeyCloakRealmUser realmUser in usersToSeed)
		{
			if (existingIdentityIds.Contains(realmUser.Id!) || existingEmails.Contains(realmUser.Email!))
			{
				continue;
			}

			var user = User.Create(
				realmUser.Email!,
				realmUser.FirstName!,
				realmUser.LastName!,
				realmUser.Id!);

			foreach (Role role in user.Roles)
			{
				dbContext.Attach(role);
			}

			dbContext.Users.Add(user);

			existingIdentityIds.Add(realmUser.Id!);
			existingEmails.Add(realmUser.Email!);
			addedCount++;
		}

		if (addedCount > 0)
		{
			await dbContext.SaveChangesAsync(cancellationToken);
			if (logger.IsEnabled(LogLevel.Information))
			{
				logger.LogInformation("Seeded {UserCount} users from the Keycloak realm", addedCount);
			}
		}

		return addedCount;
	}

	private sealed record ExistingUser(string IdentityId, string Email);
}
