using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;

partial class Build
{
    private Target TypescriptE2EAngularBaseScaffold => _ => _
        .Executes(() =>
        {
            DeleteDirectory(Paths.TypescriptE2EBaseE2EGenerated);

            DotNetRun(s => s
                .SetProjectFile(Paths.TypescriptE2EBaseScaffoldProject)
                .SetApplicationArguments($"--output { Paths.TypescriptE2EBaseE2EGenerated } {Paths.TypescriptModulesAppsBaseAngularMaterial}"));
        });

    private Target TypescriptE2EAngularBaseTest => _ => _
         .DependsOn(DotnetBasePublishCommands)
         .DependsOn(DotnetBasePublishServer)
         .DependsOn(DotnetBaseResetDatabase)
         .DependsOn(TypescriptE2EAngularBaseScaffold)
         .Executes(async () =>
         {
             DotNet("Commands.dll Populate", Paths.ArtifactsBaseCommands);

             using var server = new Server(Paths.ArtifactsBaseServer);
             using var angular = new Angular(Paths.TypescriptModules, "base-workspace-angular-material-application:serve");
             await server.Ready();
             await angular.Init();

             DotNetBuild(s => s
                 .SetProjectFile(Paths.TypescriptE2EBaseTestsProject));

             ProcessTasks.StartProcess(Paths.TypescriptE2EBaseTestsPlaywrightCommand, @$"install").WaitForExit();

             DotNetTest(s => s
                 .SetProjectFile(Paths.TypescriptE2EBaseTestsProject)
                 .AddLoggers("trx;LogFileName=TypescriptE2EAngularBase.trx")
                 .SetResultsDirectory(Paths.ArtifactsTests));
         });

}
