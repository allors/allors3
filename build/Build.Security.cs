using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target SecurityGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.SecurityRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.SecurityDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.Security)
                .SetProjectFile(this.Paths.SecurityDatabaseGenerate));
        });

    private Target SecurityTest => _ => _
        .DependsOn(this.SecurityGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.SecurityDatabaseDomainTests)
            .SetLogger("trx;LogFileName=security.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target Security => _ => _
        .After(this.Clean)
        .DependsOn(this.SecurityTest);
}
