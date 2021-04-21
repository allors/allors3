using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath System => this.Root / "System";
    public AbsolutePath SystemRepositoryTemplates => this.System / "Repository/Templates";
    public AbsolutePath SystemRepositoryTemplatesMetaCs => this.SystemRepositoryTemplates / "meta.cs.stg";
    public AbsolutePath SystemRepositoryGenerate => this.System / "Repository/Generate/Generate.csproj";

    public AbsolutePath SystemDatabase => this.System / "Database";

    public AbsolutePath SystemAdapters => this.SystemDatabase / "Adapters";

    public AbsolutePath SystemAdaptersRepositoryDomainRepository =>
        this.SystemAdapters / "Repository/Domain/Repository.csproj";

    public AbsolutePath SystemAdaptersMetaGenerated => this.SystemAdapters / "Meta/generated";
    public AbsolutePath SystemAdaptersGenerate => this.SystemAdapters / "Generate/Generate.csproj";
    public AbsolutePath SystemAdaptersStaticTests => this.SystemAdapters / "Tests.Static/Tests.Static.csproj";

    public AbsolutePath SystemWorkspaceTypescript => this.System / "Workspace/Typescript";
}
