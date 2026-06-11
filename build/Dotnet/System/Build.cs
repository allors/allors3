using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DotnetSystemAdaptersGenerate => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetSystemAdaptersRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetSystemAdaptersMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetSystemAdapters)
                .SetProjectFile(Paths.DotnetSystemAdaptersGenerate));
        });

    private Target DotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetSystemAdaptersStaticTests)
            .SetFilter("FullyQualifiedName~Allors.Database.Adapters.Memory")
            .AddLoggers("trx;LogFileName=AdaptersMemory.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            // The adapter fixtures create a per-test-class database from the ALLORS_SQLCLIENT admin
            // connection, so no local SQL Server instance needs to be provisioned here.
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetSystemAdaptersStaticTests)
                .SetFilter("FullyQualifiedName~Allors.Database.Adapters.Sql.SqlClient")
                .AddLoggers("trx;LogFileName=AdaptersSqlClient.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetSystemAdaptersStaticTests)
                .SetFilter("FullyQualifiedName~Allors.Database.Adapters.Sql.Npgsql")
                .AddLoggers("trx;LogFileName=AdaptersNpgsql.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetSystemAdapters => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetSystemAdaptersTestMemory)
        .DependsOn(DotnetSystemAdaptersTestSqlClient)
        .DependsOn(DotnetSystemAdaptersTestNpgsql);

    private Target DotnetSystemWorkspaceSignalsTest => _ => _
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetSystemWorkspaceSignalsDefaultTests)
            .AddLoggers("trx;LogFileName=DotnetSystemWorkspaceSignals.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));
}
