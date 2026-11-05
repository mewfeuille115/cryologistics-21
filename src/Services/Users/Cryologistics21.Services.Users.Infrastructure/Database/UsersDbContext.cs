using Cryologistics21.Common.Infrastructure.Inbox;
using Cryologistics21.Common.Infrastructure.Outbox;
using Cryologistics21.Services.Users.Application.Abstractions.Data;
using Cryologistics21.Services.Users.Domain.Users;
using Cryologistics21.Services.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace Cryologistics21.Services.Users.Infrastructure.Database;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options), IUnitOfWork
{
	internal DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(Schemas.Users);

		modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
		modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
		modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
		modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());

		modelBuilder.ApplyConfiguration(new PermissionConfiguration());
		modelBuilder.ApplyConfiguration(new RoleConfiguration());
		modelBuilder.ApplyConfiguration(new UserConfiguration());
	}
}
