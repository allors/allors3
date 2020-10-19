using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath Base => this.Root / "base";
    public AbsolutePath BaseRepositoryDomainRepository => this.Base / "Repository/Domain/Repository.csproj";

    public AbsolutePath BaseDatabase => this.Base / "Database";
    public AbsolutePath BaseDatabaseMetaGenerated => this.BaseDatabase / "Meta/generated";
    public AbsolutePath BaseDatabaseGenerate => this.BaseDatabase / "Generate/Generate.csproj";
    public AbsolutePath BaseDatabaseMerge => this.BaseDatabase / "Merge/Merge.csproj";
    public AbsolutePath BaseDatabaseCommands => this.BaseDatabase / "Commands";
    public AbsolutePath BaseDatabaseServer => this.BaseDatabase / "Server";
    public AbsolutePath BaseDatabaseDomainTests => this.BaseDatabase / "Domain.Tests/Domain.Tests.csproj";
    public AbsolutePath BaseDatabaseResources => this.BaseDatabase / "Resources";
    public AbsolutePath BaseDatabaseResourcesBase => this.BaseDatabaseResources / "Base";

    public AbsolutePath BaseWorkspaceTypescript => this.Base / "Workspace/Typescript";
}
