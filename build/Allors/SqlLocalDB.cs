using System;
using MartinCostello.SqlLocalDb;
using Microsoft.Data.SqlClient;

internal class SqlLocalDB : IDisposable
{
    private const string SqlClientEnvironmentVariable = "ALLORS_SQLCLIENT";
    private const string LegacySqlClientEnvironmentVariable = "ALLORS_TEST_SQLCLIENT_CONNECTION";

    private ISqlLocalDbInstanceInfo dbInstance;
    private ISqlLocalDbInstanceManager manager;
    private SqlLocalDbApi sqlLocalDbApi;
    private string adminConnectionString;

    public SqlLocalDB()
    {
        if (OperatingSystem.IsWindows())
        {
            sqlLocalDbApi = new SqlLocalDbApi();
            dbInstance = sqlLocalDbApi.GetDefaultInstance();
            manager = dbInstance.Manage();

            if (!dbInstance.IsRunning)
            {
                Serilog.Log.Debug("SqlServer: Start");
                manager.Start();
            }

            return;
        }

        var rawConnectionString = System.Environment.GetEnvironmentVariable(SqlClientEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            rawConnectionString = System.Environment.GetEnvironmentVariable(LegacySqlClientEnvironmentVariable);
        }

        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException(
                $"{SqlClientEnvironmentVariable} is not set. Point it at an admin SQL Server connection that can create databases.");
        }

        adminConnectionString = new SqlConnectionStringBuilder(rawConnectionString)
        {
            InitialCatalog = "master",
        }.ConnectionString;
    }

    public void Dispose()
    {
        sqlLocalDbApi?.Dispose();

        sqlLocalDbApi = null;
        dbInstance = null;
        manager = null;
    }

    public void Init(string database, string user = null, string password = null)
    {
        ExecuteCommand($@"
IF DB_ID('{database}') IS NOT NULL
BEGIN
    ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{database}];
END");

        ExecuteCommand($"CREATE DATABASE [{database}]");

        if (!string.IsNullOrWhiteSpace(user))
        {
            ExecuteCommand(@$"
USE [{database}]

IF NOT EXISTS(SELECT principal_id FROM sys.server_principals WHERE name = '{user}') BEGIN
    CREATE LOGIN {user}
    WITH PASSWORD = '{password}'
END

IF NOT EXISTS(SELECT principal_id FROM sys.database_principals WHERE name = '{user}') BEGIN
    CREATE USER {user} FOR LOGIN {user}
END

ALTER ROLE [db_owner] ADD MEMBER [{user}]
");
        }

        if (!OperatingSystem.IsWindows())
        {
            var databaseConnectionString = new SqlConnectionStringBuilder(adminConnectionString)
            {
                InitialCatalog = database,
            };

            if (!string.IsNullOrWhiteSpace(user))
            {
                databaseConnectionString.IntegratedSecurity = false;
                databaseConnectionString.UserID = user;
                databaseConnectionString.Password = password;
            }

            System.Environment.SetEnvironmentVariable("adapter", "sqlclient");
            System.Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", databaseConnectionString.ConnectionString);
        }
    }

    public int ExecuteCommand(string commandText)
    {
        using var connection = manager != null
            ? manager.CreateConnection()
            : new SqlConnection(adminConnectionString);
        connection.Open();
        using var command = new SqlCommand(commandText, connection);
        command.CommandTimeout = 5 * 60;
        return command.ExecuteNonQuery();
    }
}
