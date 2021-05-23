using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DemosDerivation => this.Demos / "Derivation";
    public AbsolutePath DemosDerivationRepositoryDomainRepository => this.DemosDerivation / "Repository/Domain/Repository.csproj";
    public AbsolutePath DemosDerivationDatabaseMetaGenerated => this.DemosDerivation / "Database/Meta/generated";
    public AbsolutePath DemosDerivationDatabaseGenerate => this.DemosDerivation / "Database/Generate/Generate.csproj";
    public AbsolutePath DemosDerivationDatabaseDomainTests => this.DemosDerivation / "Database/Domain.Tests/Domain.Tests.csproj";
}
