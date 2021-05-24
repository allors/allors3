using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DemosDerivationGenerate => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{Paths.DemosDerivationRepositoryDomainRepository} {Paths.DotnetSystemRepositoryTemplatesMetaCs} {Paths.DemosDerivationDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DemosDerivation)
                .SetProjectFile(Paths.DemosDerivationDatabaseGenerate));
        });

    private Target DemosDerivationTest => _ => _
        .DependsOn(DemosDerivationGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DemosDerivationDatabaseDomainTests)
            .SetLogger("trx;LogFileName=next.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DemosDerivation => _ => _
        .After(Clean)
        .DependsOn(DemosDerivationTest);
}
