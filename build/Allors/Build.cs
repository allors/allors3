using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tooling.ProcessTasks;

public partial class Build
{
    [Parameter("DotNet Verbosity")] private readonly DotNetVerbosity DotNetVerbosity = DotNetVerbosity.quiet;

    //[Solution] private readonly Solution Solution;
    //[GitRepository] private readonly GitRepository GitRepository;
    //[GitVersion] private readonly GitVersion GitVersion;

    private readonly Paths Paths = new Paths(RootDirectory);

    public Target EnsureDirectories => _ => _
        .Executes(() => Paths.ArtifactsTests.CreateDirectory());

    public static int Main() => Execute<Build>(x => x.Default);

    private Target Reset => _ => _
        .Executes(KillProcesses);

    static void KillProcesses()
    {
        static void TaskKill(string imageName)
        {
            try
            {
                StartProcess("taskkill", $"/IM {imageName} /F /T /FI \"PID ge 0\"").WaitForExit();
            }
            catch
            {
            }
        }

        TaskKill("node.exe");
        TaskKill("chrome.exe");
        TaskKill("chromedriver.exe");
    }
}
