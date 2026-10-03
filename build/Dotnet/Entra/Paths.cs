using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetEntra => Dotnet / "Entra";
    public AbsolutePath DotnetEntraRepositoryDomainRepository => DotnetEntra / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetEntraDatabase => DotnetEntra / "Database";
    public AbsolutePath DotnetEntraDatabaseMetaGenerated => DotnetEntraDatabase / "Meta/Generated";
    public AbsolutePath DotnetEntraDatabaseGenerate => DotnetEntraDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetEntraDatabaseServer => DotnetEntraDatabase / "Server";
    public AbsolutePath DotnetEntraDatabaseCommands => DotnetEntraDatabase / "Commands";
    public AbsolutePath DotnetEntraDatabaseDomainTests => DotnetEntraDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetEntraDatabaseServerLocalTests => DotnetEntraDatabase / "Server.Local.Tests/Server.Local.Tests.csproj";
    public AbsolutePath DotnetEntraDatabaseServerRemoteTests => DotnetEntraDatabase / "Server.Remote.Tests/Server.Remote.Tests.csproj";

    public AbsolutePath DotnetEntraDatabaseResources => DotnetEntraDatabase / "Resources";
    public AbsolutePath DotnetEntraDatabaseResourcesEntra => DotnetEntraDatabaseResources / "Entra";
    public AbsolutePath DotnetEntraDatabaseResourcesTest => DotnetEntraDatabaseResources / "Test";
}
