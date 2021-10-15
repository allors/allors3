using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EApps => TypescriptE2E / "apps";

    public AbsolutePath TypescriptE2EAppsGenerate => TypescriptE2EApps / "Generate/Generate.csproj";

    public AbsolutePath TypescriptE2EAppsAngularTests => TypescriptE2EApps / "Angular.Tests/Angular.Tests.csproj";
}
