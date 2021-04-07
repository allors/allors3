using System;
using MartinCostello.SqlLocalDb;
using Microsoft.Data.SqlClient;
using static Nuke.Common.Logger;

internal class SqlServer : IDisposable
{
    private ISqlLocalDbInstanceInfo dbInstance;
    private ISqlLocalDbInstanceManager manager;
    private SqlLocalDbApi sqlLocalDbApi;

    public SqlServer()
    {
        this.sqlLocalDbApi = new SqlLocalDbApi();
        this.dbInstance = this.sqlLocalDbApi.GetDefaultInstance();
        this.manager = this.dbInstance.Manage();

        if (!this.dbInstance.IsRunning)
        {
            Normal("SqlServer: Start");
            this.manager.Start();
        }
    }

    public void Dispose()
    {
        this.sqlLocalDbApi?.Dispose();

        this.sqlLocalDbApi = null;
        this.dbInstance = null;
        this.manager = null;
    }

    public void Drop(string database) => this.ExecuteCommand($"DROP DATABASE IF EXISTS [{database}]");

    public void Create(string database) => this.ExecuteCommand($"CREATE DATABASE [{database}]");

    private int ExecuteCommand(string commandText)
    {
        using var connection = this.manager.CreateConnection();
        connection.Open();
        using var command = new SqlCommand(commandText, connection);
        return command.ExecuteNonQuery();
    }
}
