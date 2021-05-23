using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{
    private Target DotnetSystemAdaptersGenerate => _ => _
        .After(this.Clean)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(this.Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(
                    $"{this.Paths.DotnetSystemAdaptersRepositoryDomainRepository} {this.Paths.DotnetSystemRepositoryTemplatesMetaCs} {this.Paths.DotnetSystemAdaptersMetaGenerated}"));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(this.Paths.DotnetSystemAdapters)
                .SetProjectFile(this.Paths.DotnetSystemAdaptersGenerate));
        });

    private Target DotnetSystemAdaptersTestMemory => _ => _
        .DependsOn(this.DotnetSystemAdaptersGenerate)
        .Executes(() => DotNetTest(s => s
            .SetProjectFile(this.Paths.DotnetSystemAdaptersStaticTests)
            .SetFilter("FullyQualifiedName~Allors.Database.Adapters.Memory")
            .SetLogger("trx;LogFileName=AdaptersMemory.trx")
            .SetResultsDirectory(this.Paths.ArtifactsTests)));

    private Target DotnetSystemAdaptersTestSqlClient => _ => _
        .DependsOn(this.DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            using (new SqlServer())
            {
                DotNetTest(s => s
                    .SetProjectFile(this.Paths.DotnetSystemAdaptersStaticTests)
                    .SetFilter("FullyQualifiedName~Allors.Database.Adapters.SqlClient")
                    .SetLogger("trx;LogFileName=AdaptersSqlClient.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));
            }
        });

    private Target DotnetSystemAdaptersTestNpgsql => _ => _
        .DependsOn(this.DotnetSystemAdaptersGenerate)
        .Executes(() =>
        {
            using (new Postgres())
            {
                DotNetTest(s => s
                    .SetProjectFile(this.Paths.DotnetSystemAdaptersStaticTests)
                    .SetFilter("FullyQualifiedName~Allors.Database.Adapters.Npgsql")
                    .SetLogger("trx;LogFileName=AdaptersNpgsql.trx")
                    .SetResultsDirectory(this.Paths.ArtifactsTests));
            }
        });

    private Target DotnetSystemInstall => _ => _
        .Executes(() => NpmInstall(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetSystemWorkspaceTypescript)));

    private Target DotnetSystemWorkspaceTypescript => _ => _
        .After(this.DotnetSystemInstall)
        .DependsOn(this.EnsureDirectories)
        .Executes(() => NpmRun(s => s
            .AddProcessEnvironmentVariable("npm_config_loglevel", "error")
            .SetProcessWorkingDirectory(this.Paths.DotnetSystemWorkspaceTypescript)
            .SetCommand("test:all")));
    
    private Target DotnetSystemWorkspaceTest => _ => _
        .DependsOn(this.DotnetSystemWorkspaceTypescript);

    private Target DotnetSystemAdapters => _ => _
        .DependsOn(this.Clean)
        .DependsOn(this.DotnetSystemAdaptersTestMemory)
        .DependsOn(this.DotnetSystemAdaptersTestSqlClient)
        .DependsOn(this.DotnetSystemAdaptersTestNpgsql);
}
