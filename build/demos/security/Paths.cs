using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DemosSecurity => this.Demos / "Security";
    public AbsolutePath DemosSecurityRepositoryDomainRepository => this.DemosSecurity / "Repository/Domain/Repository.csproj";
    public AbsolutePath DemosSecurityDatabaseMetaGenerated => this.DemosSecurity / "Database/Meta/generated";
    public AbsolutePath DemosSecurityDatabaseGenerate => this.DemosSecurity / "Database/Generate/Generate.csproj";
    public AbsolutePath DemosSecurityDatabaseDomainTests => this.DemosSecurity / "Database/Domain.Tests/Domain.Tests.csproj";
}
