using Microsoft.Data.SqlClient;

namespace Diov.Data;

public class DbConnectionFactory(
    string connection) :
    IDbConnectionFactory
{
    public string Connection { get; } =
        connection;

    public async Task<SqlConnection> GetSqlConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var sqlConnection = new SqlConnection(Connection);
        await sqlConnection
            .OpenAsync(cancellationToken)
            .ConfigureAwait(false);
        return sqlConnection;
    }
}
