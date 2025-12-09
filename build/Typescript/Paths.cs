using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Typescript => Root / "typescript";

    public AbsolutePath TypescriptApps => Typescript / "apps";

    public AbsolutePath TypescriptAppsBaseAngularMaterial => TypescriptApps / "base/workspace/angular-material";


    public AbsolutePath TypescriptLibs => Typescript / "libs";
}
