using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DemosSecurityMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{Paths.DotnetCoreDatabaseResourcesCore} {Paths.DemosSecurityDatabaseResources}")));

    private Target DemosSecurityGenerate => _ => _
        .After(Clean)
        .DependsOn(DemosSecurityMerge)
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
            .AddLoggers("trx;LogFileName=security.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DemosSecurity => _ => _
        .After(Clean)
        .DependsOn(DemosSecurityTest);
}
