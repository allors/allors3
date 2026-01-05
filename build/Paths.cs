using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Artifacts => Root / "artifacts";

    public AbsolutePath ArtifactsTests => Artifacts / "Tests";

    // Core
    public AbsolutePath ArtifactsCoreServer => Artifacts / "Core/Server";

    // Base
    public AbsolutePath ArtifactsBaseServer => Artifacts / "Base/Server";

    // Apps
    public AbsolutePath ArtifactsAppsServer => Artifacts / "Apps/Server";
}