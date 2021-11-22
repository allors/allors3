using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target TypescriptE2EAppsIntranetPrepare => _ => _
        .DependsOn(DotnetAppsGenerate)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("scaffold-apps-intranet")));

    private Target TypescriptE2EAppsIntranetScaffold => _ => _
        .DependsOn(TypescriptE2EAppsIntranetPrepare)
        .Executes(async () =>
        {

            using var angular = new Angular(Paths.TypescriptModules, "angular-apps:serve");
            await angular.Init();
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.TypescriptE2EApps)
                .SetProjectFile(Paths.TypescriptE2EAppsIntranetGenerate)
            );
        });

    private Target TypescriptE2EAppsIntranetTest => _ => _
        .DependsOn(DotnetAppsPublishCommands)
        .DependsOn(DotnetAppsPublishServer)
        .DependsOn(DotnetAppsResetDataapps)
        .DependsOn(TypescriptE2EAppsIntranetScaffold)
        .Executes(async () =>
        {

            DotNet("Commands.dll Populate", Paths.ArtifactsAppsCommands);

            using var server = new Server(Paths.ArtifactsAppsServer);
            using var angular = new Angular(Paths.TypescriptModules, "angular-apps:serve");
            await server.Ready();
            await angular.Init();
            DotNetTest(s => s
                .SetProjectFile(Paths.TypescriptE2EAppsIntranetAngularTests)
                .AddLoggers("trx;LogFileName=TypescriptE2EAppsAngular.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

}
