using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DotnetCoreMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.DotnetTestCoreDatabaseResourcesCustom,
                Paths.DotnetCoreTestDatabaseResources)));

    private Target DotnetCoreGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetCoreMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetCoreTestRepositoryDomainRepository,
                    Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetCoreTestDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetCoreTest)
                .SetProjectFile(Paths.DotnetCoreTestDatabaseGenerate));
        });

    private Target DotnetCoreDatabaseTestMeta => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreTestDatabaseMetaTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseMeta.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreDatabaseTestDomain => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreTestDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreDatabaseTestServerLocal => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreTestDatabaseServerLocalTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseApi.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCorePublishServer => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetCoreTestDatabaseServer)
                .SetOutput(Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetCoreDatabaseTestServerRemote => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetTestCoreDatabaseServerRemoteTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseServer.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetTestCoreWorkspaceTestsLocal)
                .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsLocal.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(async () =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetCoreTestWorkspaceTestsRemoteJsonSystemText)
                .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsRemoteJsonSystemText.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreWorkspaceRemoteJsonNewtonsoftTest => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(async () =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetCoreTestWorkspaceTestsRemoteNewtonsoftSharp)
                .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsRemoteJsonNewtonsoft.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreDatabaseTest => _ => _
        .DependsOn(DotnetCoreDatabaseTestMeta)
        .DependsOn(DotnetCoreDatabaseTestDomain)
        .DependsOn(DotnetCoreDatabaseTestServerLocal)
        .DependsOn(DotnetCoreDatabaseTestServerRemote);

    private Target DotnetCoreWorkspaceTest => _ => _
        .DependsOn(DotnetCoreWorkspaceLocalTest)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonSystemTextTest)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonNewtonsoftTest);

    private Target DotnetCoreTest => _ => _
        .DependsOn(DotnetCoreDatabaseTest)
        .DependsOn(DotnetCoreWorkspaceTest);

    private Target DotnetCore => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetCoreTest);
}