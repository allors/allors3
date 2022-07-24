using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;

partial class Build
{
    private Target TypescriptE2EAngularAppsIntranetScaffold => _ => _
        .Executes(() =>
        {
            DeleteDirectory(Paths.TypescriptE2EAppsIntranetE2EGenerated);

            DotNetRun(s => s
                .SetProjectFile(Paths.TypescriptE2EAppsIntranetScaffoldProject)
                .SetApplicationArguments($"--output { Paths.TypescriptE2EAppsIntranetE2EGenerated } {Paths.TypescriptModulesAppsAppsIntranetAngularMaterial}"));
        });

    private Target TypescriptE2EAngularAppsIntranetTest => _ => _
        .DependsOn(DotnetAppsPublishCommands)
        .DependsOn(DotnetAppsPublishServer)
        .DependsOn(DotnetAppsResetDataapps)
        .DependsOn(TypescriptE2EAngularAppsIntranetScaffold)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsAppsCommands);

            using var server = new Server(Paths.ArtifactsAppsServer);
            using var angular = new Angular(Paths.TypescriptModules, "angular-apps-intranet:serve");
            await server.Ready();
            await angular.Init();

            DotNetBuild(s => s
                .SetProjectFile(Paths.TypescriptE2EAppsIntranetTestsProject));

            ProcessTasks.StartProcess(Paths.TypescriptE2EAppsIntranetTestsPlaywrightCommand, @$"install").WaitForExit();

            DotNetTest(s => s
                .SetProjectFile(Paths.TypescriptE2EAppsIntranetTestsProject)
                .AddLoggers("trx;LogFileName=TypescriptE2EAngularApps.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

}