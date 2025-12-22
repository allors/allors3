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
                .SetApplicationArguments(Paths.DotnetSystemAdaptersRepositoryDomainRepository,
                    Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetSystemAdaptersMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetSystemAdapters)
                .SetProjectFile(Paths.DotnetSystemAdaptersGenerate));
        });

    private Target DotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetSystemAdaptersMemoryTests)
            .AddLoggers("trx;LogFileName=AdaptersMemory.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetSystemAdaptersSqlClientTests)
                .AddLoggers("trx;LogFileName=AdaptersSqlClient.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetSystemAdaptersNpgsqlTests)
                .AddLoggers("trx;LogFileName=AdaptersNpgsql.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetSystemAdaptersTestUnified => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetSystemAdaptersUnifiedTests)
                .AddLoggers("trx;LogFileName=AdaptersUnified.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetSystemAdapters => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetSystemAdaptersTestMemory)
        .DependsOn(DotnetSystemAdaptersTestSqlClient)
        .DependsOn(DotnetSystemAdaptersTestNpgsql)
        .DependsOn(DotnetSystemAdaptersTestUnified);
}