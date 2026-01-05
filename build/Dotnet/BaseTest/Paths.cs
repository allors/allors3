using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetBaseTest => Dotnet / "BaseTest";
    public AbsolutePath DotnetBaseTestRepositoryDomainRepository => DotnetBaseTest / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetBaseTestDatabase => DotnetBaseTest / "Database";
    public AbsolutePath DotnetBaseTestDatabaseMetaGenerated => DotnetBaseTestDatabase / "Meta/Generated";
    public AbsolutePath DotnetBaseTestDatabaseGenerate => DotnetBaseTestDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetBaseTestDatabaseServer => DotnetBaseTestDatabase / "Server";
    public AbsolutePath DotnetBaseTestDatabaseDomainTests => DotnetBaseTestDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetBaseTestDatabaseResources => DotnetBaseTestDatabase / "Resources";
    public AbsolutePath DotnetBaseTestDatabaseResourcesBase => DotnetBaseTestDatabaseResources / "Base";

    public AbsolutePath DotnetBaseTestWorkspaceTypescript => DotnetBaseTest / "Workspace/Typescript";
}
