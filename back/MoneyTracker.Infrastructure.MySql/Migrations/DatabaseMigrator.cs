using DbUp;

namespace MoneyTracker.Infrastructure.MySql.Migrations;

/// <summary>
/// Aplica las migraciones SQL versionadas (scripts embebidos en este ensamblado)
/// usando DbUp. DbUp lleva el control de los scripts ya ejecutados en la tabla
/// `SchemaVersions`, así que cada script se ejecuta una sola vez y en orden.
/// </summary>
public static class DatabaseMigrator
{
    public static void Run(string connectionString)
    {
        // Nota: la base de datos debe existir de antemano (se crea una vez con un usuario
        // admin). El usuario de la app solo tiene permisos sobre SU base, no sobre `mysql`,
        // por eso NO usamos EnsureDatabase aquí. DbUp crea las TABLAS (Script0001/0002).
        var upgrader = DeployChanges.To
            .MySqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw new InvalidOperationException(
                "Fallo al aplicar las migraciones de base de datos con DbUp.", result.Error);
    }
}
