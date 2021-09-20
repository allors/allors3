using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath TypescriptModules => Typescript / "modules";

    public AbsolutePath TypescriptModulesTemplates => TypescriptModules / "templates";
}
