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
                .SetProjectFile(this.Paths.CoreDatabaseMerge)
                .SetApplicationArguments($"{this.Paths.CoreDatabaseResourcesCore} {this.Paths.CoreDatabaseResourcesCustom} {this.Paths.CoreDatabaseResources}"));
        });

    Target CoreGenerate => _ => _
        .After(this.Clean)
        .DependsOn(this.CoreMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                .SetApplicationArguments($"{this.Paths.CoreRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.CoreDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.Core)
                .SetProjectFile(this.Paths.CoreDatabaseGenerate));
        });

    Target CoreDatabaseTestDomain => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreDatabaseDomainTests)
                .SetLogger("trx;LogFileName=CoreDatabaseDomain.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target CoreDatabaseTestServerLocal => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreDatabaseServerLocalTests)
                .SetLogger("trx;LogFileName=CoreDatabaseApi.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target CorePublishCommands => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.CoreDatabaseCommands)
                .SetOutput(this.Paths.ArtifactsCoreCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    Target CorePublishServer => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.CoreDatabaseServer)
                .SetOutput(this.Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    Target CoreDatabaseTestServerRemote => _ => _
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.CorePublishServer)
        .DependsOn(this.CorePublishCommands)
        .DependsOn(this.CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreDatabaseServerRemoteTests)
                .SetLogger("trx;LogFileName=CoreDatabaseServer.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target CoreInstall => _ => _
        .Executes(() =>
        {
            NpmInstall(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript));
        });

    Target CoreWorkspaceTypescriptMeta => _ => _
        .After(this.CoreInstall)
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() =>
        {
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript)
                .SetCommand("test:meta"));
        });


    Target CoreWorkspaceTypescriptWorkspace => _ => _
        .After(this.CoreInstall)
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() =>
        {
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript)
                .SetCommand("test:workspace"));
        });

    Target CoreWorkspaceTypescriptClient => _ => _
        .After(this.CoreInstall)
        .DependsOn(this.EnsureDirectories)
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.CorePublishServer)
        .DependsOn(this.CorePublishCommands)
        .DependsOn(this.CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript)
                .SetCommand("test:client"));
        });

    Target CoreWorkspaceCSharpTest => _ => _
        .DependsOn(this.CorePublishServer)
        .DependsOn(this.CorePublishCommands)
        .DependsOn(this.CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();

            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreWorkspaceCSharpTests)
                .SetLogger("trx;LogFileName=CoreWorkspaceCSharpTests.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));

            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreWorkspaceCSharpDirectTests)
                .SetLogger("trx;LogFileName=CoreWorkspaceCSharpDirectTests.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));

            DotNetTest(s => s
                .SetProjectFile(this.Paths.CoreWorkspaceCSharpRemoteTests)
                .SetLogger("trx;LogFileName=CoreWorkspaceCSharpRemoteTests.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target CoreDatabaseTest => _ => _
        .DependsOn(this.CoreDatabaseTestDomain)
        .DependsOn(this.CoreDatabaseTestServerLocal)
        .DependsOn(this.CoreDatabaseTestServerRemote);

    Target CoreWorkspaceTypescriptTest => _ => _
        .DependsOn(this.CoreWorkspaceTypescriptMeta)
        .DependsOn(this.CoreWorkspaceTypescriptWorkspace)
        .DependsOn(this.CoreWorkspaceTypescriptClient);

    Target CoreWorkspaceTest => _ => _
        .DependsOn(this.CoreWorkspaceCSharpTest)
        .DependsOn(this.CoreWorkspaceTypescriptTest);

    Target CoreTest => _ => _
        .DependsOn(this.CoreDatabaseTest)
        .DependsOn(this.CoreWorkspaceTest);

    Target Core => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.CoreTest);
}
