// <copyright file="VirtualDispatchTests.cs" company="Allors bv">
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

    // Guards the hand-maintained Virtual/*.v.cs dispatch shims. Each wrapper method must invoke
    // only the phase-matched layer hooks (inside OnPrePrepare() only {Layer}OnPrePrepare() may be
    // called, inside Secure() only {Layer}Secure(), ...). The hooks are bound by name, so a
    // copy-paste slip silently skips one layer's phase hook and runs another twice - invisible
    // while the hooks are empty, a latent ordering bug the moment a layer fills one in.
    public class VirtualDispatchTests
    {
        private static readonly Regex WrapperDeclaration = new(@"\bvoid\s+(\w+)\s*\(", RegexOptions.Compiled);

        private static readonly Regex LayerHookCall = new(@"\bthis\.(Core|Base|Apps|Test)(\w+)\s*\(", RegexOptions.Compiled);

        [Fact]
        public void VirtualShimsDispatchPhaseMatchedHooks()
        {
            var root = RepositoryRoot();

            var shims = Directory
                .EnumerateFiles(Path.Combine(root, "dotnet"), "*.v.cs", SearchOption.AllDirectories)
                .Where(v => Path.GetFileName(Path.GetDirectoryName(v)) == "Virtual")
                .Where(v => !v.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                            !v.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .ToArray();

            var violations = new List<string>();
            var inspectedCalls = 0;

            foreach (var shim in shims)
            {
                string wrapper = null;
                var lines = File.ReadAllLines(shim);
                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    var call = LayerHookCall.Match(line);
                    if (call.Success)
                    {
                        inspectedCalls++;
                        var hook = call.Groups[2].Value;
                        if (wrapper == null || hook != wrapper)
                        {
                            violations.Add(
                                $"{Path.GetRelativePath(root, shim)}:{i + 1}: {wrapper ?? "<no wrapper>"} dispatches " +
                                $"{call.Groups[1].Value}{hook}");
                        }

                        continue;
                    }

                    var declaration = WrapperDeclaration.Match(line);
                    if (declaration.Success)
                    {
                        wrapper = declaration.Groups[1].Value;
                    }
                }
            }

            // Sanity: the scan actually resolved shims and hook calls (guards against a silent
            // false-pass from a broken path or pattern).
            Assert.True(shims.Length >= 9, $"Expected at least 9 Virtual/*.v.cs shims, found {shims.Length}.");
            Assert.True(inspectedCalls >= 30, $"Expected at least 30 layer hook calls, inspected {inspectedCalls}.");

            Assert.True(
                violations.Count == 0,
                "A Virtual/*.v.cs wrapper must dispatch only its phase-matched layer hooks " +
                "({Layer} + wrapper name); a mismatched call skips one layer's hook for that phase " +
                "and runs another phase's hook twice. Offending: " + string.Join("; ", violations));
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
