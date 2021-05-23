using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DemosSecurityGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DemosSecurityRepositoryDomainRepository} {this.Paths.DotnetSystemRepositoryTemplatesMetaCs} {this.Paths.DemosSecurityDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.DemosSecurity)
                .SetProjectFile(this.Paths.DemosSecurityDatabaseGenerate));
        });

    private Target DemosSecurityTest => _ => _
        .DependsOn(this.DemosSecurityGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DemosSecurityDatabaseDomainTests)
            .SetLogger("trx;LogFileName=security.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DemosSecurity => _ => _
        .After(this.Clean)
        .DependsOn(this.DemosSecurityTest);
}
