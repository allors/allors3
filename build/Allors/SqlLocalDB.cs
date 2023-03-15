using System;
using MartinCostello.SqlLocalDb;
using Microsoft.Data.SqlClient;
using static Nuke.Common.Logger;

internal class SqlLocalDB : IDisposable
{
    private ISqlLocalDbInstanceInfo dbInstance;
    private ISqlLocalDbInstanceManager manager;
    private SqlLocalDbApi sqlLocalDbApi;

    public SqlLocalDB()
    {
        sqlLocalDbApi = new SqlLocalDbApi();
        dbInstance = sqlLocalDbApi.GetDefaultInstance();
        manager = dbInstance.Manage();

        if (!dbInstance.IsRunning)
        {
            Normal("SqlServer: Start");
            manager.Start();
        }
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
        ExecuteCommand($"DROP DATABASE IF EXISTS [{database}]");

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
    }

    public int ExecuteCommand(string commandText)
    {
        using var connection = manager.CreateConnection();
        connection.Open();
        using var command = new SqlCommand(commandText, connection);
        command.CommandTimeout = 5 * 60;
        return command.ExecuteNonQuery();
    }
}
