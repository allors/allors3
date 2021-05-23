using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetCoreResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Core";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target DotnetCoreMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(this.Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{this.Paths.DotnetCoreDatabaseResourcesCore} {this.Paths.DotnetCoreDatabaseResourcesCustom} {this.Paths.DotnetCoreDatabaseResources}")));

    private Target DotnetCoreGenerate => _ => _
        .After(this.Clean)
        .DependsOn(this.DotnetCoreMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DotnetCoreRepositoryDomainRepository} {this.Paths.DotnetSystemRepositoryTemplatesMetaCs} {this.Paths.DotnetCoreDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.DotnetCore)
                .SetProjectFile(this.Paths.DotnetCoreDatabaseGenerate));
        });

    private Target DotnetCoreDatabaseTestMeta => _ => _
        .DependsOn(this.DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DotnetCoreDatabaseMetaTests)
            .SetLogger("trx;LogFileName=CoreDatabaseMeta.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DotnetCoreDatabaseTestDomain => _ => _
        .DependsOn(this.DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DotnetCoreDatabaseDomainTests)
            .SetLogger("trx;LogFileName=CoreDatabaseDomain.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DotnetCoreDatabaseTestServerLocal => _ => _
        .DependsOn(this.DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DotnetCoreDatabaseServerLocalTests)
            .SetLogger("trx;LogFileName=CoreDatabaseApi.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DotnetCorePublishCommands => _ => _
        .DependsOn(this.DotnetCoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.DotnetCoreDatabaseCommands)
                .SetOutput(this.Paths.ArtifactsCoreCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetCorePublishServer => _ => _
        .DependsOn(this.DotnetCoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(this.Paths.DotnetCoreDatabaseServer)
                .SetOutput(this.Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetCoreDatabaseTestServerRemote => _ => _
        .DependsOn(this.DotnetCoreGenerate)
        .DependsOn(this.DotnetCorePublishServer)
        .DependsOn(this.DotnetCorePublishCommands)
        .DependsOn(this.DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(this.Paths.DotnetCoreDatabaseServerRemoteTests)
                .SetLogger("trx;LogFileName=CoreDatabaseServer.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    private Target DotnetCoreInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetCoreWorkspaceTypescript)));

    private Target DotnetCoreWorkspaceTypescriptMeta => _ => _
        .After(this.DotnetCoreInstall)
        .DependsOn(this.DotnetCoreGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetCoreWorkspaceTypescript)
            .SetCommand("test:meta")));

    private Target DotnetCoreWorkspaceTypescriptWorkspace => _ => _
        .After(this.DotnetCoreInstall)
        .DependsOn(this.DotnetCoreGenerate)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetCoreWorkspaceTypescript)
            .SetCommand("test:workspace")));

    private Target DotnetCoreWorkspaceTypescriptClient => _ => _
        .After(this.DotnetCoreInstall)
        .DependsOn(this.EnsureDirectories)
        .DependsOn(this.DotnetCoreGenerate)
        .DependsOn(this.DotnetCorePublishServer)
        .DependsOn(this.DotnetCorePublishCommands)
        .DependsOn(this.DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);
            using var server = new Server(this.Paths.ArtifactsCoreServer);
            await server.Ready();
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(this.Paths.DotnetCoreWorkspaceTypescript)
                .SetCommand("test:client"));
        });

    private Target DotnetCoreWorkspaceCSharpTest => _ => _
        .DependsOn(this.DotnetCorePublishServer)
        .DependsOn(this.DotnetCorePublishCommands)
        .DependsOn(this.DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", this.Paths.ArtifactsCoreCommands);

            {
                DotNetTest(s => s
                    .SetProjectFile(this.Paths.DotnetCoreWorkspaceCSharpAdaptersLocalTests)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpAdaptersLocalTests.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));

                DotNetTest(s => s
                    .SetProjectFile(this.Paths.DotnetCoreWorkspaceCSharpTestsLocal)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpTestsLocal.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));
            }

            {
                using var server = new Server(this.Paths.ArtifactsCoreServer);
                await server.Ready();

                DotNetTest(s => s
                    .SetProjectFile(this.Paths.DotnetCoreWorkspaceCSharpAdaptersRemoteTests)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpAdaptersRemoteTests.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));

                DotNetTest(s => s
                    .SetProjectFile(this.Paths.DotnetCoreWorkspaceCSharpTestsRemote)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpTestsRemote.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));
            }
        });

    private Target DotnetCoreDatabaseTest => _ => _
        .DependsOn(this.DotnetCoreDatabaseTestMeta)
        .DependsOn(this.DotnetCoreDatabaseTestDomain)
        .DependsOn(this.DotnetCoreDatabaseTestServerLocal)
        .DependsOn(this.DotnetCoreDatabaseTestServerRemote);

    private Target DotnetCoreWorkspaceTypescriptTest => _ => _
        .DependsOn(this.DotnetCoreWorkspaceTypescriptMeta)
        .DependsOn(this.DotnetCoreWorkspaceTypescriptWorkspace)
        .DependsOn(this.DotnetCoreWorkspaceTypescriptClient);

    private Target DotnetCoreWorkspaceTest => _ => _
        .DependsOn(this.DotnetCoreWorkspaceCSharpTest);
        //.DependsOn(this.CoreWorkspaceTypescriptTest);

    private Target DotnetCoreTest => _ => _
        .DependsOn(this.DotnetCoreDatabaseTest)
        .DependsOn(this.DotnetCoreWorkspaceTest);

    private Target DotnetCore => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.DotnetCoreTest);
}
