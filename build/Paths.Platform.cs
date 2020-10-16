using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Platform => this.Root / "platform";
    public AbsolutePath PlatformRepositoryTemplates => this.Platform / "Repository/Templates";
    public AbsolutePath PlatformRepositoryTemplatesMetaCs => this.PlatformRepositoryTemplates / "meta.cs.stg";
    public AbsolutePath PlatformRepositoryGenerate => this.Platform / "Repository/Generate/Generate.csproj";

    public AbsolutePath PlatformDatabase => this.Platform / "Database";

    public AbsolutePath PlatformAdapters => this.PlatformDatabase / "Adapters";
    public AbsolutePath PlatformAdaptersRepositoryDomainRepository => this.PlatformAdapters / "Repository/Domain/Repository.csproj";
    public AbsolutePath PlatformAdaptersMetaGenerated => this.PlatformAdapters / "Meta/generated";
    public AbsolutePath PlatformAdaptersGenerate => this.PlatformAdapters / "Generate/Generate.csproj";
    public AbsolutePath PlatformAdaptersStaticTests => this.PlatformAdapters / "Tests.Static/Tests.Static.csproj";
}
