using Nuke.Common;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Build
{
    Target Postgres => _ => _
        .Executes(() =>
        {
            DockerImagePull(v => v.SetName($"postgres"));
            DockerRun(v => v
                .SetDetach(true)
                .SetImage($"postgres")
                .SetName($"pg")
                .SetPublish("5432:5432")
                .AddEnv("POSTGRES_USER=test")
                .AddEnv("POSTGRES_PASSWORD=test"));
        });
}

