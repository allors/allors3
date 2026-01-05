using Nuke.Common;

partial class Build
{
    private Target CiDotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetSystemAdaptersTestMemory);

    private Target CiDotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetSystemAdaptersTestSqlClient);

    private Target CiDotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetSystemAdaptersTestNpgsql);

    private Target CiDotnetSystemAdaptersTestUnified => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetSystemAdaptersTestUnified);

    private Target CiDotnetCoreDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreTestDatabaseTest);

    private Target CiDotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreTestWorkspaceLocalTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreTestWorkspaceRemoteJsonSystemTextTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonNewtonsoftTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreTestWorkspaceRemoteJsonNewtonsoftTest);

    private Target CiDotnetBaseDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetBaseTestDatabaseTest);

    private Target CiDotnetBaseWorkspaceTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetBaseTestWorkspaceTest);

    private Target CiDotnetAppsDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetAppsDatabaseTest);

    private Target CiDotnetAppsWorkspaceTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetAppsWorkspaceTest);

    private Target CiTypescriptWorkspaceTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceTest);

    private Target CiTypescriptWorkspaceAdaptersJsonTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceAdaptersJsonTest);

    private Target CiTypescriptBaseWorkspaceE2e => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptBaseWorkspaceE2e);

    private Target CiTypescriptAppsWorkspaceE2e => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptAppsWorkspaceE2e);
}
