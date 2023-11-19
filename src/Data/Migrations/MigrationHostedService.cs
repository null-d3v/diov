using DbUp;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Diov.Data;

public class MigrationHostedService(
    IDbConnectionFactory dbConnectionFactory,
    IHostApplicationLifetime hostApplicationLifetime) :
    IHostedService
{
    public IDbConnectionFactory DbConnectionFactory { get; } =
        dbConnectionFactory;
    public IHostApplicationLifetime HostApplicationLifetime { get; } =
        hostApplicationLifetime;

    [SuppressMessage("Design", "CA1031")]
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
