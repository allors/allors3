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

    private Target CiDotnetCoreDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreDatabaseTest);

    private Target CiDotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceLocalTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonSystemTextTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonRestSharpTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonRestSharpTest);

    private Target CiDotnetLegacyDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetLegacyDatabaseTest);

    private Target CiDotnetBaseDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetBaseDatabaseTest);

    private Target CiDotnetBaseWorkspaceTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetBaseWorkspaceTest);

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

    private Target CiTypescriptE2EAngularBaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptE2EAngularBaseTest);

    private Target CiTypescriptWorkspacesE2EAngularAppsIntranetTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptE2EAngularAppsIntranetTest);

    private Target CiTypescriptWorkspacesE2EAngularAppsExtranetTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptE2EAngularAppsExtranetTest);

    private Target CiDemosTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DemosDerivationTest);
}
