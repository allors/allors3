using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetAppsResetDataapps => _ => _
        .Executes(() =>
        {
            var database = "Apps";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target DotnetAppsDatabaseTest => _ => _
        .DependsOn(DotnetAppsDatabaseTestDomain);

    private Target DotnetAppsMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{Paths.DotnetCoreDatabaseResourcesCore} {Paths.DotnetAppsDatabaseResourcesApps} {Paths.DotnetAppsDatabaseResources}")));

    private Target DotnetAppsDatabaseTestDomain => _ => _
        .DependsOn(DotnetAppsGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetAppsDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=AppsDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetAppsGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetAppsMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{Paths.DotnetAppsRepositoryDomainRepository} {Paths.DotnetSystemRepositoryTemplatesMetaCs} {Paths.DotnetAppsDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetApps)
                .SetProjectFile(Paths.DotnetAppsDatabaseGenerate));
        });

    private Target DotnetAppsPublishCommands => _ => _
        .DependsOn(DotnetAppsGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetAppsDatabaseCommands)
                .SetOutput(Paths.ArtifactsAppsCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetAppsPublishServer => _ => _
        .DependsOn(DotnetAppsGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetAppsDatabaseServer)
                .SetOutput(Paths.ArtifactsAppsServer);
            DotNetPublish(dotNetPublishSettings);
        });
    
    private Target DotnetAppsWorkspaceTypescriptSession => _ => _
        .DependsOn(DotnetAppsGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.DotnetAppsWorkspaceTypescript)
            .SetCommand("domain:test")));

    private Target DotnetAppsWorkspaceTypescriptTest => _ => _
        .DependsOn(DotnetAppsWorkspaceTypescriptSession);

    private Target DotnetAppsWorkspaceTest => _ => _
        .DependsOn(DotnetAppsWorkspaceTypescriptTest);

    private Target DotnetAppsTest => _ => _
        .DependsOn(DotnetAppsDatabaseTest)
        .DependsOn(DotnetAppsWorkspaceTypescriptTest);

    private Target DotnetApps => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetAppsTest);
}
