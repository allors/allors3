// <copyright file="InheritableSurfaceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    // Guards the inheritance boundary described in ARCHITECTURE.md: downstream products inherit the
    // layer folder (Core) by compile-globbing Core*. Test and showcase scaffolding lives in the
    // non-inherited Test/ folder and must never move into the layer folder, or every inheritor
    // would compile it. This test fails loudly if a test/bypass controller ever appears in an
    // inherited folder.
    public class InheritableSurfaceTests
    {
        private static readonly string[] InheritableServerFolders =
        {
            "dotnet/Core/Database/Server/Core",
            "dotnet/Identity/Database/Server/Identity",
        };

        // Core's tree: its repository, database side, workspace and tests.
        private const string CoreTreeFolder = "dotnet/Core";

        private static readonly string[] SourceFileExtensions = { ".cs", ".cshtml", ".csproj", ".resx", ".stg" };

        // The namespaces and packages of ASP.NET Core Identity.
        private static readonly Regex AspNetCoreIdentity = new(@"\bMicrosoft\.(AspNetCore|Extensions)\.Identity\b", RegexOptions.Compiled);

        // The authentication fields of User, which the Identity domain declares, and its Login class.
        // Substrings on purpose: they also catch WithUserName, ExistUserEmail, RemoveUserLockoutEnd,
        // NormalizedUserName, LoginBuilder and the like.
        private static readonly Regex AuthenticationField = new(
            @"UserName|UserEmail|UserPasswordHash|UserSecurityStamp|UserPhoneNumber|UserTwoFactorEnabled|UserLockout|UserAccessFailedCount|IsDisabled|Login",
            RegexOptions.Compiled);

        // Controllers whose name matches this pattern are test/bypass scaffolding, never production.
        private static readonly Regex ForbiddenController = new(@"\b(Test\w*|Ping)Controller\b", RegexOptions.Compiled);

        private static readonly Regex AnyController = new(@"\bclass\s+(\w+Controller)\b", RegexOptions.Compiled);

        private static readonly Regex AuthorizeAttribute = new(@"^\[Authorize[\]\(]", RegexOptions.Compiled);

        private static readonly Regex LoggingFrameworkUsing = new(@"^\s*(global\s+)?using\s+(NLog|Serilog|log4net)\b", RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex LoggingFrameworkPackage = new(@"Include=""(NLog|Serilog|log4net)\b", RegexOptions.Compiled);

        private static readonly string[] LoggingFrameworkConfigFiles = { "nlog.config", "log4net.config" };

        [Fact]
        public void InheritableServerFoldersExposeNoTestOrBypassControllers()
        {
            var root = RepositoryRoot();

            var discovered = new List<string>();
            var violations = new List<string>();

            foreach (var relativeFolder in InheritableServerFolders)
            {
                var folder = Path.Combine(root, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                foreach (var file in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(file);
                    foreach (Match match in AnyController.Matches(text))
                    {
                        var className = match.Groups[1].Value;
                        discovered.Add(className);
                        if (ForbiddenController.IsMatch(className))
                        {
                            violations.Add($"{className} in {Path.GetRelativePath(root, file)}");
                        }
                    }
                }
            }

            // Sanity: the scan actually resolved the folders and read controllers (guards against a
            // silent false-pass from a broken path).
            Assert.Contains("PullController", discovered);

            Assert.True(
                violations.Count == 0,
                "Test/bypass controllers must live in the non-inherited Test/ folder, never in the " +
                "inherited layer folder (Core), or downstream inheritors would compile them. " +
                "Offending: " + string.Join("; ", violations));
        }

        // The Allors API endpoints always require an authenticated user. Each controller in the
        // inheritable folder says so itself, so its protection does not depend on the fallback
        // policy an application chooses, and none of its actions opts out.
        [Fact]
        public void InheritableServerControllersRequireAuthorization()
        {
            var root = RepositoryRoot();

            var discovered = new List<string>();
            var violations = new List<string>();

            foreach (var relativeFolder in InheritableServerFolders)
            {
                var folder = Path.Combine(root, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                foreach (var file in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
                {
                    var lines = File.ReadAllLines(file);
                    for (var i = 0; i < lines.Length; i++)
                    {
                        var match = AnyController.Match(lines[i]);
                        if (!match.Success)
                        {
                            continue;
                        }

                        var className = match.Groups[1].Value;
                        discovered.Add(className);

                        var classAttributes = new List<string>();
                        for (var j = i - 1; j >= 0 && lines[j].TrimStart().StartsWith('['); j--)
                        {
                            classAttributes.Add(lines[j].Trim());
                        }

                        if (!classAttributes.Any(v => AuthorizeAttribute.IsMatch(v)))
                        {
                            violations.Add($"{className} in {Path.GetRelativePath(root, file)} has no [Authorize]");
                        }

                        if (lines.Any(v => v.Contains("[AllowAnonymous", StringComparison.Ordinal)))
                        {
                            violations.Add($"{className} in {Path.GetRelativePath(root, file)} has [AllowAnonymous]");
                        }
                    }
                }
            }

            // Sanity: the scan actually resolved the folders and read controllers.
            Assert.Contains("PullController", discovered);

            Assert.True(
                violations.Count == 0,
                "Every controller in the inherited layer folder (Core) must carry [Authorize] on its " +
                "class and no [AllowAnonymous], so the Allors API requires an authenticated user " +
                "whatever fallback policy an application sets. Offending: " + string.Join("; ", violations));
        }

        // Authentication is a plug-in's concern. Core's tree knows nothing of ASP.NET Core Identity,
        // neither its packages nor its namespaces: that lives in the Identity tree.
        [Fact]
        public void CoreTreeReferencesNoAspNetCoreIdentity()
        {
            var root = RepositoryRoot();
            var coreTree = Path.Combine(root, CoreTreeFolder.Replace('/', Path.DirectorySeparatorChar));

            var scanned = new List<string>();
            var violations = new List<string>();

            foreach (var file in SourceFiles(root, coreTree))
            {
                scanned.Add(Path.GetFileName(file));
                if (AspNetCoreIdentity.IsMatch(File.ReadAllText(file)))
                {
                    violations.Add(Path.GetRelativePath(root, file));
                }
            }

            // Sanity: the scan actually resolved the folder and read the server's Startup.
            Assert.Contains("Startup.cs", scanned);

            Assert.True(
                violations.Count == 0,
                "Core's tree must not reference ASP.NET Core Identity: authentication belongs to " +
                "a plug-in, such as the Identity tree under dotnet/Identity. Offending: " + string.Join("; ", violations));
        }

        // Core's User carries no authentication field: the user name, e-mail, password hash, security
        // stamp, phone number, two-factor, lockout and disabled fields and the logins belong to an
        // authentication plug-in, such as the Identity tree. Core's own test domain adds a UserName to
        // User for its population, so the compiler cannot tell inheritable code from test code in
        // Core's tree; this scan can. It covers the folders an inheritor compiles, the Core* folders.
        [Fact]
        public void InheritableCoreFoldersNameNoAuthenticationField()
        {
            var root = RepositoryRoot();
            var coreTree = Path.Combine(root, CoreTreeFolder.Replace('/', Path.DirectorySeparatorChar));

            var scanned = new List<string>();
            var violations = new List<string>();

            foreach (var folder in Directory.EnumerateDirectories(coreTree, "Core*", SearchOption.AllDirectories))
            {
                foreach (var file in SourceFiles(root, folder))
                {
                    scanned.Add(Path.GetFileName(file));
                    if (AuthenticationField.IsMatch(File.ReadAllText(file)))
                    {
                        violations.Add(Path.GetRelativePath(root, file));
                    }
                }
            }

            // Sanity: the scan actually resolved the inheritable folders and read the API.
            Assert.Contains("PullController.cs", scanned);

            Assert.True(
                violations.Count == 0,
                "Core's inheritable folders (Core*) must not name an authentication field of User or the " +
                "Login class: those belong to an authentication plug-in, such as the Identity tree under " +
                "dotnet/Identity, and an inheritor without that plug-in has no such field. " +
                "Offending: " + string.Join("; ", violations.Distinct()));
        }

        private static IEnumerable<string> SourceFiles(string root, string folder)
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                var folders = Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar);
                if (folders.Contains("bin") || folders.Contains("obj"))
                {
                    continue;
                }

                if (!SourceFileExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return file;
            }
        }

        // Allors logs through Microsoft.Extensions.Logging, and the host of an application decides
        // where the logs go. No project depends on a logging framework, so no inheritor has to ship one.
        [Fact]
        public void NoProjectDependsOnALoggingFramework()
        {
            var root = RepositoryRoot();

            var scanned = new List<string>();
            var violations = new List<string>();

            foreach (var file in Directory.EnumerateFiles(Path.Combine(root, "dotnet"), "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(root, file);
                var folders = relativePath.Split(Path.DirectorySeparatorChar);
                if (folders.Contains("bin") || folders.Contains("obj"))
                {
                    continue;
                }

                var fileName = Path.GetFileName(file);
                if (LoggingFrameworkConfigFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    violations.Add(relativePath);
                    continue;
                }

                var pattern = Path.GetExtension(file) switch
                {
                    ".cs" => LoggingFrameworkUsing,
                    ".csproj" => LoggingFrameworkPackage,
                    _ => null,
                };

                if (pattern == null)
                {
                    continue;
                }

                scanned.Add(fileName);
                if (pattern.IsMatch(File.ReadAllText(file)))
                {
                    violations.Add(relativePath);
                }
            }

            // Sanity: the scan actually resolved the folder and read the API.
            Assert.Contains("Api.cs", scanned);

            Assert.True(
                violations.Count == 0,
                "Log through Microsoft.Extensions.Logging (ILogger<T>, [LoggerMessage]) and let the host " +
                "choose the providers; do not reference a logging framework from a project. " +
                "Offending: " + string.Join("; ", violations));
        }

        private static string RepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "dotnet")) &&
                    File.Exists(Path.Combine(directory.FullName, "ARCHITECTURE.md")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException(
                $"Could not locate the repository root (a directory containing both 'dotnet/' and 'ARCHITECTURE.md') from '{AppContext.BaseDirectory}'.");
        }
    }
}
