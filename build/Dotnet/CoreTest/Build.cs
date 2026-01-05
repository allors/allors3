using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DotnetCoreTestMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.DotnetTestCoreDatabaseResourcesCustom,
                Paths.DotnetCoreTestDatabaseResources)));

    private Target DotnetCoreTestGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetCoreTestMerge)
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

    private Target DotnetCoreTestDatabaseTestMeta => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreTestDatabaseMetaTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseMeta.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreTestDatabaseTestDomain => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreTestDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreTestDatabaseTestServerLocal => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreTestDatabaseServerLocalTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseApi.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreTestPublishServer => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetCoreTestDatabaseServer)
                .SetOutput(Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetCoreTestDatabaseTestServerRemote => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetTestCoreDatabaseServerRemoteTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseServer.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreTestWorkspaceLocalTest => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetTestCoreWorkspaceTestsLocal)
                .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsLocal.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreTestWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(async () =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetCoreTestWorkspaceTestsRemoteJsonSystemText)
                .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsRemoteJsonSystemText.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreTestWorkspaceRemoteJsonNewtonsoftTest => _ => _
        .DependsOn(DotnetCoreTestGenerate)
        .Executes(async () =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetCoreTestWorkspaceTestsRemoteNewtonsoftSharp)
                .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsRemoteJsonNewtonsoft.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreTestDatabaseTest => _ => _
        .DependsOn(DotnetCoreTestDatabaseTestMeta)
        .DependsOn(DotnetCoreTestDatabaseTestDomain)
        .DependsOn(DotnetCoreTestDatabaseTestServerLocal)
        .DependsOn(DotnetCoreTestDatabaseTestServerRemote);

    private Target DotnetCoreTestWorkspaceTest => _ => _
        .DependsOn(DotnetCoreTestWorkspaceLocalTest)
        .DependsOn(DotnetCoreTestWorkspaceRemoteJsonSystemTextTest)
        .DependsOn(DotnetCoreTestWorkspaceRemoteJsonNewtonsoftTest);

    private Target DotnetCoreTest => _ => _
        .DependsOn(DotnetCoreTestDatabaseTest)
        .DependsOn(DotnetCoreTestWorkspaceTest);
}