using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetEntra => Dotnet / "Entra";
    public AbsolutePath DotnetEntraRepositoryDomainRepository => DotnetEntra / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetEntraDatabase => DotnetEntra / "Database";
    public AbsolutePath DotnetEntraDatabaseMetaGenerated => DotnetEntraDatabase / "Meta/Generated";
    public AbsolutePath DotnetEntraDatabaseGenerate => DotnetEntraDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetEntraDatabaseDomainTests => DotnetEntraDatabase / "Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath DotnetEntraDatabaseResources => DotnetEntraDatabase / "Resources";
    public AbsolutePath DotnetEntraDatabaseResourcesEntra => DotnetEntraDatabaseResources / "Entra";
    public AbsolutePath DotnetEntraDatabaseResourcesTest => DotnetEntraDatabaseResources / "Test";
}
