// <copyright file="RepositoryDomainsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Meta.Generation.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Allors.Repository.Attributes;
    using Allors.Repository.Domain;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.Extensions.Logging;
    using Xunit;

    // The repository parser reads the [Domain] structs of a repository project and the domains each
    // one extends. The projects here are built in memory, one folder per domain, as a repository is.
    public sealed class RepositoryDomainsTests : IDisposable
    {
        private readonly string root;
        private readonly TestLogger logger;

        public RepositoryDomainsTests()
        {
            this.root = Path.Combine(Path.GetTempPath(), "allors-generate-tests", Path.GetRandomFileName());
            Directory.CreateDirectory(this.root);
            this.logger = new TestLogger();
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

        [Fact]
        public void ADomainExtendsSeveralDomains()
        {
            var repository = this.Repository(
                Domain("Core", "11111111-1111-1111-1111-111111111111"),
                Domain("Level1", "22222222-2222-2222-2222-222222222222", "nameof(Core)"),
                Domain("Plugin1", "33333333-3333-3333-3333-333333333333", "nameof(Core)"),
                Domain("Test", "44444444-4444-4444-4444-444444444444", "nameof(Level1), nameof(Plugin1)"));

            Assert.False(repository.HasErrors);
            Assert.Empty(this.logger.Errors);
            Assert.Equal(new[] { "Core", "Level1", "Plugin1", "Test" }, repository.Domains.Select(v => v.Name).OrderBy(v => v));
            Assert.Empty(Superdomains(repository, "Core"));
            Assert.Equal(new[] { "Core" }, Superdomains(repository, "Level1"));
            Assert.Equal(new[] { "Core" }, Superdomains(repository, "Plugin1"));
            Assert.Equal(new[] { "Level1", "Plugin1" }, Superdomains(repository, "Test"));
        }

        [Fact]
        public void ADomainExtendsOneDomainByNameOrByString()
        {
            var repository = this.Repository(
                Domain("Core", "11111111-1111-1111-1111-111111111111"),
                Domain("Identity", "22222222-2222-2222-2222-222222222222", "nameof(Core)"),
                Domain("Test", "44444444-4444-4444-4444-444444444444", "\"Identity\""));

            Assert.False(repository.HasErrors);
            Assert.Equal(new[] { "Core" }, Superdomains(repository, "Identity"));
            Assert.Equal(new[] { "Identity" }, Superdomains(repository, "Test"));
        }

        [Fact]
        public void AnUnknownSuperdomainIsAnError()
        {
            var repository = this.Repository(
                Domain("Core", "11111111-1111-1111-1111-111111111111"),
                Domain("Test", "44444444-4444-4444-4444-444444444444", "nameof(Core), \"Sales\""));

            Assert.True(repository.HasErrors);
            Assert.Equal(
                new[] { "Test extends Sales, but no [Domain] struct has that name. Declared domains: Core, Test." },
                this.logger.Errors);
            Assert.Equal(new[] { "Core" }, Superdomains(repository, "Test"));
        }

        [Fact]
        public void ADuplicateDomainNameIsAnError()
        {
            var repository = this.Repository(
                Domain("Core", "11111111-1111-1111-1111-111111111111"),
                ("Sales", "namespace Sales { using Allors.Repository.Attributes; [Domain(\"22222222-2222-2222-2222-222222222222\")] [Extends(nameof(Core))] public struct Test { } }"),
                ("Stock", "namespace Stock { using Allors.Repository.Attributes; [Domain(\"33333333-3333-3333-3333-333333333333\")] [Extends(nameof(Core))] public struct Test { } }"));

            Assert.True(repository.HasErrors);
            Assert.Equal(
                new[] { $"Two [Domain] structs are named Test: in {Path.Combine(this.root, "Sales")} and in {Path.Combine(this.root, "Stock")}. Give each domain its own name." },
                this.logger.Errors);
            Assert.Equal(new[] { "Core", "Test" }, repository.Domains.Select(v => v.Name).OrderBy(v => v));
        }

        [Fact]
        public void ASuperdomainListedTwiceIsAnError()
        {
            var repository = this.Repository(
                Domain("Core", "11111111-1111-1111-1111-111111111111"),
                Domain("Test", "44444444-4444-4444-4444-444444444444", "nameof(Core), \"Core\""));

            Assert.True(repository.HasErrors);
            Assert.Equal(new[] { "Test extends Core twice. List each domain once in [Extends]." }, this.logger.Errors);
            Assert.Equal(new[] { "Core" }, Superdomains(repository, "Test"));
        }

        [Fact]
        public void ADomainThatExtendsItselfIsAnError()
        {
            var repository = this.Repository(
                Domain("Core", "11111111-1111-1111-1111-111111111111"),
                Domain("Test", "44444444-4444-4444-4444-444444444444", "nameof(Test), nameof(Core)"));

            Assert.True(repository.HasErrors);
            Assert.Equal(new[] { "Test extends itself. Remove Test from its [Extends]." }, this.logger.Errors);
            Assert.Equal(new[] { "Core" }, Superdomains(repository, "Test"));
        }

        private static (string Domain, string Source) Domain(string name, string id, string extends = null)
        {
            var extendsAttribute = extends == null ? string.Empty : $"[Extends({extends})] ";
            return (name, $"using Allors.Repository.Attributes; [Domain(\"{id}\")] {extendsAttribute}public struct {name} {{ }}");
        }

        private static string[] Superdomains(Repository repository, string name) =>
            repository.DomainByName[name].DirectSuperdomains.Select(v => v.Name).OrderBy(v => v).ToArray();

        private Repository Repository(params (string Domain, string Source)[] files)
        {
            var runtime = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var references = new[]
                {
                    Path.Combine(runtime, "System.Runtime.dll"),
                    Path.Combine(runtime, "netstandard.dll"),
                    typeof(object).Assembly.Location,
                    typeof(DomainAttribute).Assembly.Location,
                }
                .Select(v => MetadataReference.CreateFromFile(v));

            using var workspace = new AdhocWorkspace();
            var projectId = ProjectId.CreateNewId();
            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "Repository",
                "Repository",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: references);
            workspace.AddProject(projectInfo);

            foreach (var (domain, source) in files)
            {
                var path = Path.Combine(this.root, domain, domain + ".cs");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, source);

                var text = TextAndVersion.Create(SourceText.From(source), VersionStamp.Create(), path);
                workspace.AddDocument(DocumentInfo.Create(DocumentId.CreateNewId(projectId), domain + ".cs", loader: TextLoader.From(text), filePath: path));
            }

            return new Repository(workspace.CurrentSolution.GetProject(projectId), this.logger);
        }

        private sealed class TestLogger : ILogger
        {
            public List<string> Errors { get; } = new List<string>();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel >= LogLevel.Error)
                {
                    this.Errors.Add(formatter(state, exception));
                }
            }
        }
    }
}
