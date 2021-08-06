using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target TypescriptInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.Typescript)));

    private Target TypescriptWorkspaceMetaJsonSystem => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.Typescript)
            .SetCommand("test:workspace-meta-json-system")));

    private Target TypescriptWorkspaceAdaptersSystem => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.Typescript)
            .SetCommand("test:workspace-adapters-system")));

    private Target TypescriptWorkspaceAdaptersJsonAsyncSystem => _ => _
        .After(DotnetCoreInstall)
        .After(TypescriptInstall)
        .DependsOn(EnsureDirectories)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .DependsOn(DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using var server = new Server(Paths.ArtifactsCoreServer);
            await server.Ready();
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(Paths.Typescript)
                .SetCommand("test:workspace-adapters-json-async-system"));
        });

    private Target TypescriptWorkspaceAdaptersJsonReactiveSystem => _ => _
        .After(DotnetCoreInstall)
        .After(TypescriptInstall)
        .DependsOn(EnsureDirectories)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .DependsOn(DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using var server = new Server(Paths.ArtifactsCoreServer);
            await server.Ready();
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(Paths.Typescript)
                .SetCommand("test:workspace-adapters-json-reactive-system"));
        });

    private Target TypescriptWorkspaceTest => _ => _
         .After(TypescriptInstall)
         .DependsOn(TypescriptWorkspaceMetaJsonSystem)
         .DependsOn(TypescriptWorkspaceAdaptersSystem)
         .DependsOn(TypescriptWorkspaceAdaptersJsonAsyncSystem)
         .DependsOn(TypescriptWorkspaceAdaptersJsonReactiveSystem);

    private Target TypescriptTest => _ => _
        .DependsOn(TypescriptWorkspaceTest);
}
