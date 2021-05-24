using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DemosSecurityGenerate => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{Paths.DemosSecurityRepositoryDomainRepository} {Paths.DotnetSystemRepositoryTemplatesMetaCs} {Paths.DemosSecurityDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DemosSecurity)
                .SetProjectFile(Paths.DemosSecurityDatabaseGenerate));
        });

    private Target DemosSecurityTest => _ => _
        .DependsOn(DemosSecurityGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DemosSecurityDatabaseDomainTests)
            .SetLogger("trx;LogFileName=security.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DemosSecurity => _ => _
        .After(Clean)
        .DependsOn(DemosSecurityTest);
}
