using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetBaseResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Base";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target DotnetBaseDatabaseTest => _ => _
        .DependsOn(this.DotnetBaseDatabaseTestDomain);

    private Target DotnetBaseMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(this.Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{this.Paths.DotnetCoreDatabaseResourcesCore} {this.Paths.DotnetBaseDatabaseResourcesBase} {this.Paths.DotnetBaseDatabaseResources}")));

    private Target DotnetBaseDatabaseTestDomain => _ => _
        .DependsOn(this.DotnetBaseGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DotnetBaseDatabaseDomainTests)
            .SetLogger("trx;LogFileName=BaseDatabaseDomain.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DotnetBaseGenerate => _ => _
        .After(this.Clean)
        .DependsOn(this.DotnetBaseMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DotnetBaseRepositoryDomainRepository} {this.Paths.DotnetSystemRepositoryTemplatesMetaCs} {this.Paths.DotnetBaseDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.DotnetBase)
                .SetProjectFile(this.Paths.DotnetBaseDatabaseGenerate));
        });

    private Target DotnetBasePublishCommands => _ => _
        .DependsOn(this.DotnetBaseGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.DotnetBaseDatabaseCommands)
                .SetOutput(this.Paths.ArtifactsBaseCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetBasePublishServer => _ => _
        .DependsOn(this.DotnetBaseGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.DotnetBaseDatabaseServer)
                .SetOutput(this.Paths.ArtifactsBaseServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetBaseInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetBaseWorkspaceTypescript)));

    private Target DotnetBaseWorkspaceTypescriptSession => _ => _
        .DependsOn(this.DotnetBaseGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetBaseWorkspaceTypescript)
            .SetCommand("domain:test")));

    private Target DotnetBaseWorkspaceTypescriptTest => _ => _
        .DependsOn(this.DotnetBaseWorkspaceTypescriptSession);

    private Target DotnetBaseWorkspaceTest => _ => _
        .DependsOn(this.DotnetBaseWorkspaceTypescriptTest);

    private Target DotnetBaseTest => _ => _
        .DependsOn(this.DotnetBaseDatabaseTest)
        .DependsOn(this.DotnetBaseWorkspaceTypescriptTest);

    private Target DotnetBase => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.DotnetBaseTest);
}
