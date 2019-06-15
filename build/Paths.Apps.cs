using static Nuke.Common.IO.PathConstruction;

public partial class Paths
{
    public AbsolutePath Apps => Root / "apps";
    public AbsolutePath AppsRepositoryDomainRepository => Apps / "Repository/Domain/Repository.csproj";
    public AbsolutePath AppsDatabaseMetaGenerated => Apps / "Database/Meta/generated";
    public AbsolutePath AppsDatabaseGenerate => Apps / "Database/Generate/Generate.csproj";
    public AbsolutePath AppsDatabaseServer => Apps / "Database/Server";
    public AbsolutePath AppsDatabaseServerProject => AppsDatabaseServer / "Server.csproj";
    public AbsolutePath AppsDatabaseDomainTests => Apps / "Database/Domain.Tests/Domain.Tests.csproj";

    public AbsolutePath AppsWorkspaceTypescriptDomain => Apps / "Workspace/Typescript/Domain";
    public AbsolutePath AppsWorkspaceTypescriptIntranet => Apps / "Workspace/Typescript/Intranet";
    public AbsolutePath AppsWorkspaceTypescriptIntranetTrx => AppsWorkspaceTypescriptIntranet / "dist/AppsWorkspaceTypescriptIntranet.trx";
    public AbsolutePath AppsWorkspaceTypescriptIntranetTests => Apps / "Workspace/Typescript/Intranet.Tests/Intranet.Tests.csproj";
    public AbsolutePath AppsWorkspaceTypescriptAutotestAngular => Apps / "Workspace/Typescript/Autotest/Angular";
    public AbsolutePath AppsWorkspaceTypescriptAutotestGenerateGenerate => Apps / "Workspace/Typescript/Autotest/Generate/Generate.csproj";
    public AbsolutePath[] AppsWorkspaceTypescript => new[]
    {
        AppsWorkspaceTypescriptDomain,
        AppsWorkspaceTypescriptIntranet,
        AppsWorkspaceTypescriptAutotestAngular
    };
}