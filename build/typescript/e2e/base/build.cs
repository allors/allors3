using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target TypescriptE2EAngularBasePrepare => _ => _
        .DependsOn(DotnetBaseGenerate)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("scaffold-base")));

    private Target TypescriptE2EAngularBaseScaffold => _ => _
        .DependsOn(TypescriptE2EAngularBasePrepare)
        .Executes(async () =>
        {

            using var angular = new Angular(Paths.TypescriptModules, "angular-base:serve");
            await angular.Init();
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.TypescriptE2EBase)
                .SetProjectFile(Paths.TypescriptE2EBaseGenerate)
            );
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
            using var angular = new Angular(Paths.TypescriptModules, "angular-base:serve");
            await server.Ready();
            await angular.Init();
            DotNetTest(s => s
                .SetProjectFile(Paths.TypescriptE2EBaseAngularTests)
                .AddLoggers("trx;LogFileName=TypescriptE2EAngularBase.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

}
