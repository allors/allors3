using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
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
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using (var server = new Server(Paths.ArtifactsCoreServer))
            {
                await server.Ready();
                DotNetTest(s => s
                    .SetProjectFile(Paths.CoreDatabaseServerTests)
                    .SetLogger("trx;LogFileName=CoreDatabaseServer.trx")
                    .SetResultsDirectory(Paths.ArtifactsTests));
            }
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
        .DependsOn(CoreGenerate)
        .DependsOn(CorePublishServer)
        .DependsOn(CorePublishCommands)
        .DependsOn(EnsureDirectories)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using (var server = new Server(Paths.ArtifactsCoreServer))
            {
                await server.Ready();
                NpmRun(s => s
                    .SetEnvironmentVariable("npm_config_loglevel", "error")
                    .SetWorkingDirectory(Paths.CoreWorkspaceTypescript)
                    .SetCommand("test:client"));
            }
        });

    Target CoreWorkspaceCSharpDomainTests => _ => _
        .DependsOn(CorePublishServer)
        .DependsOn(CorePublishCommands)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using (var server = new Server(Paths.ArtifactsCoreServer))
            {
                await server.Ready();
                DotNetTest(s => s
                    .SetProjectFile(Paths.CoreWorkspaceCSharpDomainTests)
                    .SetLogger("trx;LogFileName=CoreWorkspaceCSharpDomainTests.trx")
                    .SetResultsDirectory(Paths.ArtifactsTests));
            }
        });

    Target CoreDatabaseTest => _ => _
        .DependsOn(CoreDatabaseTestDomain)
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
