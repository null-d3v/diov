using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Diov.Data
{
    public interface IDbConnectionFactory
    {
        string Connection { get; }

        Task<SqlConnection> GetSqlConnectionAsync();
    }
}