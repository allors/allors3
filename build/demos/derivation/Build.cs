using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DemosDerivationGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DemosDerivationRepositoryDomainRepository} {this.Paths.DotnetSystemRepositoryTemplatesMetaCs} {this.Paths.DemosDerivationDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.DemosDerivation)
                .SetProjectFile(this.Paths.DemosDerivationDatabaseGenerate));
        });

    private Target DemosDerivationTest => _ => _
        .DependsOn(this.DemosDerivationGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DemosDerivationDatabaseDomainTests)
            .SetLogger("trx;LogFileName=next.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DemosDerivation => _ => _
        .After(this.Clean)
        .DependsOn(this.DemosDerivationTest);
}
