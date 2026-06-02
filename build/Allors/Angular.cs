using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;
using static Serilog.Log;

internal class Angular : IDisposable
{
    private const int Port = 4200;

    private readonly string command;

    public Angular(AbsolutePath path, string command)
    {
        this.command = command;

        var environmentVariables = System.Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .ToDictionary(e => (string)e.Key, e => (string)e.Value, StringComparer.OrdinalIgnoreCase);
        environmentVariables["npm_config_loglevel"] = "error";
        environmentVariables["CI"] = "true";

        Process = ProcessTasks.StartProcess(
            NpmTasks.NpmPath,
            $"run {command}",
            path,
            environmentVariables);
    }

    private IProcess Process { get; set; }

    public void Dispose()
    {
        Process?.KillTree();
        Process?.Dispose();
        Process = null;
    }

    public async Task Init()
    {
        if (!await Get("/", TimeSpan.FromMinutes(10)))
        {
            throw new Exception($"Could not initialize angular ({command}). Last output:\n{OutputTail()}");
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
                throw new Exception($"Angular ({command}) exited early with code {Process.ExitCode}. Last output:\n{OutputTail()}");
            }

            await Task.Delay(1000);

            try
            {
                using var client = new HttpClient();
                Debug($"Angular request: ${url}");
                var response = await client.GetAsync($"http://localhost:{Port}{url}");
                success = response.IsSuccessStatusCode;
                var result = response.Content.ReadAsStringAsync().Result;
                if (!success)
                {
                    Warning("Angular response: Unsuccessful");
                    Warning(result);
                }
                else
                {
                    Warning("Angular response: Successful");
                    Warning(result);
                }
            }
            catch
            {
                Warning($"Angular: Exception (run {++run})");
            }
        }

        return success;
    }

    private string OutputTail() => string.Join("\n", Process.Output.TakeLast(20).Select(v => v.Text));
}
