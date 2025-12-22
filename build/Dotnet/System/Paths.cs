using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetSystem => Dotnet / "System";
    public AbsolutePath DotnetSystemRepositoryTemplates => DotnetSystem / "Repository/Templates";
    public AbsolutePath DotnetSystemRepositoryTemplatesMetaCs => DotnetSystemRepositoryTemplates / "meta.cs.stg";
    public AbsolutePath DotnetSystemRepositoryGenerate => DotnetSystem / "Repository/Generate/Generate.csproj";

    public AbsolutePath DotnetSystemDatabase => DotnetSystem / "Database";

    public AbsolutePath DotnetSystemAdapters => DotnetSystemDatabase / "Adapters";

    public AbsolutePath DotnetSystemAdaptersRepositoryDomainRepository => DotnetSystemAdapters / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetSystemAdaptersMetaGenerated => DotnetSystemAdapters / "Meta/Generated";
    public AbsolutePath DotnetSystemAdaptersGenerate => DotnetSystemAdapters / "Generate/Generate.csproj";
    public AbsolutePath DotnetSystemAdaptersMemoryTests => DotnetSystemAdapters / "Tests.Memory/Tests.Memory.csproj";
    public AbsolutePath DotnetSystemAdaptersNpgsqlTests => DotnetSystemAdapters / "Tests.Npgsql/Tests.Npgsql.csproj";
    public AbsolutePath DotnetSystemAdaptersSqlClientTests => DotnetSystemAdapters / "Tests.SqlClient/Tests.SqlClient.csproj";
    public AbsolutePath DotnetSystemAdaptersUnifiedTests => DotnetSystemAdapters / "Tests.Unified/Tests.Unified.csproj";

    public AbsolutePath DotnetSystemWorkspaceTypescript => DotnetSystem / "Workspace/Typescript";
}
