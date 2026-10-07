using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetDiamond => Dotnet / "Diamond";
    public AbsolutePath DotnetDiamondRepositoryDomainRepository => DotnetDiamond / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetDiamondDatabase => DotnetDiamond / "Database";
    public AbsolutePath DotnetDiamondDatabaseMetaGenerated => DotnetDiamondDatabase / "Meta/Generated";
    public AbsolutePath DotnetDiamondDatabaseGenerate => DotnetDiamondDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetDiamondDatabaseDomainTests => DotnetDiamondDatabase / "Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath DotnetDiamondDatabaseResources => DotnetDiamondDatabase / "Resources";
    public AbsolutePath DotnetDiamondDatabaseResourcesLevel1 => DotnetDiamondDatabaseResources / "Level1";
    public AbsolutePath DotnetDiamondDatabaseResourcesLevel2 => DotnetDiamondDatabaseResources / "Level2";
    public AbsolutePath DotnetDiamondDatabaseResourcesPlugin1 => DotnetDiamondDatabaseResources / "Plugin1";
    public AbsolutePath DotnetDiamondDatabaseResourcesTest => DotnetDiamondDatabaseResources / "Test";
}
