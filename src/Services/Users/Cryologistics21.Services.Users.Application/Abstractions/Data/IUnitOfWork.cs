namespace Cryologistics21.Services.Users.Application.Abstractions.Data;

public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
