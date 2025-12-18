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
    public AbsolutePath DotnetSystemAdaptersStaticMemoryTests => DotnetSystemAdapters / "Tests.Static.Memory/Tests.Static.Memory.csproj";
    public AbsolutePath DotnetSystemAdaptersStaticNpgsqlTests => DotnetSystemAdapters / "Tests.Static.Npgsql/Tests.Static.Npgsql.csproj";
    public AbsolutePath DotnetSystemAdaptersStaticSqlClientTests => DotnetSystemAdapters / "Tests.Static.SqlClient/Tests.Static.SqlClient.csproj";
    public AbsolutePath DotnetSystemAdaptersStaticUnifiedTests => DotnetSystemAdapters / "Tests.Static.Unified/Tests.Static.Unified.csproj";

    public AbsolutePath DotnetSystemWorkspaceTypescript => DotnetSystem / "Workspace/Typescript";
}
