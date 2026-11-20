using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Sample => Root / "sample";
    public AbsolutePath SampleRepositoryDomainRepository => Sample / "Repository/Domain/Repository.csproj";

    public AbsolutePath SampleDatabase => Sample / "Database";
    public AbsolutePath SampleDatabaseMetaGenerated => SampleDatabase / "Meta/Generated";
    public AbsolutePath SampleDatabaseResourcesGenerated => SampleDatabase / "Resources/Generated";
    public AbsolutePath SampleDatabaseGenerate => SampleDatabase / "Generate/Generate.csproj";
    public AbsolutePath SampleDatabaseDomainTests => SampleDatabase / "Domain.Tests/Domain.Tests.csproj";
}
