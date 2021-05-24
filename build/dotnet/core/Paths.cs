using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetCore => Dotnet / "core";
    public AbsolutePath DotnetCoreRepositoryDomainRepository => DotnetCore / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetCoreDatabase => DotnetCore / "Database";
    public AbsolutePath DotnetCoreDatabaseMetaGenerated => DotnetCoreDatabase / "Meta/generated";
    public AbsolutePath DotnetCoreDatabaseGenerate => DotnetCoreDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetCoreDatabaseMerge => DotnetCoreDatabase / "Merge/Merge.csproj";
    public AbsolutePath DotnetCoreDatabaseServer => DotnetCoreDatabase / "Server";
    public AbsolutePath DotnetCoreDatabaseCommands => DotnetCoreDatabase / "Commands";
    public AbsolutePath DotnetCoreDatabaseMetaTests => DotnetCoreDatabase / "Meta.Tests/Meta.Tests.csproj";
    public AbsolutePath DotnetCoreDatabaseDomainTests => DotnetCoreDatabase / "Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath DotnetCoreDatabaseServerLocalTests => DotnetCoreDatabase / "Server.Local.Tests/Server.Local.Tests.csproj";

    public AbsolutePath DotnetCoreDatabaseServerRemoteTests => DotnetCoreDatabase / "Server.Remote.Tests/Server.Remote.Tests.csproj";

    public AbsolutePath DotnetCoreDatabaseResources => DotnetCoreDatabase / "Resources";
    public AbsolutePath DotnetCoreDatabaseResourcesCore => DotnetCoreDatabaseResources / "Core";
    public AbsolutePath DotnetCoreDatabaseResourcesCustom => DotnetCoreDatabaseResources / "Custom";

    public AbsolutePath DotnetCoreWorkspaceCSharpTestsLocal => DotnetCore / "Workspace/CSharp/Tests.Local";
    public AbsolutePath DotnetCoreWorkspaceCSharpTestsRemote => DotnetCore / "Workspace/CSharp/Tests.Remote";

    public AbsolutePath DotnetCoreWorkspaceCSharpAdaptersLocalTests => DotnetCore / "Workspace/CSharp/Adapters/Tests.Local";
    public AbsolutePath DotnetCoreWorkspaceCSharpAdaptersRemoteTests => DotnetCore / "Workspace/CSharp/Adapters/Tests.Remote";

    public AbsolutePath DotnetCoreWorkspaceTypescript => DotnetCore / "Workspace/Typescript";
}
