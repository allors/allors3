using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Security => this.Root / "Demos/Security";
    public AbsolutePath SecurityRepositoryDomainRepository => this.Security / "Repository/Domain/Repository.csproj";
    public AbsolutePath SecurityDatabaseMetaGenerated => this.Security / "Database/Meta/generated";
    public AbsolutePath SecurityDatabaseGenerate => this.Security / "Database/Generate/Generate.csproj";
    public AbsolutePath SecurityDatabaseDomainTests => this.Security / "Database/Domain.Tests/Domain.Tests.csproj";
}
