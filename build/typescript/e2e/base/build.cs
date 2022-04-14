using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target TypescriptE2EAngularBaseTest => _ => _
         .DependsOn(DotnetBasePublishCommands)
         .DependsOn(DotnetBasePublishServer)
         .DependsOn(DotnetBaseResetDatabase)
         .Executes(async () =>
         {
             DotNet("Commands.dll Populate", Paths.ArtifactsBaseCommands);

             using var server = new Server(Paths.ArtifactsBaseServer);
             using var angular = new Angular(Paths.TypescriptModules, "base-workspace-angular-material-application:serve");
             await server.Ready();
             await angular.Init();

             DotNetBuild(s => s
                 .SetProjectFile(Paths.TypescriptE2EBaseAngularTests));

             ProcessTasks.StartProcess(Paths.TypescriptE2EBaseAngularPlaywright, @$"install").WaitForExit();

             DotNetTest(s => s
                 .SetProjectFile(Paths.TypescriptE2EBaseAngularTests)
                 .AddLoggers("trx;LogFileName=TypescriptE2EAngularBase.trx")
                 .SetResultsDirectory(Paths.ArtifactsTests));
         });

}
