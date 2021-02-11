using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    Target AppsResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Apps";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target AppsDatabaseTest => _ => _
         .DependsOn(this.AppsDatabaseTestDomain);

    private Target AppsMerge => _ => _
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.CoreDatabaseMerge)
                .SetApplicationArguments($"{this.Paths.CoreDatabaseResourcesCore} {this.Paths.AppsDatabaseResourcesApps} {this.Paths.AppsDatabaseResources}"));
        });

    private Target AppsDatabaseTestDomain => _ => _
         .DependsOn(this.AppsGenerate)
         .Executes(() =>
         {
             DotNetTest(s => s
                 .SetProjectFile(this.Paths.AppsDatabaseDomainTests)
                 .SetLogger("trx;LogFileName=AppsDatabaseDomain.trx")
                 .SetResultsDirectory(this.Paths.ArtifactsTests));
         });

    private Target AppsGenerate => _ => _
         .After(this.Clean)
         .DependsOn(this.AppsMerge)
         .Executes(() =>
         {
             DotNetRun(s => s
                 .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                 .SetApplicationArguments($"{this.Paths.AppsRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.AppsDatabaseMetaGenerated}"));
             DotNetRun(s => s
                 .SetProcessWorkingDirectory(this.Paths.Apps)
                 .SetProjectFile(this.Paths.AppsDatabaseGenerate));
         });

    private Target AppsPublishCommands => _ => _
         .DependsOn(this.AppsGenerate)
         .Executes(() =>
         {
             var dotNetPublishSettings = new DotNetPublishSettings()
                 .SetProcessWorkingDirectory(this.Paths.AppsDatabaseCommands)
                 .SetOutput(this.Paths.ArtifactsAppsCommands);
             DotNetPublish(dotNetPublishSettings);
         });

    private Target AppsPublishServer => _ => _
             .DependsOn(this.AppsGenerate)
         .Executes(() =>
         {
             var dotNetPublishSettings = new DotNetPublishSettings()
                 .SetProcessWorkingDirectory(this.Paths.AppsDatabaseServer)
                 .SetOutput(this.Paths.ArtifactsAppsServer);
             DotNetPublish(dotNetPublishSettings);
         });

    private Target AppsInstall => _ => _
        .Executes(() =>
        {
            NpmInstall(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(this.Paths.AppsWorkspaceTypescript));
        });

    private Target AppsWorkspaceTypescriptSession => _ => _
         .DependsOn(this.AppsGenerate)
         .DependsOn(this.EnsureDirectories)
         .Executes(() =>
         {
             NpmRun(s => s
                 .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                 .SetProcessWorkingDirectory(this.Paths.AppsWorkspaceTypescript)
                 .SetCommand("domain:test"));
         });

    private Target AppsWorkspaceTypescriptTest => _ => _
        .DependsOn(this.AppsWorkspaceTypescriptSession);

    private Target AppsWorkspaceTest => _ => _
        .DependsOn(this.AppsWorkspaceTypescriptTest);

    private Target AppsTest => _ => _
        .DependsOn(this.AppsDatabaseTest)
        .DependsOn(this.AppsWorkspaceTypescriptTest);

    private Target Apps => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.AppsTest);
}
