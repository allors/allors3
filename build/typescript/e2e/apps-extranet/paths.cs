using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EAppsExtranet => TypescriptE2E / "apps-intranet";

    public AbsolutePath TypescriptE2EAppsExtranetGenerate => TypescriptE2EAppsExtranet / "Generate/Generate.csproj";

    public AbsolutePath TypescriptE2EAppsExtranetAngularTests => TypescriptE2EAppsExtranet / "Angular.Tests/Angular.Tests.csproj";
}
