using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target TypescriptInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.Typescript)));

    private Target TypescriptTest => _ => _
         .After(TypescriptInstall)
         .DependsOn(EnsureDirectories)
         .Executes(() => NpmRun(s => s
             .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
             .SetProcessWorkingDirectory(Paths.DotnetSystemWorkspaceTypescript)
             .SetCommand("test:all")));
}
