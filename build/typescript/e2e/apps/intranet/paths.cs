using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EAppsIntranet => TypescriptE2E / "apps";

    public AbsolutePath TypescriptE2EAppsIntranetGenerate => TypescriptE2EAppsIntranet / "Generate/Generate.csproj";

    public AbsolutePath TypescriptE2EAppsIntranetAngularTests => TypescriptE2EAppsIntranet / "Angular.Tests/Angular.Tests.csproj";
}
