using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target TypescriptE2EAngularAppsExtranetPrepare => _ => _
        .DependsOn(DotnetAppsGenerate)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("scaffold-apps-intranet")));

    private Target TypescriptE2EAngularAppsExtranetScaffold => _ => _
        .DependsOn(TypescriptE2EAngularAppsExtranetPrepare)
        .Executes(async () =>
        {

            using var angular = new Angular(Paths.TypescriptModules, "angular-apps-intranet:serve");
            await angular.Init();
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.TypescriptE2EAppsExtranet)
                .SetProjectFile(Paths.TypescriptE2EAppsExtranetGenerate)
            );
        });

    private Target TypescriptE2EAngularAppsExtranetTest => _ => _
        .DependsOn(DotnetAppsPublishCommands)
        .DependsOn(DotnetAppsPublishServer)
        .DependsOn(DotnetAppsResetDataapps)
        .DependsOn(TypescriptE2EAngularAppsExtranetScaffold)
        .Executes(async () =>
        {

            DotNet("Commands.dll Populate", Paths.ArtifactsAppsCommands);

            using var server = new Server(Paths.ArtifactsAppsServer);
            using var angular = new Angular(Paths.TypescriptModules, "angular-apps-intranet:serve");
            await server.Ready();
            await angular.Init();
            DotNetTest(s => s
                .SetProjectFile(Paths.TypescriptE2EAppsExtranetAngularTests)
                .AddLoggers("trx;LogFileName=TypescriptE2EAngularAppsExtranet.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });
}
