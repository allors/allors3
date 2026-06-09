// -------------------------------------------------------------------------------------------------
// <copyright file="MergerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// ---------------------------------------------------------------------------------------------

namespace Allors.R1.Development.Resources.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public sealed class MergerTests : IDisposable
    {
        private static readonly XNamespace Xsd = "http://www.w3.org/2001/XMLSchema";
        private static readonly XNamespace Xml = "http://www.w3.org/XML/1998/namespace";

        private readonly string root;

        public MergerTests()
        {
            this.root = Path.Combine(Path.GetTempPath(), "allors-merge-tests", Path.GetRandomFileName());
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

        [Fact]
        public void OverlappingKeyPreservesValueAndCommentStructure()
        {
            var first = this.Dir("first");
            var second = this.Dir("second");
            WriteResx(first, "Errors.resx", Data("Greeting", "Hello", "original comment"));
            WriteResx(second, "Errors.resx", Data("Greeting", "Howdy", "override comment"));

            var greeting = this.MergeAndRead("Errors.resx", first, second).Single(d => Name(d) == "Greeting");

            // BUG-09 guard: the merged element keeps its <value>/<comment> children
            // instead of being flattened into a single raw text node.
            Assert.Equal("Howdy", (string)greeting.Element("value"));
            Assert.Equal("override comment", (string)greeting.Element("comment"));
            Assert.DoesNotContain(greeting.Nodes().OfType<XText>(), t => !string.IsNullOrWhiteSpace(t.Value));
        }

        [Fact]
        public void OverlappingKeyPreservesAttributes()
        {
            var first = this.Dir("first");
            var second = this.Dir("second");
            WriteResx(first, "Errors.resx", Typed("Count", "1"));
            WriteResx(second, "Errors.resx", Typed("Count", "2"));

            var count = this.MergeAndRead("Errors.resx", first, second).Single(d => Name(d) == "Count");

            Assert.Equal("preserve", (string)count.Attribute(Xml + "space"));
            Assert.Equal("System.Int32, mscorlib", (string)count.Attribute("type"));
            Assert.Equal("2", (string)count.Element("value"));
        }

        [Fact]
        public void DisjointKeysAreUnioned()
        {
            var first = this.Dir("first");
            var second = this.Dir("second");
            WriteResx(first, "Errors.resx", Data("Alpha", "a"));
            WriteResx(second, "Errors.resx", Data("Beta", "b"));

            var names = this.MergeAndRead("Errors.resx", first, second).Select(Name).ToArray();

            Assert.Contains("Alpha", names);
            Assert.Contains("Beta", names);
        }

        [Fact]
        public void LastInputDirectoryWins()
        {
            var first = this.Dir("first");
            var second = this.Dir("second");
            WriteResx(first, "Errors.resx", Data("Greeting", "first"));
            WriteResx(second, "Errors.resx", Data("Greeting", "second"));

            var greeting = this.MergeAndRead("Errors.resx", first, second).Single(d => Name(d) == "Greeting");

            Assert.Equal("second", (string)greeting.Element("value"));
        }

        [Fact]
        public void DifferentFilenamesStaySeparate()
        {
            var first = this.Dir("first");
            var second = this.Dir("second");
            WriteResx(first, "Alpha.resx", Data("A", "a"));
            WriteResx(second, "Beta.resx", Data("B", "b"));

            var output = this.Output(first, second);

            var alpha = ReadData(output, "Alpha.resx").Select(Name).ToArray();
            var beta = ReadData(output, "Beta.resx").Select(Name).ToArray();

            Assert.Equal(new[] { "A" }, alpha);
            Assert.Equal(new[] { "B" }, beta);
        }

        [Fact]
        public void NonExistentInputDirectoryIsSkipped()
        {
            var first = this.Dir("first");
            WriteResx(first, "Errors.resx", Data("Greeting", "Hello"));
            var missing = new DirectoryInfo(Path.Combine(this.root, "does-not-exist"));

            var merger = new Merger();
            var exception = Record.Exception(() => merger.Input(new[] { missing, new DirectoryInfo(first) }));

            Assert.Null(exception);

            var output = this.OutputOf(merger);
            Assert.Equal(new[] { "Greeting" }, ReadData(output, "Errors.resx").Select(Name).ToArray());
        }

        [Fact]
        public void NonResxFilesAreIgnored()
        {
            var first = this.Dir("first");
            WriteResx(first, "Errors.resx", Data("Greeting", "Hello"));
            File.WriteAllText(Path.Combine(first, "notes.txt"), "not a resource file");

            var output = this.Output(first);

            Assert.True(File.Exists(Path.Combine(output, "Errors.resx")));
            Assert.False(File.Exists(Path.Combine(output, "notes.txt")));
        }

        [Fact]
        public void UppercaseResxExtensionIsMatched()
        {
            var first = this.Dir("first");
            WriteResx(first, "Errors.RESX", Data("Greeting", "Hello"));

            var output = this.Output(first);

            Assert.Equal(new[] { "Greeting" }, ReadData(output, "Errors.RESX").Select(Name).ToArray());
        }

        [Fact]
        public void SchemaAndResHeaderSurviveMerge()
        {
            var first = this.Dir("first");
            var second = this.Dir("second");
            WriteResx(
                first,
                "Errors.resx",
                Schema(),
                ResHeader("version", "2.0"),
                ResHeader("reader", "System.Resources.ResXResourceReader"),
                Data("Greeting", "Hello"));
            WriteResx(second, "Errors.resx", Data("Greeting", "Howdy"));

            var rootElement = this.MergeAndReadRoot("Errors.resx", first, second);

            Assert.Contains(rootElement.Elements(), e => e.Name.LocalName == "schema");
            Assert.Equal(2, rootElement.Elements("resheader").Count());
            Assert.Equal("Howdy", (string)rootElement.Elements("data").Single().Element("value"));
        }

        private static XElement Data(string name, string value, string comment = null)
        {
            var data = new XElement(
                "data",
                new XAttribute("name", name),
                new XAttribute(Xml + "space", "preserve"),
                new XElement("value", value));

            if (comment != null)
            {
                data.Add(new XElement("comment", comment));
            }

            return data;
        }

        private static XElement Typed(string name, string value) =>
            new XElement(
                "data",
                new XAttribute("name", name),
                new XAttribute("type", "System.Int32, mscorlib"),
                new XAttribute(Xml + "space", "preserve"),
                new XElement("value", value));

        private static XElement Schema() =>
            new XElement(
                Xsd + "schema",
                new XAttribute("id", "root"),
                new XElement(Xsd + "element", new XAttribute("name", "root")));

        private static XElement ResHeader(string name, string value) =>
            new XElement("resheader", new XAttribute("name", name), new XElement("value", value));

        private static string Name(XElement data) => (string)data.Attribute("name");

        private static void WriteResx(string directory, string fileName, params XElement[] children)
        {
            var document = new XDocument(new XElement("root", children));
            document.Save(Path.Combine(directory, fileName));
        }

        private string Dir(string name)
        {
            var directory = Path.Combine(this.root, name);
            Directory.CreateDirectory(directory);
            return directory;
        }

        private string Output(params string[] inputDirectories)
        {
            var merger = new Merger();
            merger.Input(inputDirectories.Select(d => new DirectoryInfo(d)).ToArray());
            return this.OutputOf(merger);
        }

        private string OutputOf(Merger merger)
        {
            var output = this.Dir("output");
            merger.Output(new DirectoryInfo(output));
            return output;
        }

        private static XElement[] ReadData(string outputDirectory, string fileName) =>
            XDocument.Load(Path.Combine(outputDirectory, fileName)).Root.Elements("data").ToArray();

        private XElement[] MergeAndRead(string fileName, params string[] inputDirectories) =>
            ReadData(this.Output(inputDirectories), fileName);

        private XElement MergeAndReadRoot(string fileName, params string[] inputDirectories) =>
            XDocument.Load(Path.Combine(this.Output(inputDirectories), fileName)).Root;
    }
}
