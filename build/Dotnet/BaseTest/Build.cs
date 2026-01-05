using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetBaseTestDatabaseTest => _ => _
        .DependsOn(DotnetBaseTestDatabaseTestDomain);

    private Target DotnetBaseTestMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.DotnetBaseTestDatabaseResourcesBase, Paths.DotnetBaseTestDatabaseResources)));

    private Target DotnetBaseTestDatabaseTestDomain => _ => _
        .DependsOn(DotnetBaseTestGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetBaseTestDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=BaseDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetBaseTestGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetBaseTestMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetBaseTestRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetBaseTestDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetBaseTest)
                .SetProjectFile(Paths.DotnetBaseTestDatabaseGenerate));
        });

    private Target DotnetBaseTestPublishServer => _ => _
        .DependsOn(DotnetBaseTestGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetBaseTestDatabaseServer)
                .SetOutput(Paths.ArtifactsBaseServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetBaseTestWorkspaceTypescriptSession => _ => _
        .DependsOn(DotnetBaseTestGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.DotnetBaseTestWorkspaceTypescript)
            .SetCommand("domain:test")));

    private Target DotnetBaseTestWorkspaceTypescriptTest => _ => _
        .DependsOn(DotnetBaseTestWorkspaceTypescriptSession);

    private Target DotnetBaseTestWorkspaceTest => _ => _
        .DependsOn(DotnetBaseTestWorkspaceTypescriptTest);

    private Target DotnetBaseTest => _ => _
        .DependsOn(DotnetBaseTestDatabaseTest)
        .DependsOn(DotnetBaseTestWorkspaceTypescriptTest);
}
