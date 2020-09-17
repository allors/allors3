using System;
using Npgsql;
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

partial class Postgres : IDisposable
{
    private readonly bool alreadyListening;

    public Postgres()
    {
        using (var connection = new NpgsqlConnection("Server=localhost; User Id=postgres; Password=test; Pooling=false; Timeout=1;"))
        {
            try
            {
                connection.Open();
                this.alreadyListening = true;
            }
            catch
            {
                this.alreadyListening = false;
            }
        }

        if (!this.alreadyListening)
        {
            DockerImagePull(v => v.SetName("postgres"));
            DockerRun(v => v
                .SetDetach(true)
                .SetImage("postgres")
                .SetName("pg-docker")
                .SetPublish("5432:5432")
                .SetEnv("POSTGRES_PASSWORD=test"));
        }
    }

    public void Dispose()
    {
        if (!this.alreadyListening)
        {
        }
    }
}
