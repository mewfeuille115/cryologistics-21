using System.Data.Common;

namespace Cryologistics21.Common.Application.Data;

public interface IDbConnectionFactory
{
	ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
