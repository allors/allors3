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
        };

        // Controllers whose name matches this pattern are test/bypass scaffolding, never production.
        private static readonly Regex ForbiddenController = new(@"\b(Test\w*|Ping)Controller\b", RegexOptions.Compiled);

        private static readonly Regex AnyController = new(@"\bclass\s+(\w+Controller)\b", RegexOptions.Compiled);

        private static readonly Regex AuthorizeAttribute = new(@"^\[Authorize[\]\(]", RegexOptions.Compiled);

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
