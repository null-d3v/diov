using Microsoft.Data.SqlClient;

namespace Diov.Data;

public interface IDbConnectionFactory
{
    string Connection { get; }

    Task<SqlConnection> GetSqlConnectionAsync(
        CancellationToken cancellationToken = default);
}