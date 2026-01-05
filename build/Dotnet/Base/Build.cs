using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

partial class Build
{

    private Target DotnetBaseMerge => _ => _
        .Executes(() => DotNetRun(s => s
            .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
            .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.DotnetBaseDatabaseResourcesBase, Paths.DotnetBaseDatabaseResources)));


    private Target DotnetBaseGenerate => _ => _
        .After(Clean)
        .DependsOn(DotnetBaseMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.DotnetBaseRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.DotnetBaseDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.DotnetBase)
                .SetProjectFile(Paths.DotnetBaseDatabaseGenerate));
        });


    private Target DotnetBase => _ => _
        .DependsOn(DotnetBaseGenerate);
}
