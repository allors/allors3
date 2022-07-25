using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EAppsExtranet => TypescriptE2E / "AppsExtranet";

    public AbsolutePath TypescriptE2EAppsExtranetAngularTests => TypescriptE2EAppsExtranet / "Angular.Tests/Angular.Tests.csproj";
}
