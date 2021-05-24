using System.IO;
using System.Linq;
using Nuke.Common;
using static Nuke.Common.IO.FileSystemTasks;

partial class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    private Target Install => _ => _
        .DependsOn(DotnetCoreInstall)
        .DependsOn(DotnetBaseInstall)
        .DependsOn(DotnetAppsInstall);

    private Target Clean => _ => _
        .Executes(() =>
        {
            void Delete(DirectoryInfo directoryInfo)
            {
                directoryInfo.Refresh();
                if (directoryInfo.Exists)
                {
                    if (new[] {"node_modules", "packages", "out-tsc", "bin", "obj", "generated"}.Contains(
                        directoryInfo.Name.ToLowerInvariant()))
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

            foreach (var path in new[] {Paths.DotnetSystem, Paths.DotnetCore, Paths.DotnetApps})
            {
                foreach (var child in new DirectoryInfo(path).GetDirectories().Where(v => !v.Name.Equals("build")))
                {
                    Delete(child);
                }
            }

            DeleteDirectory(Paths.Artifacts);
        });

    private Target Generate => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .DependsOn(DotnetCoreGenerate)
        .DependsOn(DotnetBaseGenerate)
        .DependsOn(DotnetAppsGenerate)
        .DependsOn(DemosDerivationGenerate)
        .DependsOn(DemosSecurityGenerate);

    private Target Default => _ => _
        .DependsOn(Generate);

    private Target All => _ => _
        .DependsOn(Install)
        .DependsOn(Generate);
}
