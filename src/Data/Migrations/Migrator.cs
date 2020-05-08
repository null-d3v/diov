using DbUp;
using System;
using System.Reflection;

namespace Diov.Data
{
    public class Migrator
    {
        public Migrator(string connection)
        {
            Connection = connection ??
                throw new ArgumentNullException(nameof(connection));
        }

        public string Connection { get; }

        public void Migrate()
        {
            var upgradeEngine = DeployChanges.To
                .SqlDatabase(Connection)
                .JournalToSqlTable("dbo", "Migration")
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            if (upgradeEngine.IsUpgradeRequired())
            {
                upgradeEngine.PerformUpgrade();
            }
        }
    }
}