using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    Target CoreResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Core";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target CoreMerge => _ => _
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.CoreDatabaseMerge)
                .SetApplicationArguments($"{Paths.CoreDatabaseResourcesCore} {Paths.CoreDatabaseResourcesCustom} {Paths.CoreDatabaseResources}"));
        });

    Target CoreGenerate => _ => _
        .After(Clean)
        .DependsOn(CoreMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.PlatformRepositoryGenerate)
                .SetApplicationArguments($"{Paths.CoreRepositoryDomainRepository} {Paths.PlatformRepositoryTemplatesMetaCs} {Paths.CoreDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetWorkingDirectory(Paths.Core)
                .SetProjectFile(Paths.CoreDatabaseGenerate));
        });

    Target CoreDatabaseTestDomain => _ => _
        .DependsOn(CoreGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.CoreDatabaseDomainTests)
                .SetLogger("trx;LogFileName=CoreDatabaseDomain.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    Target CoreDatabaseTestApi => _ => _
        .DependsOn(CoreGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.CoreDatabaseApiTests)
                .SetLogger("trx;LogFileName=CoreDatabaseApi.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    Target CorePublishCommands => _ => _
        .DependsOn(CoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetWorkingDirectory(Paths.CoreDatabaseCommands)
                .SetOutput(Paths.ArtifactsCoreCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    Target CorePublishServer => _ => _
        .DependsOn(CoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetWorkingDirectory(Paths.CoreDatabaseServer)
                .SetOutput(Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    Target CoreDatabaseTestServer => _ => _
        .DependsOn(CoreGenerate)
        .DependsOn(CorePublishServer)
        .DependsOn(CorePublishCommands)
        .DependsOn(CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreDatabaseServerTests)
                .SetLogger("trx;LogFileName=CoreDatabaseServer.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target CoreInstall => _ => _
        .Executes(() =>
        {
            NpmInstall(s => s
                .SetEnvironmentVariable("npm_config_loglevel", "error")
                .SetWorkingDirectory(Paths.CoreWorkspaceTypescript));
        });

    Target CoreWorkspaceTypescriptMeta => _ => _
        .After(CoreInstall)
        .DependsOn(CoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() =>
        {
            NpmRun(s => s
                .SetEnvironmentVariable("npm_config_loglevel", "error")
                .SetWorkingDirectory(Paths.CoreWorkspaceTypescript)
                .SetCommand("test:meta"));
        });


    Target CoreWorkspaceTypescriptWorkspace => _ => _
        .After(CoreInstall)
        .DependsOn(CoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() =>
        {
            NpmRun(s => s
                .SetEnvironmentVariable("npm_config_loglevel", "error")
                .SetWorkingDirectory(Paths.CoreWorkspaceTypescript)
                .SetCommand("test:workspace"));
        });

    Target CoreWorkspaceTypescriptClient => _ => _
        .After(CoreInstall)
        .DependsOn(EnsureDirectories)
        .DependsOn(CoreGenerate)
        .DependsOn(CorePublishServer)
        .DependsOn(CorePublishCommands)
        .DependsOn(CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            NpmRun(s => s
                .SetEnvironmentVariable("npm_config_loglevel", "error")
                .SetWorkingDirectory(this.Paths.CoreWorkspaceTypescript)
                .SetCommand("test:client"));
        });

    Target CoreWorkspaceCSharpDomainTests => _ => _
        .DependsOn(CorePublishServer)
        .DependsOn(CorePublishCommands)
        .DependsOn(CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreWorkspaceCSharpDomainTests)
                .SetLogger("trx;LogFileName=CoreWorkspaceCSharpDomainTests.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target CoreDatabaseTest => _ => _
        .DependsOn(CoreDatabaseTestDomain)
        .DependsOn(CoreDatabaseTestApi)
        .DependsOn(CoreDatabaseTestServer);

    Target CoreWorkspaceTypescriptTest => _ => _
        .DependsOn(CoreWorkspaceTypescriptMeta)
        .DependsOn(this.CoreWorkspaceTypescriptWorkspace)
        .DependsOn(this.CoreWorkspaceTypescriptClient);

    Target CoreWorkspaceCSharpTest => _ => _
        .DependsOn(CoreWorkspaceCSharpDomainTests);

    Target CoreWorkspaceTest => _ => _
        .DependsOn(CoreWorkspaceCSharpTest)
        .DependsOn(CoreWorkspaceTypescriptTest);

    Target CoreTest => _ => _
        .DependsOn(CoreDatabaseTest)
        .DependsOn(CoreWorkspaceTest);

    Target Core => _ => _
        .DependsOn(Clean)
        .DependsOn(CoreTest);
}
