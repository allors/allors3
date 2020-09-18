using System;
using MartinCostello.SqlLocalDb;
using static Nuke.Common.Logger;

partial class SqlServer : IDisposable
{
    private SqlLocalDbApi sqlLocalDbApi;
    private ISqlLocalDbInstanceInfo dbInstance;
    private ISqlLocalDbInstanceManager manager;

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
}
