using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Security => Root / "Demos/Security";
    public AbsolutePath SecurityRepositoryDomainRepository => Security / "Repository/Domain/Repository.csproj";
    public AbsolutePath SecurityDatabaseMetaGenerated => Security / "Database/Meta/generated";
    public AbsolutePath SecurityDatabaseGenerate => Security / "Database/Generate/Generate.csproj";
    public AbsolutePath SecurityDatabaseDomainTests => Security / "Database/Domain.Tests/Domain.Tests.csproj";
}

