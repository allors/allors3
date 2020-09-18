using Microsoft.Data.SqlClient;
using Nuke.Common;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    Target SqlServerSetup => _ => _
        .Executes(() =>
        {
            using var connection = new SqlConnection(@"Server=(local);Database=master;User Id=sa;Password='Password1234'");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE LOGIN test WITH PASSWORD = 'Password1234'";
            command.ExecuteNonQuery();
            command.CommandText = @"
EXECUTE sys.sp_addsrvrolemember
    @loginame = N'test',
    @rolename = N'dbcreator'";
            command.ExecuteNonQuery();
        });

    Target SqlServer => _ => _
        .Executes(() =>
        {
            DockerImagePull(v => v.SetName("mcr.microsoft.com/mssql/server"));
            DockerRun(v => v
                .SetDetach(true)
                .SetImage("mcr.microsoft.com/mssql/server")
                .SetName("sql")
                .SetPublish("1433:1433")
                .AddEnv("ACCEPT_EULA=Y")
                .AddEnv("MSSQL_PID=Developer")
                .AddEnv("SA_PASSWORD=Password1234")
            );
        });
}

