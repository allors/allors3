using Nuke.Common;

partial class Build
{
    private Target CiDotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(this.DotnetSystemAdaptersTestMemory);

    private Target CiDotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(this.DotnetSystemAdaptersTestSqlClient);

    private Target CiDotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(this.DotnetSystemAdaptersTestNpgsql);

    private Target CiDotnetSystemWorkspaceTest => _ => _
        .DependsOn(this.DotnetSystemInstall)
        .DependsOn(this.DotnetSystemWorkspaceTest);

    private Target CiDotnetCoreDatabaseTest => _ => _
        .DependsOn(this.DotnetCoreDatabaseTest);

    private Target CiDotnetCoreWorkspaceTest => _ => _
        .DependsOn(this.DotnetCoreInstall);
    //.DependsOn(this.CoreWorkspaceTest);

    private Target CiDotnetBaseDatabaseTest => _ => _
        .DependsOn(this.DotnetBaseDatabaseTest);

    private Target CiDotnetBaseWorkspaceTest => _ => _
        .DependsOn(this.DotnetBaseInstall)
        .DependsOn(this.DotnetBaseWorkspaceTest);

    private Target CiDotnetAppsDatabaseTest => _ => _
        .DependsOn(this.DotnetAppsDatabaseTest);

    private Target CiDotnetAppsWorkspaceTest => _ => _
        .DependsOn(this.DotnetAppsInstall)
        .DependsOn(this.DotnetAppsWorkspaceTest);

    private Target CiDemosTest => _ => _
        .DependsOn(this.DemosDerivationTest)
        .DependsOn(this.DemosSecurityTest);
}
