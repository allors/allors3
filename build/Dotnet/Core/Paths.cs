using Nuke.Common.IO;

public partial class Paths
{
    public AbsolutePath DotnetCore => Dotnet / "Core";
    
    public AbsolutePath DotnetCoreRepositoryDomainRepository => DotnetCore / "Repository/Domain/Repository.csproj";

    public AbsolutePath DotnetCoreDatabase => DotnetCore / "Database";
    public AbsolutePath DotnetCoreDatabaseMetaGenerated => DotnetCoreDatabase / "Meta/Generated";
    public AbsolutePath DotnetCoreDatabaseGenerate => DotnetCoreDatabase / "Generate/Generate.csproj";
    public AbsolutePath DotnetCoreDatabaseMerge => DotnetCoreDatabase / "Merge/Merge.csproj";
    public AbsolutePath DotnetCoreDatabaseResources => DotnetCoreDatabase / "Resources";
    public AbsolutePath DotnetCoreDatabaseResourcesCore => DotnetCoreDatabaseResources / "Core";
}
