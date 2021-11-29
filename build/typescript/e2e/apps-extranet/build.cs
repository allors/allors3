using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target TypescriptE2EAngularAppsExtranetTest => _ => _
        .DependsOn(DotnetAppsPublishCommands)
        .DependsOn(DotnetAppsPublishServer)
        .DependsOn(DotnetAppsResetDataapps)
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
