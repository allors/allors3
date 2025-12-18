using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Typescript => Root / "typescript";

    public AbsolutePath TypescriptApps => Typescript / "apps";

    public AbsolutePath TypescriptBaseAngularMaterial => TypescriptApps / "base/workspace/angular-material";

    public AbsolutePath TypescriptLibs => Typescript / "libs";

    public AbsolutePath TypescriptAppsAppsIntranetAngularMaterial => TypescriptLibs / "apps-intranet/workspace/angular-material";
}
