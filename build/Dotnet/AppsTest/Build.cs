using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{

    private Target DotnetAppsTestMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{Paths.DotnetCoreDatabaseResourcesCore} {Paths.DotnetBaseDatabaseResourcesBase} {Paths.DotnetAppsDatabaseResourcesApps} {Paths.DotnetAppsTestDatabaseResourcesApps} {Paths.DotnetAppsTestDatabaseResources}")));

    private Target DotnetAppsTestGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetAppsTestMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetAppsTestRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetAppsTestDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetAppsTest)
                .SetProjectFile(Paths.DotnetAppsTestDatabaseGenerate));
        });

    private Target DotnetAppsTestPublishServer => _ => _
        .DependsOn(DotnetAppsTestGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetAppsTestDatabaseServer)
                .SetOutput(Paths.ArtifactsAppsServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetAppsTestDatabaseTestDomain => _ => _
        .DependsOn(DotnetAppsTestGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetAppsTestDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=AppsDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetAppsTestDatabaseTest => _ => _
        .DependsOn(DotnetAppsTestDatabaseTestDomain);
    
    private Target DotnetAppsTestWorkspaceTest => _ => _;

    private Target DotnetAppsTest => _ => _
        .DependsOn(DotnetAppsTestDatabaseTest)
        .DependsOn(DotnetAppsTestWorkspaceTest);

}
