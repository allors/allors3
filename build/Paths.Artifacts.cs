using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Artifacts => this.Root / "artifacts";

    public AbsolutePath ArtifactsTests => this.Artifacts / "Tests";

    // Core
    public AbsolutePath ArtifactsCoreCommands => this.Artifacts / "Core/Commands";
    public AbsolutePath ArtifactsCoreServer => this.Artifacts / "Core/Server";

    // Apps
    public AbsolutePath ArtifactsTestsAppsWorkspaceTypescriptDomain => this.ArtifactsTests / "AppsWorkspaceTypescriptDomain.trx";
    public AbsolutePath ArtifactsAppsCommands => this.Artifacts / "Apps/Commands";
    public AbsolutePath ArtifactsAppsServer => this.Artifacts / "Apps/Server";
    public AbsolutePath ArtifactsAppsExcellAddIn => this.Artifacts / "Apps/ExcelAddIn";
}
