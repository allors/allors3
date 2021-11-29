using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EAppsIntranet => TypescriptE2E / "apps-intranet";

    public AbsolutePath TypescriptE2EAppsIntranetAngularTests => TypescriptE2EAppsIntranet / "Angular.Tests/Angular.Tests.csproj";
}
