using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetBaseResetDatabase => _ => _
        .DependsOn(DotnetBasePublishCommands)
        .Executes(() =>
        {
            SetDatabaseEnvironment("Base");
            DotNet("Commands.dll Init", Paths.ArtifactsBaseCommands);
        });

    private Target DotnetBaseDatabaseTest => _ => _
        .DependsOn(DotnetBaseDatabaseTestDomain);

    private Target DotnetBaseMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.DotnetBaseDatabaseResourcesBase, Paths.DotnetBaseDatabaseResources)));

    private Target DotnetBaseDatabaseTestDomain => _ => _
        .DependsOn(DotnetBaseGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetBaseDatabaseDomainTests)
            .AddLoggers("trx;LogFileName=BaseDatabaseDomain.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetBaseGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetBaseMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetBaseRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetBaseDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetBase)
                .SetProjectFile(Paths.DotnetBaseDatabaseGenerate));
        });

    private Target DotnetBasePublishCommands => _ => _
        .DependsOn(DotnetBaseGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetBaseDatabaseCommands)
                .SetOutput(Paths.ArtifactsBaseCommands);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetBasePublishServer => _ => _
        .DependsOn(DotnetBaseGenerate)
        .Executes(() =>
        {
            var dotNetPublishSettings = new DotNetPublishSettings()
                .SetProcessWorkingDirectory(Paths.DotnetBaseDatabaseServer)
                .SetOutput(Paths.ArtifactsBaseServer);
            DotNetPublish(dotNetPublishSettings);
        });

    private Target DotnetBaseWorkspaceTypescriptSession => _ => _
        .DependsOn(DotnetBaseGenerate)
        .DependsOn(EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(Paths.DotnetBaseWorkspaceTypescript)
            .SetCommand("domain:test")));

    private Target DotnetBaseWorkspaceTypescriptTest => _ => _
        .DependsOn(DotnetBaseWorkspaceTypescriptSession);

    // bUnit tests over the Blazor role components; they run against an in-memory workspace, so no
    // database is needed.
    private Target DotnetBaseWorkspaceBlazorTest => _ => _
        .DependsOn(DotnetBaseGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(Paths.DotnetBaseWorkspaceBlazorBootstrapTests)
            .AddLoggers("trx;LogFileName=BaseWorkspaceBlazor.trx")
            .SetResultsDirectory(Paths.ArtifactsTests)));

    private Target DotnetBaseWorkspaceTest => _ => _
        .DependsOn(DotnetBaseWorkspaceTypescriptTest)
        .DependsOn(DotnetBaseWorkspaceBlazorTest);

    private Target DotnetBaseTest => _ => _
        .DependsOn(DotnetBaseDatabaseTest)
        .DependsOn(DotnetBaseWorkspaceTypescriptTest)
        .DependsOn(DotnetBaseWorkspaceBlazorTest);

    private Target DotnetBase => _ => _
        .DependsOn(Clean)
        .DependsOn(DotnetBaseTest);
}
