using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DerivationGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DerivationRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.DerivationDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.Derivation)
                .SetProjectFile(this.Paths.DerivationDatabaseGenerate));
        });

    private Target DerivationTest => _ => _
        .DependsOn(this.DerivationGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DerivationDatabaseDomainTests)
            .SetLogger("trx;LogFileName=next.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target Derivation => _ => _
        .After(this.Clean)
        .DependsOn(this.DerivationTest);
}
