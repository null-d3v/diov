using Microsoft.Data.SqlClient;

namespace Diov.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    public DbConnectionFactory(string connection)
    {
        Connection = connection;
    }

    public string Connection { get; }

    public async Task<SqlConnection> GetSqlConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var sqlConnection = new SqlConnection(Connection);
        await sqlConnection.OpenAsync(cancellationToken);
        return sqlConnection;
    }
}
