using System;
using System.Threading.Tasks;
using Dapper;

namespace Diov.Data
{
    public class AdminAuthorizationRepository : IAdminAuthorizationRepository
    {
        private const string SelectStatement =
            @"SELECT
                [AccountId],
                [Id],
                [IdentityProvider]
            FROM [AdminAuthorization]
            WHERE [AccountId] = @AccountId AND
                [IdentityProvider] = @IdentityProvider";

        public AdminAuthorizationRepository(
            IDbConnectionFactory dbConnectionFactory)
        {
            DbConnectionFactory = dbConnectionFactory ??
                throw new ArgumentNullException(
                    nameof(dbConnectionFactory));
        }

        public IDbConnectionFactory DbConnectionFactory { get; }

        public async Task<AdminAuthorization> GetAdminAuthorizationAsync(
            string accountId,
            string identityProvider)
        {
            using var sqlConnection = await DbConnectionFactory
                .GetSqlConnectionAsync();
            var adminAuthorization = await sqlConnection
                .QueryFirstOrDefaultAsync<AdminAuthorization>(
                    SelectStatement,
                    new
                    {
                        AccountId = accountId,
                        IdentityProvider = identityProvider
                            .ToLowerInvariant(),
                    });

            return adminAuthorization;
        }
    }
}