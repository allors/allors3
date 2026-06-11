using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetIdentity => Dotnet / "Identity";
    public AbsolutePath DotnetIdentityRepositoryDomainRepository => DotnetIdentity / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetIdentityDatabase => DotnetIdentity / "Database";
    public AbsolutePath DotnetIdentityDatabaseMetaGenerated => DotnetIdentityDatabase / "Meta/Generated";
    public AbsolutePath DotnetIdentityDatabaseGenerate => DotnetIdentityDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetIdentityDatabaseCommands => DotnetIdentityDatabase / "Commands";
    public AbsolutePath DotnetIdentityDatabaseServer => DotnetIdentityDatabase / "Server";
    public AbsolutePath DotnetIdentityDatabaseDomainTests => DotnetIdentityDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath DotnetIdentityDatabaseServerRemoteTests => DotnetIdentityDatabase / "Server.Remote.Tests/Server.Remote.Tests.csproj";
    public AbsolutePath DotnetIdentityDatabaseResources => DotnetIdentityDatabase / "Resources";
    public AbsolutePath DotnetIdentityDatabaseResourcesIdentity => DotnetIdentityDatabaseResources / "Identity";
}
