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
            .SetProcessWorkingDirectory(Paths.TypescriptModules)));

    private Target TypescriptWorkspaceMetaJsonSystem => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("test:workspace-meta-json-system")));

    private Target TypescriptWorkspaceAdaptersSystem => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("test:workspace-adapters-system")));

    private Target TypescriptWorkspaceAdaptersJsonSystemAsync => _ => _
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
                .SetProcessWorkingDirectory(Paths.TypescriptModules)
                .SetCommand("test:workspace-adapters-json-system-async"));
        });

    private Target TypescriptWorkspaceAdaptersJsonSystemReactive => _ => _
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
                .SetProcessWorkingDirectory(Paths.TypescriptModules)
                .SetCommand("test:workspace-adapters-json-system-reactive"));
        });

    private Target TypescriptWorkspaceTest => _ => _
         .After(TypescriptInstall)
         .DependsOn(TypescriptWorkspaceMetaJsonSystem)
         .DependsOn(TypescriptWorkspaceAdaptersSystem);

    private Target TypescriptWorkspaceAsyncTest => _ => _
        .After(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceAdaptersJsonSystemAsync);

    private Target TypescriptWorkspaceReactiveTest => _ => _
        .After(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceAdaptersJsonSystemReactive);

}
