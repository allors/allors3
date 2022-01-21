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

    private Target TypescriptSystemWorkspaceMeta => _ => _
    .After(TypescriptInstall)
    .DependsOn(DotnetCoreGenerate)
    .DependsOn(EnsureDirectories)
    .Executes(() => NpmRun(s => s
        .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
        .SetProcessWorkingDirectory(Paths.TypescriptModules)
        .SetCommand("system-workspace-meta:test")));


    private Target TypescriptSystemWorkspaceMetaJson => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("system-workspace-meta-json:test")));

    private Target TypescriptSystemWorkspaceAdapters => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("system-workspace-adapters:test")));

    private Target TypescriptSystemWorkspaceAdaptersJson => _ => _
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
                .SetCommand("system-workspace-adapters-json:test"));
        });

    private Target TypescriptWorkspaceTest => _ => _
         .After(TypescriptInstall)
         .DependsOn(TypescriptSystemWorkspaceMeta)
         .DependsOn(TypescriptSystemWorkspaceMetaJson)
         .DependsOn(TypescriptSystemWorkspaceAdapters);

    private Target TypescriptWorkspaceAdaptersJsonTest => _ => _
        .After(TypescriptInstall)
        .DependsOn(TypescriptSystemWorkspaceAdaptersJson);
}
