using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetCore => Dotnet / "Core";
    
    public AbsolutePath DotnetCoreDatabase => DotnetCore / "Database";
    public AbsolutePath DotnetCoreDatabaseMerge => DotnetCoreDatabase / "Merge/Merge.csproj";
    public AbsolutePath DotnetCoreDatabaseResources => DotnetCoreDatabase / "Resources";
    public AbsolutePath DotnetCoreDatabaseResourcesCore => DotnetCoreDatabaseResources / "Core";

    
    public AbsolutePath DotnetCoreTest => Dotnet / "CoreTest";
    public AbsolutePath DotnetCoreTestRepositoryDomainRepository => DotnetCoreTest / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetCoreTestDatabase => DotnetCoreTest / "Database";
    public AbsolutePath DotnetCoreTestDatabaseMetaGenerated => DotnetCoreTestDatabase / "Meta/Generated";
    public AbsolutePath DotnetCoreTestDatabaseGenerate => DotnetCoreTestDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetCoreTestDatabaseServer => DotnetCoreTestDatabase / "Server";
    public AbsolutePath DotnetCoreTestDatabaseMetaTests => DotnetCoreTestDatabase / "Meta.Tests/Meta.Tests.csproj";
    public AbsolutePath DotnetCoreTestDatabaseDomainTests => DotnetCoreTestDatabase / "Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath DotnetCoreTestDatabaseServerLocalTests => DotnetCoreTestDatabase / "Server.Local.Tests/Server.Local.Tests.csproj";

    public AbsolutePath DotnetTestCoreDatabaseServerRemoteTests => DotnetCoreTestDatabase / "Server.Remote.Tests/Server.Remote.Tests.csproj";

    public AbsolutePath DotnetCoreTestDatabaseResources => DotnetCoreTestDatabase / "Resources";
    public AbsolutePath DotnetTestCoreDatabaseResourcesCustom => DotnetCoreTestDatabaseResources / "Custom";

    public AbsolutePath DotnetTestCoreWorkspace => DotnetCoreTest / "Workspace";
    public AbsolutePath DotnetTestCoreWorkspaceTestsLocal => DotnetTestCoreWorkspace / "Tests.Local";
    public AbsolutePath DotnetCoreTestWorkspaceTestsRemoteJsonSystemText => DotnetTestCoreWorkspace / "Tests.Remote.Json.SystemText";
    public AbsolutePath DotnetCoreTestWorkspaceTestsRemoteNewtonsoftSharp => DotnetTestCoreWorkspace / "Tests.Remote.Json.Newtonsoft";
}
