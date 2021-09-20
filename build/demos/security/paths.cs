using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DemosSecurity => Demos / "Security";
    public AbsolutePath DemosSecurityRepositoryDomainRepository => DemosSecurity / "Repository/Domain/Repository.csproj";
    public AbsolutePath DemosSecurityDatabaseMetaGenerated => DemosSecurity / "Database/Meta/generated";
    public AbsolutePath DemosSecurityDatabaseGenerate => DemosSecurity / "Database/Generate/Generate.csproj";
    public AbsolutePath DemosSecurityDatabaseDomainTests => DemosSecurity / "Database/Domain.Tests/Domain.Tests.csproj";
}
