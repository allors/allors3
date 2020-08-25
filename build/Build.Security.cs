using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    Target SecurityGenerate => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.PlatformRepositoryGenerate)
                .SetApplicationArguments($"{Paths.SecurityRepositoryDomainRepository} {Paths.PlatformRepositoryTemplatesMetaCs} {Paths.SecurityDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetWorkingDirectory(Paths.Security)
                .SetProjectFile(Paths.SecurityDatabaseGenerate));
        });

    Target SecurityTest => _ => _
        .DependsOn(SecurityGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.SecurityDatabaseDomainTests)
                .SetLogger("trx;LogFileName=security.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    Target Security => _ => _
        .After(Clean)
        .DependsOn(SecurityTest);
}
