using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EAppsIntranet => TypescriptE2E / "AppsIntranet";

    public AbsolutePath TypescriptE2EAppsIntranetE2E => TypescriptE2EAppsIntranet / "E2E";
    public AbsolutePath TypescriptE2EAppsIntranetE2EGenerated => TypescriptE2EAppsIntranetE2E / "Generated";

    public AbsolutePath TypescriptE2EAppsIntranetScaffold => TypescriptE2EAppsIntranet / "Scaffold.Command";
    public AbsolutePath TypescriptE2EAppsIntranetScaffoldProject => TypescriptE2EAppsIntranetScaffold / "Scaffold.Command.csproj";

    public AbsolutePath TypescriptE2EAppsIntranetTests => TypescriptE2EAppsIntranet / "Tests";
    public AbsolutePath TypescriptE2EAppsIntranetTestsProject => TypescriptE2EAppsIntranetTests / "Tests.csproj";
    public AbsolutePath TypescriptE2EAppsIntranetTestsPlaywrightCommand => TypescriptE2EAppsIntranetTests / "bin/debug/net6.0/.playwright/node/win32_x64/playwright.cmd";

}