using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    Target SecurityGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.SystemRepositoryGenerate)
                .SetApplicationArguments($"{this.Paths.SecurityRepositoryDomainRepository} {this.Paths.SystemRepositoryTemplatesMetaCs} {this.Paths.SecurityDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.Security)
                .SetProjectFile(this.Paths.SecurityDatabaseGenerate));
        });

    Target SecurityTest => _ => _
        .DependsOn(this.SecurityGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(this.Paths.SecurityDatabaseDomainTests)
                .SetLogger("trx;LogFileName=security.trx")
                .SetResultsDirectory(this.Paths.ArtifactsTests));
        });

    Target Security => _ => _
        .After(this.Clean)
        .DependsOn(this.SecurityTest);
}
