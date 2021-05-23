using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetSystem => this.Dotnet / "System";
    public AbsolutePath DotnetSystemRepositoryTemplates => this.DotnetSystem / "Repository/Templates";
    public AbsolutePath DotnetSystemRepositoryTemplatesMetaCs => this.DotnetSystemRepositoryTemplates / "meta.cs.stg";
    public AbsolutePath DotnetSystemRepositoryGenerate => this.DotnetSystem / "Repository/Generate/Generate.csproj";

    public AbsolutePath DotnetSystemDatabase => this.DotnetSystem / "Database";

    public AbsolutePath DotnetSystemAdapters => this.DotnetSystemDatabase / "Adapters";

    public AbsolutePath DotnetSystemAdaptersRepositoryDomainRepository => this.DotnetSystemAdapters / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetSystemAdaptersMetaGenerated => this.DotnetSystemAdapters / "Meta/generated";
    public AbsolutePath DotnetSystemAdaptersGenerate => this.DotnetSystemAdapters / "Generate/Generate.csproj";
    public AbsolutePath DotnetSystemAdaptersStaticTests => this.DotnetSystemAdapters / "Tests.Static/Tests.Static.csproj";

    public AbsolutePath DotnetSystemWorkspaceTypescript => this.DotnetSystem / "Workspace/Typescript";
}
