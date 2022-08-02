using Nuke.Common;

partial class Build
{
    private Target CiDotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(DotnetSystemAdaptersTestMemory);

    private Target CiDotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(DotnetSystemAdaptersTestSqlClient);

    private Target CiDotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(DotnetSystemAdaptersTestNpgsql);

    private Target CiDotnetCoreDatabaseTest => _ => _
        .DependsOn(DotnetCoreDatabaseTest);

    private Target CiDotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(DotnetCoreWorkspaceLocalTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(DotnetCoreWorkspaceRemoteJsonSystemTextTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonRestSharpTest => _ => _
        .DependsOn(DotnetCoreWorkspaceRemoteJsonRestSharpTest);

    private Target CiDotnetLegacyDatabaseTest => _ => _
        .DependsOn(DotnetLegacyDatabaseTest);

    private Target CiDotnetBaseDatabaseTest => _ => _
        .DependsOn(DotnetBaseDatabaseTest);

    private Target CiDotnetBaseWorkspaceTest => _ => _
        .DependsOn(DotnetBaseWorkspaceTest);

    private Target CiDotnetAppsDatabaseTest => _ => _
        .DependsOn(DotnetAppsDatabaseTest);

    private Target CiDotnetAppsWorkspaceTest => _ => _
        .DependsOn(DotnetAppsWorkspaceTest);

    private Target CiTypescriptWorkspaceTest => _ => _
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceTest);

    private Target CiTypescriptWorkspaceAdaptersJsonTest => _ => _
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceAdaptersJsonTest);

    private Target CiTypescriptE2EAngularBaseTest => _ => _
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptE2EAngularBaseTest);

    private Target CiTypescriptWorkspacesE2EAngularAppsIntranetTest => _ => _
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptE2EAngularAppsIntranetTest);

    private Target CiTypescriptWorkspacesE2EAngularAppsExtranetTest => _ => _
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptE2EAngularAppsExtranetTest);

    private Target CiDemosTest => _ => _
        .DependsOn(DemosDerivationTest);
}
