using DbUp;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Diov.Data;

public class MigrationHostedService : IHostedService
{
    public MigrationHostedService(
        IDbConnectionFactory dbConnectionFactory,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        DbConnectionFactory = dbConnectionFactory;
        HostApplicationLifetime = hostApplicationLifetime;
    }

    public IDbConnectionFactory DbConnectionFactory { get; }
    public IHostApplicationLifetime HostApplicationLifetime { get; }

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        EnsureDatabase.For.SqlDatabase(
            DbConnectionFactory.Connection);

        var upgradeEngine = DeployChanges.To
            .SqlDatabase(DbConnectionFactory.Connection)
            .JournalToSqlTable("dbo", "Migration")
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly())
            .WithTransactionPerScript()
            .LogToAutodetectedLog()
            .Build();

        if (upgradeEngine.IsUpgradeRequired())
        {
            try
            {
                upgradeEngine.PerformUpgrade();
            }
            catch (Exception)
            {
                HostApplicationLifetime.StopApplication();
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
