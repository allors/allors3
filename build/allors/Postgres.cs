using Nuke.Common;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;


partial class Build
{
    private void postgres(int version)
    {
        DockerImagePull(v => v.SetName("postgres"));
        DockerRun(v => v
            .SetDetach(true)
            .SetImage($"postgres:{version}")
            .SetName($"pg{version}")
            .SetPublish("5432:5432")
            .AddEnv("POSTGRES_USER=test")
            .AddEnv("POSTGRES_PASSWORD=test"));
    }

    Target Postgres8 => _ => _
        .Executes(() =>
        {
            this.postgres(8);
        });

    Target Postgres12 => _ => _
        .Executes(() =>
        {
            this.postgres(12);
        });
}

