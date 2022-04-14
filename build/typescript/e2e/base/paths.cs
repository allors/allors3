using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EBase => TypescriptE2E / "base";
    
    public AbsolutePath TypescriptE2EBaseAngularTests => TypescriptE2EBase / "Tests/Tests.csproj";
    public AbsolutePath TypescriptE2EBaseAngularPlaywright => TypescriptE2EBase / "Tests/bin/debug/net6.0/.playwright/node/win32_x64/playwright.cmd";

}
