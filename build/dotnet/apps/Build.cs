using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetAppsResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Apps";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target DotnetAppsDatabaseTest => _ => _
        .DependsOn(this.DotnetAppsDatabaseTestDomain);

    private Target DotnetAppsMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(this.Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{this.Paths.DotnetCoreDatabaseResourcesCore} {this.Paths.DotnetAppsDatabaseResourcesApps} {this.Paths.DotnetAppsDatabaseResources}")));

    private Target DotnetAppsDatabaseTestDomain => _ => _
        .DependsOn(this.DotnetAppsGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DotnetAppsDatabaseDomainTests)
            .SetLogger("trx;LogFileName=AppsDatabaseDomain.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DotnetAppsGenerate => _ => _
        .After(this.Clean)
        .DependsOn(this.DotnetAppsMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DotnetAppsRepositoryDomainRepository} {this.Paths.DotnetSystemRepositoryTemplatesMetaCs} {this.Paths.DotnetAppsDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.DotnetApps)
                .SetProjectFile(this.Paths.DotnetAppsDatabaseGenerate));
        });

    private Target DotnetAppsPublishCommands => _ => _
        .DependsOn(this.DotnetAppsGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.DotnetAppsDatabaseCommands)
                .SetOutput(this.Paths.ArtifactsAppsCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetAppsPublishServer => _ => _
        .DependsOn(this.DotnetAppsGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.DotnetAppsDatabaseServer)
                .SetOutput(this.Paths.ArtifactsAppsServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetAppsInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetAppsWorkspaceTypescript)));

    private Target DotnetAppsWorkspaceTypescriptSession => _ => _
        .DependsOn(this.DotnetAppsGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetAppsWorkspaceTypescript)
            .SetCommand("domain:test")));

    private Target DotnetAppsWorkspaceTypescriptTest => _ => _
        .DependsOn(this.DotnetAppsWorkspaceTypescriptSession);

    private Target DotnetAppsWorkspaceTest => _ => _
        .DependsOn(this.DotnetAppsWorkspaceTypescriptTest);

    private Target DotnetAppsTest => _ => _
        .DependsOn(this.DotnetAppsDatabaseTest)
        .DependsOn(this.DotnetAppsWorkspaceTypescriptTest);

    private Target DotnetApps => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.DotnetAppsTest);
}
