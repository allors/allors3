// <copyright file="DomainIsolationTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Xunit;

    // A domain knows the domains it extends and nothing of the others. Level1 extends Core, so its
    // folders name no type of Level2, of Plugin1 or of Test; Level2 extends Level1, so its folders
    // name no type of Plugin1 or of Test; the plug-in Plugin1 extends Core, so its folders name no
    // type of Level1, of Level2 or of Test. A folder that broke this would compile here, where the
    // concrete domain extends everything, and fail in an application that extends less.
    public class DomainIsolationTests
    {
        private const string TreeFolder = "dotnet/Diamond";

        private const string CoreTreeFolder = "dotnet/Core";

        // The domains of the tree, each with the domains it extends.
        private static readonly (string Domain, string[] Extends)[] Domains =
        {
            ("Level1", new[] { "Core" }),
            ("Level2", new[] { "Core", "Level1" }),
            ("Plugin1", new[] { "Core" }),
            ("Test", new[] { "Core", "Level1", "Level2", "Plugin1" }),
        };

        private static readonly string[] SourceFileExtensions = { ".cs", ".resx" };

        // A declaration at the start of a line, not the word class in a comment.
        private static readonly Regex TypeDeclaration = new(
            @"^\s*public\s+(?:static\s+|abstract\s+|sealed\s+|partial\s+)*(?:class|interface|struct|record)\s+(\w+)\b",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex ClassDeclaration = new(@"^\s*public\s+(?:partial\s+)?class\s+(\w+)\b", RegexOptions.Compiled | RegexOptions.Multiline);

        // The plurals that the name with an s does not give.
        private static readonly Dictionary<string, string> IrregularPlurals = new()
        {
            ["Person"] = "People",
        };

        [Fact]
        public void ADomainNamesNoTypeOfADomainItDoesNotExtend()
        {
            var root = RepositoryRoot();
            var tree = Path.Combine(root, TreeFolder.Replace('/', Path.DirectorySeparatorChar));
            var coreTree = Path.Combine(root, CoreTreeFolder.Replace('/', Path.DirectorySeparatorChar));

            // The names Core declares in its inheritable folders. A partial declaration of one of
            // them in a domain's folder extends Core's type; it is not a type of that domain.
            var coreNames = Directory.EnumerateDirectories(coreTree, "Core*", SearchOption.AllDirectories)
                .SelectMany(folder => SourceFiles(root, folder))
                .SelectMany(DeclaredNames)
                .ToHashSet();

            var declaredNamesByDomain = Domains.ToDictionary(v => v.Domain, v => DomainFolders(root, tree, v.Domain).SelectMany(folder => SourceFiles(root, folder)).SelectMany(DeclaredNames).ToHashSet());

            // The types of a domain: what its folders declare, minus Core's names and minus a
            // partial of a type of a domain it extends; for a class of its repository also the
            // builder and the extent the generator derives from it.
            var typesByDomain = new Dictionary<string, HashSet<string>>();
            foreach (var (domain, extends) in Domains)
            {
                var inherited = coreNames.Concat(extends.Where(declaredNamesByDomain.ContainsKey).SelectMany(v => declaredNamesByDomain[v])).ToHashSet();
                var own = declaredNamesByDomain[domain].Where(v => !inherited.Contains(v)).ToHashSet();

                var repositoryClasses = SourceFiles(root, Path.Combine(tree, "Repository", "Domain", domain))
                    .SelectMany(file => ClassDeclaration.Matches(File.ReadAllText(file)).Select(match => match.Groups[1].Value))
                    .Where(own.Contains)
                    .ToArray();

                foreach (var name in repositoryClasses)
                {
                    own.Add(name + "Builder");
                    own.Add(IrregularPlurals.TryGetValue(name, out var plural) ? plural : name + "s");
                }

                typesByDomain[domain] = own;
            }

            // Sanity: the scan actually found the types the domains declare.
            Assert.Contains("Level1Item", typesByDomain["Level1"]);
            Assert.Contains("ILevel1Log", typesByDomain["Level1"]);
            Assert.Contains("Level2Item", typesByDomain["Level2"]);
            Assert.DoesNotContain("Level1Item", typesByDomain["Level2"]);
            Assert.Contains("IPlugin1Log", typesByDomain["Plugin1"]);
            Assert.DoesNotContain("User", typesByDomain["Plugin1"]);
            Assert.Contains("Person", typesByDomain["Test"]);
            Assert.Contains("PersonBuilder", typesByDomain["Test"]);
            Assert.Contains("People", typesByDomain["Test"]);
            Assert.Contains("HookLog", typesByDomain["Test"]);

            var scanned = new List<string>();
            var violations = new List<string>();

            foreach (var (domain, extends) in Domains)
            {
                var forbidden = Domains
                    .Select(v => v.Domain)
                    .Where(v => v != domain && !extends.Contains(v))
                    .SelectMany(v => typesByDomain[v])
                    .Distinct()
                    .ToArray();

                if (forbidden.Length == 0)
                {
                    continue;
                }

                var forbiddenName = new Regex(@"\b(" + string.Join("|", forbidden.Select(Regex.Escape)) + @")\b");

                foreach (var folder in DomainFolders(root, tree, domain))
                {
                    foreach (var file in SourceFiles(root, folder))
                    {
                        scanned.Add(Path.GetFileName(file));
                        var match = forbiddenName.Match(File.ReadAllText(file));
                        if (match.Success)
                        {
                            violations.Add($"{match.Value} in {Path.GetRelativePath(root, file)}");
                        }
                    }
                }
            }

            // Sanity: the scan actually resolved the folders of the three domains and read their declarations.
            Assert.Contains("Level1.cs", scanned);
            Assert.Contains("Level2.cs", scanned);
            Assert.Contains("Plugin1.cs", scanned);

            Assert.True(
                violations.Count == 0,
                "The folders of a domain must not name a type of a domain it does not extend, nor its " +
                "builder or extent: an application that extends less has no such type. " +
                "Offending: " + string.Join("; ", violations.Distinct()));
        }

        // The folders of a domain: those named after it, directly in a project that is not a test project.
        private static IEnumerable<string> DomainFolders(string root, string tree, string domain) =>
            SourceFiles(root, tree, ".csproj")
                .Select(Path.GetDirectoryName)
                .Where(project => !Path.GetFileName(project).EndsWith("Tests", StringComparison.Ordinal))
                .SelectMany(project => Directory.EnumerateDirectories(project, domain + "*", SearchOption.TopDirectoryOnly));

        private static IEnumerable<string> DeclaredNames(string file) => TypeDeclaration.Matches(File.ReadAllText(file)).Select(match => match.Groups[1].Value);

        private static IEnumerable<string> SourceFiles(string root, string folder, params string[] extensions)
        {
            var sourceFileExtensions = extensions.Length == 0 ? SourceFileExtensions : extensions;

            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                var folders = Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar);
                if (folders.Contains("bin") || folders.Contains("obj"))
                {
                    continue;
                }

                if (!sourceFileExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                yield return file;
            }
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
