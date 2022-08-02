using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptModules => Typescript / "modules";

    public AbsolutePath TypescriptModulesApps => TypescriptModules / "apps";

    public AbsolutePath TypescriptModulesAppsBaseAngularMaterial => TypescriptModulesApps / "base/workspace/angular-material";


    public AbsolutePath TypescriptModulesLibs => TypescriptModules / "libs";

    public AbsolutePath TypescriptModulesAppsAppsIntranetAngularMaterial => TypescriptModulesLibs / "apps-intranet/workspace/angular-material";
}
