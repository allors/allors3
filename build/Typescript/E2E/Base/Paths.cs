using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EBase => TypescriptE2E / "base";

    public AbsolutePath TypescriptE2EBaseE2E => TypescriptE2EBase / "E2E";
    public AbsolutePath TypescriptE2EBaseE2EGenerated => TypescriptE2EBaseE2E / "Generated";
    
    public AbsolutePath TypescriptE2EBaseScaffold => TypescriptE2EBase / "Scaffold.Command";
    public AbsolutePath TypescriptE2EBaseScaffoldProject => TypescriptE2EBaseScaffold / "Scaffold.Command.csproj";
    
    public AbsolutePath TypescriptE2EBaseTests => TypescriptE2EBase / "Tests";
    public AbsolutePath TypescriptE2EBaseTestsProject => TypescriptE2EBaseTests / "Tests.csproj";
    public AbsolutePath TypescriptE2EBaseTestsPlaywrightCommand => TypescriptE2EBaseTests / "bin/debug/net6.0/.playwright/node/win32_x64/playwright.cmd";

}
