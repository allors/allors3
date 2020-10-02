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
         .DependsOn(BaseDatabaseTestDomain);

    private Target BaseMerge => _ => _
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.CoreDatabaseMerge)
                .SetApplicationArguments($"{Paths.CoreDatabaseResourcesCore} {Paths.BaseDatabaseResourcesBase} {Paths.BaseDatabaseResources}"));
        });

    private Target BaseDatabaseTestDomain => _ => _
         .DependsOn(BaseGenerate)
         .Executes(() =>
         {
             DotNetTest(s => s
                 .SetProjectFile(Paths.BaseDatabaseDomainTests)
                 .SetLogger("trx;LogFileName=BaseDatabaseDomain.trx")
                 .SetResultsDirectory(Paths.ArtifactsTests));
         });

    private Target BaseGenerate => _ => _
         .After(Clean)
         .DependsOn(BaseMerge)
         .Executes(() =>
         {
             DotNetRun(s => s
                 .SetProjectFile(Paths.PlatformRepositoryGenerate)
                 .SetApplicationArguments($"{Paths.BaseRepositoryDomainRepository} {Paths.PlatformRepositoryTemplatesMetaCs} {Paths.BaseDatabaseMetaGenerated}"));
             DotNetRun(s => s
                 .SetWorkingDirectory(Paths.Base)
                 .SetProjectFile(Paths.BaseDatabaseGenerate));
         });

    private Target BasePublishCommands => _ => _
         .DependsOn(BaseGenerate)
         .Executes(() =>
         {
             var dotNetPublishSettings = new DotNetPublishSettings()
                 .SetWorkingDirectory(Paths.BaseDatabaseCommands)
                 .SetOutput(Paths.ArtifactsBaseCommands);
             DotNetPublish(dotNetPublishSettings);
         });

    private Target BasePublishServer => _ => _
             .DependsOn(BaseGenerate)
         .Executes(() =>
         {
             var dotNetPublishSettings = new DotNetPublishSettings()
                 .SetWorkingDirectory(Paths.BaseDatabaseServer)
                 .SetOutput(Paths.ArtifactsBaseServer);
             DotNetPublish(dotNetPublishSettings);
         });

    private Target BaseInstall => _ => _
        .Executes(() =>
        {
            NpmInstall(s => s
                .SetEnvironmentVariable("npm_config_loglevel", "error")
                .SetWorkingDirectory(Paths.BaseWorkspaceTypescript));
        });

    private Target BaseWorkspaceTypescriptSession => _ => _
         .DependsOn(BaseGenerate)
         .DependsOn(EnsureDirectories)
         .Executes(() =>
         {
             NpmRun(s => s
                 .SetEnvironmentVariable("npm_config_loglevel", "error")
                 .SetWorkingDirectory(Paths.BaseWorkspaceTypescript)
                 .SetCommand("domain:test"));
         });

    private Target BaseWorkspaceTypescriptTest => _ => _
        .DependsOn(BaseWorkspaceTypescriptSession);

    private Target BaseWorkspaceTest => _ => _
        .DependsOn(BaseWorkspaceTypescriptTest);

    private Target BaseTest => _ => _
        .DependsOn(BaseDatabaseTest)
        .DependsOn(BaseWorkspaceTypescriptTest);

    private Target Base => _ => _
        .DependsOn(Clean)
        .DependsOn(BaseTest);
}
