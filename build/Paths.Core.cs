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
    public AbsolutePath CoreDatabaseDomainTests => this.CoreDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath CoreDatabaseApiTests => this.CoreDatabase / "Api.Tests/Api.Tests.csproj";
    public AbsolutePath CoreDatabaseServerTests => this.CoreDatabase / "Server.Tests/Server.Tests.csproj";
    public AbsolutePath CoreDatabaseResources => this.CoreDatabase / "Resources";
    public AbsolutePath CoreDatabaseResourcesCore => this.CoreDatabaseResources / "Core";
    public AbsolutePath CoreDatabaseResourcesCustom => this.CoreDatabaseResources / "Custom";

    public AbsolutePath CoreWorkspaceCSharpTests => this.Core / "Workspace/CSharp/Tests";

    public AbsolutePath CoreWorkspaceTypescript=> this.Core / "Workspace/Typescript";
}
