using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetLegacy => Dotnet / "Legacy";
    public AbsolutePath DotnetLegacyRepositoryDomainRepository => DotnetLegacy / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetLegacyDatabase => DotnetLegacy / "Database";
    public AbsolutePath DotnetLegacyDatabaseMetaGenerated => DotnetLegacyDatabase / "Meta/Generated";
    public AbsolutePath DotnetLegacyDatabaseGenerate => DotnetLegacyDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetLegacyDatabaseCommands => DotnetLegacyDatabase / "Commands";
    public AbsolutePath DotnetLegacyDatabaseServer => DotnetLegacyDatabase / "Server";
    public AbsolutePath DotnetLegacyDatabaseDomainTests => DotnetLegacyDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetLegacyDatabaseResources => DotnetLegacyDatabase / "Resources";
    public AbsolutePath DotnetLegacyDatabaseResourcesLegacy => DotnetLegacyDatabaseResources / "Legacy";

    public AbsolutePath DotnetLegacyWorkspaceTypescript => DotnetLegacy / "Workspace/Typescript";
}
