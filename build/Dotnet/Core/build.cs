using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DotnetCoreResetDatabase => _ => _
        .Executes(() =>
        {
            var database = "Core";
            using var sqlServer = new SqlServer();
            sqlServer.Drop(database);
            sqlServer.Create(database);
        });

    private Target DotnetCoreMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(
                $"{Paths.DotnetCoreDatabaseResourcesCore} {Paths.DotnetCoreDatabaseResourcesCustom} {Paths.DotnetCoreDatabaseResources}")));

    private Target DotnetCoreGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetCoreMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{Paths.DotnetCoreRepositoryDomainRepository} {Paths.DotnetSystemRepositoryTemplatesMetaCs} {Paths.DotnetCoreDatabaseMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetCore)
                .SetProjectFile(Paths.DotnetCoreDatabaseGenerate));
        });

    private Target DotnetCoreDatabaseTestMeta => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMetaTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseMeta.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreDatabaseTestDomain => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCoreDatabaseTestServerLocal => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseServerLocalTests)
            .AddLoggers("trx;LogFileName=CoreDatabaseApi.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetCorePublishCommands => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetCoreDatabaseCommands)
                .SetOutput(Paths.ArtifactsCoreCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetCorePublishServer => _ => _
        .DependsOn(DotnetCoreGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetCoreDatabaseServer)
                .SetOutput(Paths.ArtifactsCoreServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetCoreDatabaseTestServerRemote => _ => _
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .DependsOn(DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            using var server = new Server(Paths.ArtifactsCoreServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetCoreDatabaseServerRemoteTests)
                .AddLoggers("trx;LogFileName=CoreDatabaseServer.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .DependsOn(DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);

            {
                DotNetTest(s => s
                    .SetProjectFile(Paths.DotnetCoreWorkspaceTestsLocal)
                    .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsLocal.trx")
                    .SetResultsDirectory(Paths.ArtifactsTests));
            }
        });

    private Target DotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .DependsOn(DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);

           
            {
                using var server = new Server(Paths.ArtifactsCoreServer);
                await server.Ready();

                DotNetTest(s => s
                    .SetProjectFile(Paths.DotnetCoreWorkspaceTestsRemoteJsonSystemText)
                    .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsRemoteJsonSystemText.trx")
                    .SetResultsDirectory(Paths.ArtifactsTests));
            }

        });

    private Target DotnetCoreWorkspaceRemoteJsonRestSharpTest => _ => _
        .DependsOn(DotnetCorePublishServer)
        .DependsOn(DotnetCorePublishCommands)
        .DependsOn(DotnetCoreResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsCoreCommands);
            {
                using var server = new Server(Paths.ArtifactsCoreServer);
                await server.Ready();

                DotNetTest(s => s
                    .SetProjectFile(Paths.DotnetCoreWorkspaceTestsRemoteJsonRestSharp)
                    .AddLoggers("trx;LogFileName=DotnetCoreWorkspaceTestsRemoteJsonRestSharp.trx")
                    .SetResultsDirectory(Paths.ArtifactsTests));
            }
        });

    private Target DotnetCoreDatabaseTest => _ => _
        .DependsOn(DotnetCoreDatabaseTestMeta)
        .DependsOn(DotnetCoreDatabaseTestDomain)
        .DependsOn(DotnetCoreDatabaseTestServerLocal)
        .DependsOn(DotnetCoreDatabaseTestServerRemote);

    private Target DotnetCoreWorkspaceTest => _ => _
        .DependsOn(DotnetCoreWorkspaceLocalTest)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonSystemTextTest)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonRestSharpTest);

    private Target DotnetCoreTest => _ => _
        .DependsOn(DotnetCoreDatabaseTest)
        .DependsOn(DotnetCoreWorkspaceTest);

    private Target DotnetCore => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetCoreTest);
}
