using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    Target BaseResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Base";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target BaseDatabaseTest => _ => _
         .DependsOn(this.BaseDatabaseTestDomain);

    private Target BaseMerge => _ => _
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.CoreDatabaseMerge)
                .SetApplicationArguments($"{this.Paths.CoreDatabaseResourcesCore} {this.Paths.BaseDatabaseResourcesBase} {this.Paths.BaseDatabaseResources}"));
        });

    private Target BaseDatabaseTestDomain => _ => _
         .DependsOn(this.BaseGenerate)
         .Executes(() =>
         {
             DotNetTest(s => s
                 .SetProjectFile(this.Paths.BaseDatabaseDomainTests)
                 .SetLogger("trx;LogFileName=BaseDatabaseDomain.trx")
                 .SetResultsDirectory(this.Paths.ArtifactsTests));
         });

    private Target BaseGenerate => _ => _
         .After(this.Clean)
         .DependsOn(this.BaseMerge)
         .Executes(() =>
         {
             DotNetRun(s => s
                 .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                 .SetApplicationArguments($"{this.Paths.BaseRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.BaseDatabaseMetaGenerated}"));
             DotNetRun(s => s
                 .SetWorkingDirectory(this.Paths.Base)
                 .SetProjectFile(this.Paths.BaseDatabaseGenerate));
         });

    private Target BasePublishCommands => _ => _
         .DependsOn(this.BaseGenerate)
         .Executes(() =>
         {
             var dotNetPublishSettings = new DotNetPublishSettings()
                 .SetWorkingDirectory(this.Paths.BaseDatabaseCommands)
                 .SetOutput(this.Paths.ArtifactsBaseCommands);
             DotNetPublish(dotNetPublishSettings);
         });

    private Target BasePublishServer => _ => _
             .DependsOn(this.BaseGenerate)
         .Executes(() =>
         {
             var dotNetPublishSettings = new DotNetPublishSettings()
                 .SetWorkingDirectory(this.Paths.BaseDatabaseServer)
                 .SetOutput(this.Paths.ArtifactsBaseServer);
             DotNetPublish(dotNetPublishSettings);
         });

    private Target BaseInstall => _ => _
        .Executes(() =>
        {
            NpmInstall(s => s
                .SetEnvironmentVariable("npm_config_loglevel", "error")
                .SetWorkingDirectory(this.Paths.BaseWorkspaceTypescript));
        });

    private Target BaseWorkspaceTypescriptSession => _ => _
         .DependsOn(this.BaseGenerate)
         .DependsOn(this.EnsureDirectories)
         .Executes(() =>
         {
             NpmRun(s => s
                 .SetEnvironmentVariable("npm_config_loglevel", "error")
                 .SetWorkingDirectory(this.Paths.BaseWorkspaceTypescript)
                 .SetCommand("domain:test"));
         });

    private Target BaseWorkspaceTypescriptTest => _ => _
        .DependsOn(this.BaseWorkspaceTypescriptSession);

    private Target BaseWorkspaceTest => _ => _
        .DependsOn(this.BaseWorkspaceTypescriptTest);

    private Target BaseTest => _ => _
        .DependsOn(this.BaseDatabaseTest)
        .DependsOn(this.BaseWorkspaceTypescriptTest);

    private Target Base => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.BaseTest);
}
