using Nuke.Common;

partial class Build
{
    private Target CiDotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(DotnetSystemAdaptersTestMemory);

    private Target CiDotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(DotnetSystemAdaptersTestSqlClient);

    private Target CiDotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(DotnetSystemAdaptersTestNpgsql);

    private Target CiDotnetSystemWorkspaceTest => _ => _
        .DependsOn(DotnetSystemInstall)
        .DependsOn(DotnetSystemWorkspaceTest);

    private Target CiDotnetCoreDatabaseTest => _ => _
        .DependsOn(DotnetCoreDatabaseTest);

    private Target CiDotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(DotnetCoreInstall)
        .DependsOn(DotnetCoreWorkspaceLocalTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(DotnetCoreInstall)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonSystemTextTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonRestSharpTest => _ => _
        .DependsOn(DotnetCoreInstall)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonRestSharpTest);

    private Target CiDotnetLegacyDatabaseTest => _ => _
        .DependsOn(DotnetLegacyDatabaseTest);
    
    private Target CiDotnetBaseDatabaseTest => _ => _
        .DependsOn(DotnetBaseDatabaseTest);

    private Target CiDotnetBaseWorkspaceTest => _ => _
        .DependsOn(DotnetBaseInstall)
        .DependsOn(DotnetBaseWorkspaceTest);

    private Target CiDotnetAppsDatabaseTest => _ => _
        .DependsOn(DotnetAppsDatabaseTest);

    private Target CiDotnetAppsWorkspaceTest => _ => _
        .DependsOn(DotnetAppsInstall)
        .DependsOn(DotnetAppsWorkspaceTest);

    private Target CiTypescriptTest => _ => _
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptTest);

    private Target CiDemosTest => _ => _
        .DependsOn(DemosDerivationTest)
        .DependsOn(DemosSecurityTest);
}
