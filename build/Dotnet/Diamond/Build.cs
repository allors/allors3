using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// The Diamond tree: the platform test domains Level1 and Level2, the test plug-in Plugin1 and the
// concrete Test domain that extends Level2 and Plugin1, so the population is a diamond:
// Core <- Level1 <- Level2 <- Test and Core <- Plugin1 <- Test. It has a database side with
// domain tests on the memory adapter, and no commands, server or workspace.
partial class Build
{
    private Target DotnetDiamondMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                Paths.DotnetCoreDatabaseResourcesCore,
                Paths.DotnetDiamondDatabaseResourcesLevel1,
                Paths.DotnetDiamondDatabaseResourcesLevel2,
                Paths.DotnetDiamondDatabaseResourcesPlugin1,
                Paths.DotnetDiamondDatabaseResourcesTest,
                Paths.DotnetDiamondDatabaseResources)));

    private Target DotnetDiamondGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetDiamondMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetDiamondRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetDiamondDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetDiamond)
                .SetProjectFile(Paths.DotnetDiamondDatabaseGenerate));
        });

    private Target DotnetDiamondDatabaseTestDomain => _ => _
        .DependsOn(DotnetDiamondGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetDiamondDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=DiamondDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetDiamondDatabaseTest => _ => _
        .DependsOn(DotnetDiamondDatabaseTestDomain);

    private Target DotnetDiamondTest => _ => _
        .DependsOn(DotnetDiamondDatabaseTest);

    private Target DotnetDiamond => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetDiamondTest);
}
