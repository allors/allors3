using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    Target DerivationGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.PlatformRepositoryGenerate)
                .SetApplicationArguments($"{this.Paths.DerivationRepositoryDomainRepository} {this.Paths.PlatformRepositoryTemplatesMetaCs} {this.Paths.DerivationDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetWorkingDirectory(this.Paths.Derivation)
                .SetProjectFile(this.Paths.DerivationDatabaseGenerate));
        });

    Target DerivationTest => _ => _
        .DependsOn(this.DerivationGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(this.Paths.DerivationDatabaseDomainTests)
                .SetLogger("trx;LogFileName=next.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target Derivation => _ => _
        .After(this.Clean)
        .DependsOn(this.DerivationTest);
}
