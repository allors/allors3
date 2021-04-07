using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Core => this.Root / "core";
    public AbsolutePath CoreRepositoryDomainRepository => this.Core / "Repository/Domain/Repository.csproj";

    public AbsolutePath CoreDatabase => this.Core / "Database";
    public AbsolutePath CoreDatabaseMetaGenerated => this.CoreDatabase / "Meta/generated";
    public AbsolutePath CoreDatabaseGenerate => this.CoreDatabase / "Generate/Generate.csproj";
    public AbsolutePath CoreDatabaseMerge => this.CoreDatabase / "Merge/Merge.csproj";
    public AbsolutePath CoreDatabaseServer => this.CoreDatabase / "Server";
    public AbsolutePath CoreDatabaseCommands => this.CoreDatabase / "Commands";
    public AbsolutePath CoreDatabaseMetaTests => this.CoreDatabase / "Meta.Tests/Meta.Tests.csproj";
    public AbsolutePath CoreDatabaseDomainTests => this.CoreDatabase / "Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath CoreDatabaseServerLocalTests =>
        this.CoreDatabase / "Server.Local.Tests/Server.Local.Tests.csproj";

    public AbsolutePath CoreDatabaseServerRemoteTests =>
        this.CoreDatabase / "Server.Remote.Tests/Server.Remote.Tests.csproj";

    public AbsolutePath CoreDatabaseResources => this.CoreDatabase / "Resources";
    public AbsolutePath CoreDatabaseResourcesCore => this.CoreDatabaseResources / "Core";
    public AbsolutePath CoreDatabaseResourcesCustom => this.CoreDatabaseResources / "Custom";

    public AbsolutePath CoreWorkspaceCSharpTestsLocal => this.Core / "Workspace/CSharp/Tests.Local";
    public AbsolutePath CoreWorkspaceCSharpTestsRemote => this.Core / "Workspace/CSharp/Tests.Remote";

    public AbsolutePath CoreWorkspaceCSharpAdaptersLocalTests => this.Core / "Workspace/CSharp/Adapters/Tests.Local";
    public AbsolutePath CoreWorkspaceCSharpAdaptersRemoteTests => this.Core / "Workspace/CSharp/Adapters/Tests.Remote";

    public AbsolutePath CoreWorkspaceTypescript => this.Core / "Workspace/Typescript";
}
