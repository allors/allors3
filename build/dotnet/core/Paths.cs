using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetCore => this.Dotnet / "core";
    public AbsolutePath DotnetCoreRepositoryDomainRepository => this.DotnetCore / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetCoreDatabase => this.DotnetCore / "Database";
    public AbsolutePath DotnetCoreDatabaseMetaGenerated => this.DotnetCoreDatabase / "Meta/generated";
    public AbsolutePath DotnetCoreDatabaseGenerate => this.DotnetCoreDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetCoreDatabaseMerge => this.DotnetCoreDatabase / "Merge/Merge.csproj";
    public AbsolutePath DotnetCoreDatabaseServer => this.DotnetCoreDatabase / "Server";
    public AbsolutePath DotnetCoreDatabaseCommands => this.DotnetCoreDatabase / "Commands";
    public AbsolutePath DotnetCoreDatabaseMetaTests => this.DotnetCoreDatabase / "Meta.Tests/Meta.Tests.csproj";
    public AbsolutePath DotnetCoreDatabaseDomainTests => this.DotnetCoreDatabase / "Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath DotnetCoreDatabaseServerLocalTests => this.DotnetCoreDatabase / "Server.Local.Tests/Server.Local.Tests.csproj";

    public AbsolutePath DotnetCoreDatabaseServerRemoteTests => this.DotnetCoreDatabase / "Server.Remote.Tests/Server.Remote.Tests.csproj";

    public AbsolutePath DotnetCoreDatabaseResources => this.DotnetCoreDatabase / "Resources";
    public AbsolutePath DotnetCoreDatabaseResourcesCore => this.DotnetCoreDatabaseResources / "Core";
    public AbsolutePath DotnetCoreDatabaseResourcesCustom => this.DotnetCoreDatabaseResources / "Custom";

    public AbsolutePath DotnetCoreWorkspaceCSharpTestsLocal => this.DotnetCore / "Workspace/CSharp/Tests.Local";
    public AbsolutePath DotnetCoreWorkspaceCSharpTestsRemote => this.DotnetCore / "Workspace/CSharp/Tests.Remote";

    public AbsolutePath DotnetCoreWorkspaceCSharpAdaptersLocalTests => this.DotnetCore / "Workspace/CSharp/Adapters/Tests.Local";
    public AbsolutePath DotnetCoreWorkspaceCSharpAdaptersRemoteTests => this.DotnetCore / "Workspace/CSharp/Adapters/Tests.Remote";

    public AbsolutePath DotnetCoreWorkspaceTypescript => this.DotnetCore / "Workspace/Typescript";
}
