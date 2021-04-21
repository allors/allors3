using Nuke.Common;

partial class Build
{
    private Target CiAdaptersTestMemory => _ => _
        .DependsOn(this.AdaptersTestMemory);

    private Target CiAdaptersTestSqlClient => _ => _
        .DependsOn(this.AdaptersTestSqlClient);

    private Target CiAdaptersTestNpgsql => _ => _
        .DependsOn(this.AdaptersTestNpgsql);

    private Target CiSystemWorkspaceTest => _ => _
        .DependsOn(this.SystemInstall)
        .DependsOn(this.SystemWorkspaceTest);
    
    private Target CiCoreDatabaseTest => _ => _
        .DependsOn(this.CoreDatabaseTest);

    private Target CiCoreWorkspaceTest => _ => _
        .DependsOn(this.CoreInstall);
    //.DependsOn(this.CoreWorkspaceTest);

    private Target CiBaseDatabaseTest => _ => _
        .DependsOn(this.BaseDatabaseTest);

    private Target CiBaseWorkspaceTest => _ => _
        .DependsOn(this.BaseInstall)
        .DependsOn(this.BaseWorkspaceTest);

    private Target CiAppsDatabaseTest => _ => _
        .DependsOn(this.AppsDatabaseTest);

    private Target CiAppsWorkspaceTest => _ => _
        .DependsOn(this.AppsInstall)
        .DependsOn(this.AppsWorkspaceTest);

    private Target CiDemosTest => _ => _
        .DependsOn(this.DerivationTest)
        .DependsOn(this.SecurityTest);
}
