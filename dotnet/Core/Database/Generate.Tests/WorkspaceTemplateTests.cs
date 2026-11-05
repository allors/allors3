// <copyright file="WorkspaceTemplateTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Meta.Generation.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Meta.Generation.Model;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit;

    public sealed class WorkspaceTemplateTests : IDisposable
    {
        // No composite is assigned to this workspace
        private const string Unassigned = "Unassigned";

        private readonly string root;

        public WorkspaceTemplateTests()
        {
            this.root = Path.Combine(Path.GetTempPath(), "allors-generate-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(this.root);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(this.root, recursive: true);
            }
            catch (IOException)
            {
                // best-effort cleanup of the temp tree
            }
        }

        // Default has inheritance, the other workspaces have none
        [Theory]
        [InlineData("Default")]
        [InlineData("X")]
        [InlineData("Y")]
        [InlineData(Unassigned)]
        public void MetaLazyCompiles(string workspaceName)
        {
            var model = new MetaModel(new MetaBuilder().Build());

            this.Generate(model, "meta.cs.stg", workspaceName);
            this.Generate(model, "meta.lazy.cs.stg", workspaceName);

            var errors = this.Compile();

            Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
        }

        private void Generate(MetaModel model, string template, string workspaceName)
        {
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", template);
            var output = Path.Combine(this.root, Path.GetFileNameWithoutExtension(template));

            var log = Generation.Generate.Execute(model, templatePath, output, workspaceName);

            Assert.False(log.ErrorOccured);
        }

        private string[] Compile()
        {
            var syntaxTrees = Directory
                .EnumerateFiles(this.root, "*.cs", SearchOption.AllDirectories)
                .Select(v => CSharpSyntaxTree.ParseText(File.ReadAllText(v), path: v))
                .ToArray();

            Assert.NotEmpty(syntaxTrees);

            var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var references = new[]
                {
                    Path.Combine(runtime, "System.Runtime.dll"),
                    Path.Combine(runtime, "netstandard.dll"),
                    typeof(object).Assembly.Location,
                    typeof(UnitTags).Assembly.Location,
                    typeof(Allors.Workspace.Meta.IMetaPopulation).Assembly.Location,
                    typeof(Allors.Workspace.Meta.Inheritance).Assembly.Location,
                }
                .Select(v => MetadataReference.CreateFromFile(v));

            var compilation = CSharpCompilation.Create(
                "Workspace.Meta.Lazy",
                syntaxTrees,
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return compilation.GetDiagnostics()
                .Where(v => v.Severity == DiagnosticSeverity.Error)
                .Select(v => v.ToString().Replace(this.root, string.Empty))
                .ToArray();
        }
    }
}
