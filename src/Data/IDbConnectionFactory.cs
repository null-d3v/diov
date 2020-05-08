using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Diov.Data
{
    public interface IDbConnectionFactory
    {
        Task<SqlConnection> GetSqlConnectionAsync();
    }
}