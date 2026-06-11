using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build
{
    private Target DotnetIdentityResetDatabase => _ => _
        .DependsOn(DotnetIdentityPublishCommands)
        .Executes(() =>
        {
            SetDatabaseEnvironment("Identity");
            DotNet("Commands.dll Init", Paths.ArtifactsIdentityCommands);
        });

    private Target DotnetIdentityMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.DotnetIdentityDatabaseResourcesIdentity, Paths.DotnetIdentityDatabaseResources)));

    private Target DotnetIdentityGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetIdentityMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetIdentityRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetIdentityDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetIdentity)
                .SetProjectFile(Paths.DotnetIdentityDatabaseGenerate));
        });

    private Target DotnetIdentityDatabaseTestDomain => _ => _
        .DependsOn(DotnetIdentityGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetIdentityDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=IdentityDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetIdentityPublishCommands => _ => _
        .DependsOn(DotnetIdentityGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetIdentityDatabaseCommands)
                .SetOutput(Paths.ArtifactsIdentityCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetIdentityPublishServer => _ => _
        .DependsOn(DotnetIdentityGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetIdentityDatabaseServer)
                .SetOutput(Paths.ArtifactsIdentityServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetIdentityDatabaseTestServerRemote => _ => _
        .DependsOn(DotnetIdentityGenerate)
        .DependsOn(DotnetIdentityPublishServer)
        .DependsOn(DotnetIdentityPublishCommands)
        .DependsOn(DotnetIdentityResetDatabase)
        .Executes(async () =>
        {
            DotNet("Commands.dll Populate", Paths.ArtifactsIdentityCommands);
            using var server = new Server(Paths.ArtifactsIdentityServer);
            await server.Ready();
            DotNetTest(s => s
                .SetProjectFile(Paths.DotnetIdentityDatabaseServerRemoteTests)
                .AddLoggers("trx;LogFileName=IdentityDatabaseServer.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });

    private Target DotnetIdentityDatabaseTest => _ => _
        .DependsOn(DotnetIdentityDatabaseTestDomain)
        .DependsOn(DotnetIdentityDatabaseTestServerRemote);

    private Target DotnetIdentityTest => _ => _
        .DependsOn(DotnetIdentityDatabaseTest);

    private Target DotnetIdentity => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetIdentityTest);
}
