using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// The sample application takes the platform as an application does: its projects include the folders
// named Core by path and never the folders named Custom. The targets run the tools of the platform on
// the sample's own projects, so a change to Core that an application has to follow fails here.
partial class Build
{
    private Target SampleMerge => _ => _
        .Executes(() =>
        {
            Paths.SampleDatabaseResourcesGenerated.CreateDirectory();
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetCoreDatabaseMerge)
                .SetApplicationArguments(Paths.DotnetCoreDatabaseResourcesCore, Paths.SampleDatabaseResourcesGenerated));
        });

    private Target SampleGenerate => _ => _
        .After(Clean)
        .DependsOn(SampleMerge)
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(Paths.DotnetSystemRepositoryGenerate)
                .SetApplicationArguments(Paths.SampleRepositoryDomainRepository, Paths.DotnetSystemRepositoryTemplatesMetaCs, Paths.SampleDatabaseMetaGenerated));
            DotNetRun(s => s
                .SetProcessWorkingDirectory(Paths.Sample)
                .SetProjectFile(Paths.SampleDatabaseGenerate));
        });

    // The domain tests run on the Memory adapter. The tests of the command-line tool set the adapter Snapshot
    // in the environment, and the tests of the server give it to the server in its settings. The tests of the
    // workspace run on the Local and the Remote adapter, against a database on Snapshot that they configure
    // themselves, so none needs a database server.
    private Target SampleTest => _ => _
        .DependsOn(SampleGenerate)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Paths.SampleDatabaseDomainTests)
                .AddLoggers("trx;LogFileName=Sample.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
            DotNetTest(s => s
                .SetProjectFile(Paths.SampleDatabaseCommandsTests)
                .AddLoggers("trx;LogFileName=SampleCommands.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
            DotNetTest(s => s
                .SetProjectFile(Paths.SampleDatabaseServerTests)
                .AddLoggers("trx;LogFileName=SampleServer.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
            DotNetTest(s => s
                .SetProjectFile(Paths.SampleWorkspaceTests)
                .AddLoggers("trx;LogFileName=SampleWorkspace.trx")
                .SetResultsDirectory(Paths.ArtifactsTests));
        });
}
