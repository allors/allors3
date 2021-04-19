using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target CoreResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Core";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target CoreMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(this.Paths.CoreDatabaseMerge)
            .SetApplicationArguments(
                $"{this.Paths.CoreDatabaseResourcesCore} {this.Paths.CoreDatabaseResourcesCustom} {this.Paths.CoreDatabaseResources}")));

    private Target CoreGenerate => _ => _
        .After(this.Clean)
        .DependsOn(this.CoreMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.CoreRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.CoreDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.Core)
                .SetProjectFile(this.Paths.CoreDatabaseGenerate));
        });

    private Target CoreDatabaseTestMeta => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.CoreDatabaseMetaTests)
            .SetLogger("trx;LogFileName=CoreDatabaseMeta.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target CoreDatabaseTestDomain => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.CoreDatabaseDomainTests)
            .SetLogger("trx;LogFileName=CoreDatabaseDomain.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target CoreDatabaseTestServerLocal => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.CoreDatabaseServerLocalTests)
            .SetLogger("trx;LogFileName=CoreDatabaseApi.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target CorePublishCommands => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.CoreDatabaseCommands)
                .SetOutput(this.Paths.ArtifactsCoreCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target CorePublishServer => _ => _
        .DependsOn(this.CoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.CoreDatabaseServer)
                .SetOutput(this.Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target CoreDatabaseTestServerRemote => _ => _
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

    private Target CoreInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript)));

    private Target CoreWorkspaceTypescriptMeta => _ => _
        .After(this.CoreInstall)
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript)
            .SetCommand("test:meta")));

    private Target CoreWorkspaceTypescriptWorkspace => _ => _
        .After(this.CoreInstall)
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.CoreWorkspaceTypescript)
            .SetCommand("test:workspace")));

    private Target CoreWorkspaceTypescriptClient => _ => _
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

    private Target CoreWorkspaceCSharpTest => _ => _
        .DependsOn(this.CorePublishServer)
        .DependsOn(this.CorePublishCommands)
        .DependsOn(this.CoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);

            {
                DotNetTest(s => s
                    .SetProjectFile(this.Paths.CoreWorkspaceCSharpAdaptersLocalTests)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpAdaptersLocalTests.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));

                DotNetTest(s => s
                    .SetProjectFile(this.Paths.CoreWorkspaceCSharpTestsLocal)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpTestsLocal.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));
            }

            {
                using var server = new Server(this.Paths.ArtifactsCoreServer);
                await server.Ready();

                DotNetTest(s => s
                    .SetProjectFile(this.Paths.CoreWorkspaceCSharpAdaptersRemoteTests)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpAdaptersRemoteTests.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));

                DotNetTest(s => s
                    .SetProjectFile(this.Paths.CoreWorkspaceCSharpTestsRemote)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpTestsRemote.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));
            }
        });

    private Target CoreDatabaseTest => _ => _
        .DependsOn(this.CoreDatabaseTestMeta)
        .DependsOn(this.CoreDatabaseTestDomain)
        .DependsOn(this.CoreDatabaseTestServerLocal)
        .DependsOn(this.CoreDatabaseTestServerRemote);

    private Target CoreWorkspaceTypescriptTest => _ => _
        .DependsOn(this.CoreWorkspaceTypescriptMeta)
        .DependsOn(this.CoreWorkspaceTypescriptWorkspace)
        .DependsOn(this.CoreWorkspaceTypescriptClient);

    private Target CoreWorkspaceTest => _ => _
        .DependsOn(this.CoreWorkspaceCSharpTest);
        //.DependsOn(this.CoreWorkspaceTypescriptTest);

    private Target CoreTest => _ => _
        .DependsOn(this.CoreDatabaseTest)
        .DependsOn(this.CoreWorkspaceTest);

    private Target Core => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.CoreTest);
}
