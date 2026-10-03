using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// The Entra tree: Core <- Entra <- Test. Its Test server selects the Entra plug-in, so users sign in
// with Microsoft Entra ID; against a fake Entra of its own in the tests. It has no workspace.
partial class Build
{
    private Target DotnetEntraResetDatabase => _ => _
        .DependsOn(DotnetEntraPublishCommands)
        .Executes(() =>
        {
            SetDatabaseEnvironment("Entra");
            DotNet("Commands.dll Init", Paths.ArtifactsEntraCommands);
        });

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

    private Target DotnetEntraDatabaseTestServerLocal => _ => _
        .DependsOn(DotnetEntraGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetEntraDatabaseServerLocalTests)
            .AddLoggers("trx;LogFileName=EntraDatabaseApi.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetEntraPublishCommands => _ => _
        .DependsOn(DotnetEntraGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetEntraDatabaseCommands)
                .SetOutput(Paths.ArtifactsEntraCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetEntraPublishServer => _ => _
        .DependsOn(DotnetEntraGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetEntraDatabaseServer)
                .SetOutput(Paths.ArtifactsEntraServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetEntraDatabaseTestServerRemote => _ => _
        .DependsOn(DotnetEntraGenerate)
        .DependsOn(DotnetEntraPublishServer)
        .DependsOn(DotnetEntraPublishCommands)
        .DependsOn(DotnetEntraResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsEntraCommands);
            using var server = new Server(Paths.ArtifactsEntraServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetEntraDatabaseServerRemoteTests)
                .AddLoggers("trx;LogFileName=EntraDatabaseServer.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetEntraDatabaseTest => _ => _
        .DependsOn(DotnetEntraDatabaseTestDomain)
        .DependsOn(DotnetEntraDatabaseTestServerLocal)
        .DependsOn(DotnetEntraDatabaseTestServerRemote);

    private Target DotnetEntraTest => _ => _
        .DependsOn(DotnetEntraDatabaseTest);

    private Target DotnetEntra => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetEntraTest);
}
