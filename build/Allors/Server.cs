using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Serilog.Log;
using static Nuke.Common.Tooling.ProcessTasks;

internal class Server : IDisposable
{
    private const int Port = 5000;

    public Server(AbsolutePath path)
    {
        var arguments = $@"{path}/Server.dll";
        var workingDirectory = path;

        Process = StartProcess(DotNetTasks.DotNetPath, arguments, workingDirectory);
    }

    private IProcess Process { get; set; }

    public void Dispose()
    {
        Process?.KillTree();
        Process?.Dispose();
        Process = null;
    }

    public async Task Ready()
    {
        if (!await Get("/allors/Test/Ready", TimeSpan.FromMinutes(5)))
        {
            throw new Exception($"Server is not ready. Last output:\n{OutputTail()}");
        }
    }

    public async Task<bool> Get(string url, TimeSpan wait)
    {
        var stop = DateTime.Now.Add(wait);
        var run = 0;

        var success = false;
        while (!success && DateTime.Now < stop)
        {
            if (Process.HasExited)
            {
                throw new Exception($"Server exited early with code {Process.ExitCode}. Last output:\n{OutputTail()}");
            }

            await Task.Delay(1000);

            try
            {
                using var client = new HttpClient();
                Debug($"Server request: ${url}");
                var response = await client.GetAsync($"http://localhost:{Port}{url}");
                success = response.IsSuccessStatusCode;
                var result = response.Content.ReadAsStringAsync().Result;
                if (!success)
                {
                    Warning("Server response: Unsuccessful");
                    Warning(result);
                }
                else
                {
                    Warning("Server response: Successful");
                    Warning(result);
                }
            }
            catch
            {
                Warning($"Server: Exception (run {++run})");
            }
        }

        return success;
    }

    private string OutputTail() => string.Join("\n", Process.Output.TakeLast(20).Select(v => v.Text));
}
