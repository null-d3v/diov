using Microsoft.Data.SqlClient;

namespace Diov.Data;

public interface IDbConnectionFactory
{
    public string Connection { get; }

    Task<SqlConnection> GetSqlConnectionAsync(
        CancellationToken cancellationToken = default);
}