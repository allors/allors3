using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using static Nuke.Common.IO.FileSystemTasks;

[CheckBuildProjectConfigurations(TimeoutInMilliseconds = 5000)]
[UnsetVisualStudioEnvironmentVariables]
partial class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Install => _ => _
        .DependsOn(this.CoreInstall)
        .DependsOn(this.AppsInstall);

    Target Clean => _ => _
        .Executes(() =>
        {
            void Delete(DirectoryInfo directoryInfo)
            {
                directoryInfo.Refresh();
                if (directoryInfo.Exists)
                {
                    if (new[] { "node_modules", "packages", "out-tsc", "bin", "obj", "generated" }.Contains(directoryInfo.Name.ToLowerInvariant()))
                    {
                        DeleteDirectory(directoryInfo.FullName);
                        return;
                    }

                    if (!directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        foreach (var child in directoryInfo.GetDirectories())
                        {
                            Delete(child);
                        }
                    }
                }
            }

            foreach (var path in new AbsolutePath[] {this.Paths.Platform, this.Paths.Core, this.Paths.Apps })
            {
                foreach (var child in new DirectoryInfo(path).GetDirectories().Where(v => !v.Name.Equals("build")))
                {
                    Delete(child);
                }
            }

            DeleteDirectory(this.Paths.Artifacts);
        });

    Target Generate => _ => _
        .DependsOn(this.AdaptersGenerate)
        .DependsOn(this.CoreGenerate)
        .DependsOn(this.AppsGenerate);

    Target Default => _ => _
        .DependsOn(this.Generate);

    Target All => _ => _
        .DependsOn(this.Install)
        .DependsOn(this.Generate);
}
