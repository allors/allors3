using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Artifacts => Root / "artifacts";

    public AbsolutePath ArtifactsTests => Artifacts / "Tests";

    // Configuration templates (config/<provider>/<domain>/...)
    public AbsolutePath Config => Root / "config";
    public AbsolutePath ConfigSqlClient => Config / "sqlclient";
    public AbsolutePath ConfigNpgsql => Config / "npgsql";

    // Core
    public AbsolutePath ArtifactsCoreCommands => Artifacts / "Core/Commands";
    public AbsolutePath ArtifactsCoreServer => Artifacts / "Core/Server";

    // Identity
    public AbsolutePath ArtifactsIdentityCommands => Artifacts / "Identity/Commands";
    public AbsolutePath ArtifactsIdentityServer => Artifacts / "Identity/Server";

    // Base
    public AbsolutePath ArtifactsBaseCommands => Artifacts / "Base/Commands";
    public AbsolutePath ArtifactsBaseServer => Artifacts / "Base/Server";


    // Apps
    public AbsolutePath ArtifactsAppsCommands => Artifacts / "Apps/Commands";
    public AbsolutePath ArtifactsAppsServer => Artifacts / "Apps/Server";
}