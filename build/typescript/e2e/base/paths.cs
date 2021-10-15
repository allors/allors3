using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptE2EBase => TypescriptE2E / "base";

    public AbsolutePath TypescriptE2EBaseGenerate => TypescriptE2EBase / "Generate/Generate.csproj";

    public AbsolutePath TypescriptE2EBaseAngularTests => TypescriptE2EBase / "Angular.Tests/Angular.Tests.csproj";
}
