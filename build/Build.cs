using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;

partial class Build : NukeBuild
{
    private const string ConfigRootVariable = "ALLORS_CONFIG_ROOT";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The template under config/ that is the configuration root of the processes the build starts: snapshot, sqlclient, npgsql or localdb. Without it the build uses ALLORS_CONFIG_ROOT, or config/snapshot when that is not set")]
    private readonly string Provider;

    /// <summary>
    /// Points <c>ALLORS_CONFIG_ROOT</c> at the chosen template for the tests, the command-line tool and
    /// the server that the targets start. An explicit <c>--provider</c> wins over the variable; without
    /// either, the Snapshot template keeps a run free of database servers.
    /// </summary>
    protected override void OnBuildInitialized()
    {
        base.OnBuildInitialized();

        var current = System.Environment.GetEnvironmentVariable(ConfigRootVariable);
        if (Provider == null && !string.IsNullOrWhiteSpace(current))
        {
            Serilog.Log.Information("Configuration root: {Root} ({Variable})", current, ConfigRootVariable);
            return;
        }

        var provider = Provider ?? "snapshot";
        var root = RootDirectory / "config" / provider;
        if (!root.DirectoryExists())
        {
            var templates = string.Join(", ", (RootDirectory / "config").GetDirectories().Select(v => v.Name).OrderBy(v => v));
            Assert.Fail($"There is no configuration template config/{provider}. Pass --provider with one of {templates}, or set {ConfigRootVariable} to a configuration root.");
        }

        System.Environment.SetEnvironmentVariable(ConfigRootVariable, root);
        Serilog.Log.Information("Configuration root: {Root} ({Source})", root, Provider == null ? "the default, " + ConfigRootVariable + " is not set" : "--provider");
    }

    private Target Clean => _ => _
        .Executes(() =>
        {
            static void Delete(DirectoryInfo directoryInfo)
            {
                directoryInfo.Refresh();
                if (!directoryInfo.Exists) return;
                if (new[] { "node_modules", "packages", "out-tsc", "bin", "obj", "generated" }.Contains(
                    directoryInfo.Name.ToLowerInvariant()))
                {
                    ((AbsolutePath)directoryInfo.FullName).DeleteDirectory();
                    return;
                }

                if (directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)) return;
                foreach (var child in directoryInfo.GetDirectories())
                {
                    Delete(child);
                }
            }

            foreach (var path in new[] { Paths.DotnetSystem, Paths.DotnetCore })
            {
                foreach (var child in new DirectoryInfo(path).GetDirectories().Where(v => !v.Name.Equals("build")))
                {
                    Delete(child);
                }
            }

            Paths.Artifacts.DeleteDirectory();
        });


    private Target Install => _ => _
        .DependsOn(TypescriptInstall);

    private Target Merge => _ => _
       .DependsOn(DotnetCoreMerge);

    private Target Generate => _ => _
        .DependsOn(DotnetSystemAdaptersGenerate)
        .DependsOn(DotnetCoreGenerate);

    private Target Default => _ => _
        .DependsOn(Install)
        .DependsOn(Generate);

    private Target ShowVersion => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Version:                      {V}", Versioning.Version);
            Serilog.Log.Information("AssemblyVersion:              {V}", Versioning.AssemblyVersion);
            Serilog.Log.Information("AssemblyInformationalVersion: {V}", Versioning.AssemblyInformationalVersion);
            Serilog.Log.Information("NuGetPackageVersion:          {V}", Versioning.NuGetPackageVersion);
            Serilog.Log.Information("NpmPackageVersion:            {V}", Versioning.NpmPackageVersion);
        });
}
