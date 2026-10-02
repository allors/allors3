using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// The Entra tree: Core <- Entra <- Test. Its Test domain selects the Entra plug-in, so users
// sign in with Microsoft Entra ID. It has no workspace.
partial class Build
{
    private Target DotnetEntraMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                Paths.DotnetCoreDatabaseResourcesCore,
                Paths.DotnetEntraDatabaseResourcesEntra,
                Paths.DotnetEntraDatabaseResourcesTest,
                Paths.DotnetEntraDatabaseResources)));

    private Target DotnetEntraGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetEntraMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetEntraRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetEntraDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetEntra)
                .SetProjectFile(Paths.DotnetEntraDatabaseGenerate));
        });

    private Target DotnetEntraDatabaseTestDomain => _ => _
        .DependsOn(DotnetEntraGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetEntraDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=EntraDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetEntraDatabaseTest => _ => _
        .DependsOn(DotnetEntraDatabaseTestDomain);

    private Target DotnetEntraTest => _ => _
        .DependsOn(DotnetEntraDatabaseTest);

    private Target DotnetEntra => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetEntraTest);
}
