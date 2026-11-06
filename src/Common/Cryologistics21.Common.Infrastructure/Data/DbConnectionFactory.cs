using System.Data.Common;
using Cryologistics21.Common.Application.Data;
using Npgsql;

namespace Cryologistics21.Common.Infrastructure.Data;

public sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
	public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
	{
		return await dataSource.OpenConnectionAsync(cancellationToken);
	}
}
