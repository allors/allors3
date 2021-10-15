using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target TypescriptE2EBasePrepare => _ => _
        .DependsOn(DotnetBaseGenerate)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.TypescriptModules)
            .SetCommand("scaffold-base")));

    private Target TypescriptE2EBaseScaffold => _ => _
        .DependsOn(TypescriptE2EBasePrepare)
        .Executes(() => DotNetRun(s => s
            .SetProcessWorkingDirectory(Paths.TypescriptE2EBase)
            .SetProjectFile(Paths.TypescriptE2EBaseGenerate)
            ));

    private Target TypescriptE2EBaseTest => _ => _
        .DependsOn(DotnetBasePublishCommands)
        .DependsOn(DotnetBasePublishServer)
        .DependsOn(DotnetBaseResetDatabase)
        .DependsOn(TypescriptE2EBaseScaffold)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsBaseCommands);

            using var server = new Server(Paths.ArtifactsBaseServer);
            using var angular = new Angular(Paths.TypescriptModules, "angular-base:serve");
            await server.Ready();
            await angular.Init();
            DotNetTest(s => s
                .SetProjectFile(Paths.TypescriptE2EBaseAngularTests)
                .AddLoggers("trx;LogFileName=TypescriptE2EBaseAngular.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

}
