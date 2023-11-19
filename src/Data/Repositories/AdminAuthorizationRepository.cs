using Dapper;

namespace Diov.Data;

public class AdminAuthorizationRepository(
    IDbConnectionFactory dbConnectionFactory) :
    IAdminAuthorizationRepository
{
    private const string SelectStatement =
        @"SELECT
            [AccountId],
            [Id],
            [IdentityProvider]
        FROM [AdminAuthorization]
        WHERE [AccountId] = @AccountId AND
            [IdentityProvider] = @IdentityProvider";

    public IDbConnectionFactory DbConnectionFactory { get; } =
        dbConnectionFactory;

    public async Task<AdminAuthorization?> GetAdminAuthorizationAsync(
        string accountId,
        string identityProvider,
        CancellationToken cancellationToken = default)
    {
        using var sqlConnection = await DbConnectionFactory
            .GetSqlConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        var adminAuthorization = await sqlConnection
            .QueryFirstOrDefaultAsync<AdminAuthorization?>(
                SelectStatement,
                new
                {
                    AccountId = accountId,
                    IdentityProvider = identityProvider
                        .ToLowerInvariant(),
                })
            .ConfigureAwait(false);

        return adminAuthorization;
    }
}
