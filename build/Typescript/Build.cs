using Nuke.Common;
using Nuke.Common.IO;
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

    private Target TypescriptSystemWorkspaceMeta => _ => _
    .After(TypescriptInstall)
    .DependsOn(DotnetCoreGenerate)
    .DependsOn(EnsureDirectories)
    .Executes(() => NpmRun(s => s
        .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
        .SetProcessWorkingDirectory(Paths.Typescript)
        .SetCommand("system-workspace-meta:test")));


    private Target TypescriptSystemWorkspaceMetaJson => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.Typescript)
            .SetCommand("system-workspace-meta-json:test")));

    private Target TypescriptSystemWorkspaceAdapters => _ => _
        .After(TypescriptInstall)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.Typescript)
            .SetCommand("system-workspace-adapters:test")));

    private Target TypescriptSystemWorkspaceAdaptersJson => _ => _
        .After(TypescriptInstall)
        .DependsOn(EnsureDirectories)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .Executes(async () =>
        {
            using var server = new Server(Paths.ArtifactsCoreServer);
            await server.Ready();
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            NpmRun(s => s
                .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
                .SetProcessWorkingDirectory(Paths.Typescript)
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
