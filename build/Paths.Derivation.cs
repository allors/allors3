using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Derivation => this.Root / "Demos/Derivation";
    public AbsolutePath DerivationRepositoryDomainRepository => this.Derivation / "Repository/Domain/Repository.csproj";
    public AbsolutePath DerivationDatabaseMetaGenerated => this.Derivation / "Database/Meta/generated";
    public AbsolutePath DerivationDatabaseGenerate => this.Derivation / "Database/Generate/Generate.csproj";
    public AbsolutePath DerivationDatabaseDomainTests => this.Derivation / "Database/Domain.Tests/Domain.Tests.csproj";
}
