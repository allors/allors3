using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptModules => Typescript / "modules";

    public AbsolutePath TypescriptModulesApps => TypescriptModules / "apps";

    public AbsolutePath TypescriptModulesLibs => TypescriptModules / "libs";
}
