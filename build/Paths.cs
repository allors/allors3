using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Artifacts => Root / "artifacts";

    public AbsolutePath ArtifactsTests => Artifacts / "Tests";

    // Core
    public AbsolutePath ArtifactsCoreCommands => Artifacts / "Core/Commands";
    public AbsolutePath ArtifactsCoreServer => Artifacts / "Core/Server";

    // Base
    public AbsolutePath ArtifactsBaseCommands => Artifacts / "Base/Commands";
    public AbsolutePath ArtifactsBaseServer => Artifacts / "Base/Server";


    // Apps
    public AbsolutePath ArtifactsAppsCommands => Artifacts / "Apps/Commands";
    public AbsolutePath ArtifactsAppsServer => Artifacts / "Apps/Server";
}