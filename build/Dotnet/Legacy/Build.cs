using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DotnetLegacyMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{Paths.DotnetCoreDatabaseResourcesCore} {Paths.DotnetLegacyDatabaseResourcesLegacy} {Paths.DotnetLegacyDatabaseResources}")));

    private Target DotnetLegacyGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetLegacyMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{Paths.DotnetLegacyRepositoryDomainRepository} {Paths.DotnetSystemRepositoryTemplatesMetaCs} {Paths.DotnetLegacyDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetLegacy)
                .SetProjectFile(Paths.DotnetLegacyDatabaseGenerate));
        });

    private Target DotnetLegacyDatabaseTestDomain => _ => _
        .DependsOn(DotnetLegacyGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetLegacyDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=LegacyDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetLegacyDatabaseTest => _ => _
        .DependsOn(DotnetLegacyDatabaseTestDomain);

    private Target DotnetLegacyTest => _ => _
        .DependsOn(DotnetLegacyDatabaseTest);

    private Target DotnetLegacy => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetLegacyTest);
}
