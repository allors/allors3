using Nuke.Common;

partial class Build
{
    private Target CiDotnetSystemSharedTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetSystemSharedTest);

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

    private Target CiDotnetCoreWorkspaceConnectionTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceConnectionTest);

    private Target CiDotnetCoreWorkspaceLocalTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceLocalTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonSystemTextTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonSystemTextTest);

    private Target CiDotnetCoreWorkspaceRemoteJsonNewtonsoftTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetCoreWorkspaceRemoteJsonNewtonsoftTest);

    private Target CiDotnetIdentityDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetIdentityDatabaseTest);

    private Target CiDotnetEntraDatabaseTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetEntraDatabaseTest);

    private Target CiDotnetDiamondTest => _ => _
        .DependsOn(Reset)
        .DependsOn(DotnetDiamondTest);

    private Target CiTypescriptWorkspaceTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceTest);

    private Target CiTypescriptWorkspaceAdaptersJsonTest => _ => _
        .DependsOn(Reset)
        .DependsOn(TypescriptInstall)
        .DependsOn(TypescriptWorkspaceAdaptersJsonTest);
}
