using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DemosDerivation => Demos / "Derivation";
    public AbsolutePath DemosDerivationRepositoryDomainRepository => DemosDerivation / "Repository/Domain/Repository.csproj";
    public AbsolutePath DemosDerivationDatabaseMetaGenerated => DemosDerivation / "Database/Meta/Generated";
    public AbsolutePath DemosDerivationDatabaseGenerate => DemosDerivation / "Database/Generate/Generate.csproj";
    public AbsolutePath DemosDerivationDatabaseDomainTests => DemosDerivation / "Database/Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DemosDerivationDatabaseResources => DemosDerivation / "Database/Resources";
}
