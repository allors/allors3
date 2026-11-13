// <copyright file="SerializationTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;
    using Meta;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;
    using C3 = Domain.C3;
    using C4 = Domain.C4;
    using DateTime = System.DateTime;
    using S1234 = Domain.S1234;

    public abstract class SerializationTest : IDisposable
    {
        protected static readonly bool[] TrueFalse = { true, false };
        private static readonly string GuidString = Guid.NewGuid().ToString();

        // The forms in which the XML of a saved population can reach Load. Every form holds the same population.
        private static readonly (string Name, Action<XmlDocument, IMetaPopulation> Change)[] Forms =
        {
            ("as saved", null),
            ("self-closing elements", SelfClosingElements),
            ("self-closing object types without objects", (document, meta) => ObjectTypesWithoutObjects(document, meta, true)),
            ("object types without objects", (document, meta) => ObjectTypesWithoutObjects(document, meta, false)),
            ("self-closing relation types without relations", (document, meta) => RelationTypesWithoutRelations(document, meta, true)),
            ("relation types without relations", (document, meta) => RelationTypesWithoutRelations(document, meta, false)),
            ("comments between elements", CommentsBetweenElements),
            ("comments inside values", CommentsInsideValues),
            ("type ids with hyphens", (document, meta) => TypeIds(document, "D", false)),
            ("type ids in upper case", (document, meta) => TypeIds(document, "N", true)),
            ("values in CDATA sections", ValuesInCDataSections),
            ("decimals without the zeros of their scale", DecimalsWithoutZeros),
            (
                "all forms together",
                (document, meta) =>
                {
                    ObjectTypesWithoutObjects(document, meta, true);
                    RelationTypesWithoutRelations(document, meta, true);
                    ValuesInCDataSections(document, meta);
                    SelfClosingElements(document, meta);
                    TypeIds(document, "D", true);
                    CommentsBetweenElements(document, meta);
                }
            ),
        };

        #region population
        private C1 c1A;
        private C1 c1B;
        private C1 c1C;
        private C1 c1D;
        private C1 c1Empty;
        private C2 c2A;
        private C2 c2B;
        private C2 c2C;
        private C2 c2D;
        private C3 c3A;
        private C3 c3B;
        private C3 c3C;
        private C3 c3D;
        private C4 c4A;
        private C4 c4B;
        private C4 c4C;
        private C4 c4D;
        #endregion

        protected virtual bool EmptyStringIsNull => false;

        protected abstract IProfile Profile { get; }

        protected IDatabase Population => this.Profile.Database;

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Markers => this.Profile.Markers;

        protected Action[] Inits => this.Profile.Inits;

        public abstract void Dispose();

        [Fact]
        public void DifferentVersion()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var otherPopulation = this.CreatePopulation();
                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    this.Populate(otherTransaction);
                    otherTransaction.Commit();

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        otherPopulation.Save(writer);
                    }

                    var xml = stringWriter.ToString();
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xml);
                    var populationElement = (XmlElement)xmlDocument.SelectSingleNode("//population");
                    populationElement.SetAttribute("version", "0");
                    xml = xmlDocument.OuterXml;

                    try
                    {
                        using (var stringReader = new StringReader(xml))
                        {
                            using (var reader = XmlReader.Create(stringReader))
                            {
                                this.Population.Load(reader);
                            }
                        }

                        Assert.True(false); // Fail
                    }
                    catch (ArgumentException)
                    {
                    }

                    populationElement.SetAttribute("version", "2");
                    xml = xmlDocument.OuterXml;

                    try
                    {
                        using (var stringReader = new StringReader(xml))
                        {
                            using (var reader = XmlReader.Create(stringReader))
                            {
                                this.Population.Load(reader);
                            }
                        }

                        Assert.True(false); // Fail
                    }
                    catch (ArgumentException)
                    {
                    }

                    populationElement.SetAttribute("version", "a");
                    xml = xmlDocument.OuterXml;

                    var exception = false;
                    try
                    {
                        using (var stringReader = new StringReader(xml))
                        {
                            using (var reader = XmlReader.Create(stringReader))
                            {
                                this.Population.Load(reader);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        exception = true;
                    }

                    Assert.True(exception);

                    populationElement.SetAttribute("version", string.Empty);
                    xml = xmlDocument.OuterXml;

                    var exceptionThrown = false;
                    try
                    {
                        using (var stringReader = new StringReader(xml))
                        {
                            using (var reader = XmlReader.Create(stringReader))
                            {
                                this.Population.Load(reader);
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        exceptionThrown = true;
                    }
                    catch (InvalidOperationException)
                    {
                        exceptionThrown = true;
                    }

                    Assert.True(exceptionThrown);

                    populationElement.RemoveAttribute("version");
                    xml = xmlDocument.OuterXml;

                    Assert.Throws<ArgumentException>(() => DoLoad(this.Population, xml));
                }
            }
        }

        [Fact]
        public void Load()
        {
            // Every form is loaded, so that one run names every form that an adapter does not load.
            var failures = new List<string>();

            foreach (var indentation in TrueFalse)
            {
                foreach (var init in this.Inits)
                {
                    init();
                    var m = this.Transaction.Database.Context().M;

                    var otherPopulation = this.CreatePopulation();
                    using (var otherTransaction = otherPopulation.CreateTransaction())
                    {
                        this.Populate(otherTransaction);
                        otherTransaction.Commit();

                        var stringWriter = new StringWriter();
                        var xmlWriterSettings = new XmlWriterSettings { Indent = indentation };
                        using (var writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
                        {
                            otherPopulation.Save(writer);
                        }

                        var saved = Digest(otherPopulation);

                        foreach (var form in Forms)
                        {
                            try
                            {
                                var xml = ChangeForm(stringWriter.ToString(), otherPopulation.MetaPopulation, form.Change);
                                var stringReader = new StringReader(xml);
                                using (var reader = XmlReader.Create(stringReader))
                                {
                                    this.Population.Load(reader);
                                }

                                using (var transaction = this.Population.CreateTransaction())
                                {
                                    var x = (C1)transaction.Instantiate(1);
                                    var str = x.C1AllorsString;

                                    this.AssertPopulation(transaction);
                                }

                                var difference = Difference(saved, Digest(this.Population));
                                Assert.True(difference == null, difference);
                            }
                            catch (Exception e)
                            {
                                failures.Add($"{form.Name}, {(indentation ? "indented" : "not indented")}: {Summary(e)}");
                            }
                        }
                    }
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Distinct()));
        }

        [Fact]
        public void RoundTrip()
        {
            foreach (var init in this.Inits)
            {
                var failures = new List<string>();

                // The adapter under test saves; the in-memory adapters and the adapter under test load.
                init();
                PopulateEveryRelationType(this.Population);
                var saved = Digest(this.Population);
                var xml = DoSave(this.Population);

                foreach (var (name, create) in this.Counterparts)
                {
                    CheckLoad($"saved by the adapter under test, loaded by {name}", saved, xml, create(), failures);
                }

                CheckLoad("saved and loaded by the adapter under test", saved, xml, this.Population, failures);

                // The in-memory adapters save; the adapter under test loads.
                foreach (var (name, create) in this.Counterparts)
                {
                    init();
                    var counterpart = create();
                    PopulateEveryRelationType(counterpart);
                    CheckLoad($"saved by {name}, loaded by the adapter under test", Digest(counterpart), DoSave(counterpart), this.Population, failures);
                }

                Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
            }
        }

        [Fact]
        public void LoadVersions()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var otherPopulation = this.CreatePopulation();
                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    // Initial
                    var otherC1 = otherTransaction.Create<C1>();

                    otherTransaction.Commit();

                    var initialObjectVersion = otherC1.Strategy.ObjectVersion;

                    var xml = DoSave(otherPopulation);
                    DoLoad(this.Population, xml);

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var c1 = transaction.Instantiate(otherC1.Id);

                        Assert.Equal(otherC1.Strategy.ObjectVersion, c1.Strategy.ObjectVersion);
                    }

                    // Change
                    otherC1.C1AllorsString = "Changed";

                    otherTransaction.Commit();

                    var changedObjectVersion = otherC1.Strategy.ObjectVersion;

                    xml = DoSave(otherPopulation);
                    DoLoad(this.Population, xml);

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var c1 = transaction.Instantiate(otherC1.Id);

                        Assert.Equal(otherC1.Strategy.ObjectVersion, c1.Strategy.ObjectVersion);
                        Assert.NotEqual(initialObjectVersion, c1.Strategy.ObjectVersion);
                    }

                    // Change again
                    otherC1.C1AllorsString = "Changed again";

                    otherTransaction.Commit();

                    xml = DoSave(otherPopulation);
                    DoLoad(this.Population, xml);

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var c1 = transaction.Instantiate(otherC1.Id);

                        Assert.Equal(otherC1.Strategy.ObjectVersion, c1.Strategy.ObjectVersion);
                        Assert.NotEqual(initialObjectVersion, c1.Strategy.ObjectVersion);
                        Assert.NotEqual(changedObjectVersion, c1.Strategy.ObjectVersion);
                    }
                }
            }
        }

        [Fact]
        public void LoadRollback()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var otherPopulation = this.CreatePopulation();
                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    this.Populate(otherTransaction);
                    otherTransaction.Commit();

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        otherPopulation.Save(writer);
                    }

                    var xml = stringWriter.ToString();
                    // Console.Out.WriteLine(xml);
                    var stringReader = new StringReader(xml);
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        this.Population.Load(reader);
                    }

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        transaction.Rollback();

                        this.AssertPopulation(transaction);
                    }
                }
            }
        }

        [Fact]
        public void LoadDifferentMode()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var population = this.CreatePopulation();
                var transaction = population.CreateTransaction();

                try
                {
                    this.Populate(transaction);
                    transaction.Commit();

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        population.Save(writer);
                    }

                    var stringReader = new StringReader(stringWriter.ToString());
                    var reader = XmlReader.Create(stringReader);

                    try
                    {
                        this.Population.Load(reader);
                        Assert.True(false); // Fail
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }

        [Fact]
        public void LoadDifferentCultureInfos()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var writeCultureInfo = new CultureInfo("en-US");
                var readCultureInfo = new CultureInfo("en-GB");

                CultureInfo.CurrentCulture = writeCultureInfo;
                CultureInfo.CurrentUICulture = writeCultureInfo;

                var loadPopulation = this.CreatePopulation();
                var loadTransaction = loadPopulation.CreateTransaction();
                this.Populate(loadTransaction);

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    loadTransaction.Database.Save(writer);
                }

                CultureInfo.CurrentCulture = readCultureInfo;
                CultureInfo.CurrentUICulture = readCultureInfo;

                var xml = stringWriter.ToString();
                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.AssertPopulation(transaction);
                }

                loadTransaction.Rollback();
            }
        }

        [Fact]
        public void LoadDifferentVersion()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var population = this.CreatePopulation();
                var transaction = population.CreateTransaction();

                try
                {
                    this.Populate(transaction);
                    transaction.Commit();

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        population.Save(writer);
                    }

                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(stringWriter.ToString());
                    var allorsElement = (XmlElement)xmlDocument.SelectSingleNode("/allors");
                    allorsElement.SetAttribute("version", "0.9");

                    var stringReader = new StringReader(xmlDocument.InnerText);
                    var reader = XmlReader.Create(stringReader);

                    try
                    {
                        this.Population.Load(reader);
                        Assert.True(false); // Fail
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    transaction.Commit();
                }
            }
        }

        [Fact]
        public void LoadSpecial()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var savePopulation = this.CreatePopulation();
                var saveTransaction = savePopulation.CreateTransaction();

                try
                {
                    this.c1A = C1.Create(saveTransaction);
                    this.c1A.C1AllorsString = "> <";
                    this.c1A.I12AllorsString = "< >";
                    this.c1A.I1AllorsString = "& &&";
                    this.c1A.S1AllorsString = "' \" ''";

                    this.c1Empty = C1.Create(saveTransaction);

                    saveTransaction.Commit();

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        savePopulation.Save(writer);
                    }

                    // writer = XmlWriter.Create(@"population.xml", Encoding.UTF8);
                    // saveTransaction.Population.Save(writer);
                    // writer.Close();
                    var stringReader = new StringReader(stringWriter.ToString());
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        this.Population.Load(reader);
                    }

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var copyValues = C1.Instantiate(transaction, this.c1A.Strategy.ObjectId);

                        Assert.Equal(this.c1A.C1AllorsString, copyValues.C1AllorsString);
                        Assert.Equal(this.c1A.I12AllorsString, copyValues.I12AllorsString);
                        Assert.Equal(this.c1A.I1AllorsString, copyValues.I1AllorsString);
                        Assert.Equal(this.c1A.S1AllorsString, copyValues.S1AllorsString);

                        var c1EmptyLoaded = C1.Instantiate(transaction, this.c1Empty.Strategy.ObjectId);
                        Assert.NotNull(c1EmptyLoaded);
                    }
                }
                finally
                {
                    saveTransaction.Rollback();
                }
            }
        }

        [Fact]
        public void Save()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.Populate(transaction);

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        this.Population.Save(writer);
                    }

                    //using (var writer = XmlWriter.Create("population.xml"))
                    //{
                    //    this.Population.Save(writer);
                    //    writer.Close();
                    //}

                    var xml = stringWriter.ToString();

                    var stringReader = new StringReader(xml);
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        var savePopulation = this.CreatePopulation();
                        savePopulation.Load(reader);

                        using (var saveTransaction = savePopulation.CreateTransaction())
                        {
                            this.AssertPopulation(saveTransaction);
                        }
                    }
                }
            }
        }

        [Fact]
        public void SaveVersions()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                using (var transaction = this.Population.CreateTransaction())
                {
                    // Initial
                    var c1 = transaction.Create<C1>();

                    transaction.Commit();

                    var initialObjectVersion = c1.Strategy.ObjectVersion;

                    var xml = DoSave(this.Population);

                    var otherPopulation = this.CreatePopulation();
                    DoLoad(otherPopulation, xml);

                    using (var otherTransaction = otherPopulation.CreateTransaction())
                    {
                        var otherC1 = otherTransaction.Instantiate(c1.Id);

                        Assert.Equal(c1.Strategy.ObjectVersion, otherC1.Strategy.ObjectVersion);
                    }

                    // Change
                    c1.C1AllorsString = "Changed";

                    transaction.Commit();

                    var changedObjectVersion = c1.Strategy.ObjectVersion;

                    xml = DoSave(this.Population);

                    otherPopulation = this.CreatePopulation();
                    DoLoad(otherPopulation, xml);

                    using (var otherTransaction = otherPopulation.CreateTransaction())
                    {
                        var otherC1 = otherTransaction.Instantiate(c1.Id);

                        Assert.Equal(c1.Strategy.ObjectVersion, otherC1.Strategy.ObjectVersion);
                        Assert.NotEqual(initialObjectVersion, otherC1.Strategy.ObjectVersion);
                    }

                    // Change again
                    c1.C1AllorsString = "Changed again";

                    transaction.Commit();

                    xml = DoSave(this.Population);

                    otherPopulation = this.CreatePopulation();
                    DoLoad(otherPopulation, xml);

                    using (var otherTransaction = otherPopulation.CreateTransaction())
                    {
                        var otherC1 = otherTransaction.Instantiate(c1.Id);

                        Assert.Equal(c1.Strategy.ObjectVersion, otherC1.Strategy.ObjectVersion);
                        Assert.NotEqual(initialObjectVersion, otherC1.Strategy.ObjectVersion);
                        Assert.NotEqual(changedObjectVersion, otherC1.Strategy.ObjectVersion);
                    }
                }
            }
        }

        [Fact]
        public void SaveDifferentCultureInfos()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var writeCultureInfo = new CultureInfo("en-US");
                var readCultureInfo = new CultureInfo("en-GB");

                CultureInfo.CurrentCulture = writeCultureInfo;
                CultureInfo.CurrentUICulture = writeCultureInfo;

                using (var transaction = this.CreatePopulation().CreateTransaction())
                {
                    this.Populate(transaction);

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        transaction.Database.Save(writer);
                    }

                    CultureInfo.CurrentCulture = readCultureInfo;
                    CultureInfo.CurrentUICulture = readCultureInfo;

                    var stringReader = new StringReader(stringWriter.ToString());
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        var savePopulation = this.CreatePopulation();
                        savePopulation.Load(reader);

                        var saveTransaction = savePopulation.CreateTransaction();

                        this.AssertPopulation(saveTransaction);

                        saveTransaction.Rollback();
                    }
                }
            }
        }

        [Fact]
        public void LoadBinary()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var otherPopulation = this.CreatePopulation();
                var otherTransaction = otherPopulation.CreateTransaction();

                try
                {
                    this.c1A = C1.Create(otherTransaction);
                    this.c1B = C1.Create(otherTransaction);
                    this.c1C = C1.Create(otherTransaction);

                    this.c1A.C1AllorsBinary = Array.Empty<byte>();
                    this.c1B.C1AllorsBinary = new byte[] { 1, 2, 3, 4 };
                    this.c1C.C1AllorsBinary = null;

                    otherTransaction.Commit();

                    var stringWriter = new StringWriter();
                    using (var writer = XmlWriter.Create(stringWriter))
                    {
                        otherPopulation.Save(writer);
                    }

                    var xml = stringWriter.ToString();

                    var stringReader = new StringReader(stringWriter.ToString());
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        this.Population.Load(reader);
                    }

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var c1ACopy = C1.Instantiate(transaction, this.c1A.Strategy.ObjectId);
                        var c1BCopy = C1.Instantiate(transaction, this.c1B.Strategy.ObjectId);
                        var c1CCopy = C1.Instantiate(transaction, this.c1C.Strategy.ObjectId);

                        Assert.Equal(this.c1A.C1AllorsBinary, c1ACopy.C1AllorsBinary);
                        Assert.Equal(this.c1B.C1AllorsBinary, c1BCopy.C1AllorsBinary);
                        Assert.Equal(this.c1C.C1AllorsBinary, c1CCopy.C1AllorsBinary);
                    }
                }
                finally
                {
                    otherTransaction.Commit();
                }
            }
        }

        [Fact]
        public void EnsureObjectId()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var xml =
                    @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">2:1</ot>
      </database>
    </objects>
  </population>
</allors>";
                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.c1A = (C1)transaction.Instantiate(1);
                    this.c2A = (C2)transaction.Instantiate(2);

                    Assert.Equal(2, this.c1A.Strategy.ObjectVersion);
                    Assert.Equal(2, this.c2A.Strategy.ObjectVersion);
                }
            }
        }

        [Fact]
        public void CreateAfterLoad()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                string Xml(params long[] ids)
                {
                    var objectType = ids.Length > 0
                        ? $@"<ot i=""{m.C1.Id:N}"">{string.Join(",", ids.Select(v => v.ToString(CultureInfo.InvariantCulture) + ":0"))}</ot>"
                        : string.Empty;

                    return
$@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        {objectType}
      </database>
    </objects>
  </population>
</allors>";
                }

                long[] Create(int count)
                {
                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var created = transaction.Create<C1>(count);
                        transaction.Commit();

                        return created.Select(v => v.Id).OrderBy(v => v).ToArray();
                    }
                }

                int Count()
                {
                    using (var transaction = this.Population.CreateTransaction())
                    {
                        return this.GetExtent(transaction, m.C1).Length;
                    }
                }

                // New objects are numbered after the highest loaded id, wherever it stands in the file.
                DoLoad(this.Population, Xml(7, 1, 2, 3));
                Assert.Equal(new long[] { 8, 9, 10 }, Create(3));
                Assert.Equal(7, Count());

                // A load starts the numbering again: the objects that it replaces do not count.
                DoLoad(this.Population, Xml(1, 2, 3));
                Assert.Equal(new long[] { 4, 5, 6 }, Create(3));
                Assert.Equal(6, Count());

                // So does a load without objects.
                DoLoad(this.Population, Xml());
                Assert.Equal(new long[] { 1, 2, 3 }, Create(3));
                Assert.Equal(3, Count());
            }
        }

        [Fact]
        public void CantLoadObjects()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0</ot>
        <ot i=""71000000000000000000000000000000"">3:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">2:0</ot>
      </database>
    </objects>
  </population>
</allors>";
                var notLoadedEventArgs = new List<ObjectNotLoadedEventArgs>();
                this.Population.ObjectNotLoaded += (o, args) =>
                    notLoadedEventArgs.Add(args);

                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                Assert.Single(notLoadedEventArgs);
                var notLoadedEventArg = notLoadedEventArgs.First();
                Assert.Equal(3, notLoadedEventArg.ObjectId);
                Assert.Equal(new Guid("71000000000000000000000000000000"), notLoadedEventArg.ObjectTypeId);

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.c1A = (C1)transaction.Instantiate(1);
                    this.c2A = (C2)transaction.Instantiate(2);

                    Assert.NotNull(this.c1A);
                    Assert.NotNull(this.c2A);
                }
            }
        }

        [Fact]
        public void CantLoadUnitRelation()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i=""207138608abd4d718ccc2b4d1b88bce3"">
          <r a=""1"">QSBTdHJpbmc=</r>
        </rtu>
        <rtu i=""40000000000000000000000000000000"">
          <r a=""2"">T29wcw==</r>
        </rtu>
        <rtu i=""b4ee673fbba04e249cda3cf993c79a0a"">
          <r a=""3"">true</r>
        </rtu>
        <rtu i=""cef13620b7d74bfe8d3bc0f826da5989"">
          <r a=""1"">537f6823-d22c-4b3b-ab3c-e15a6b61b9d6</r>
        </rtu>
      </database>
    </relations>
  </population>
</allors>";

                var notLoadedEventArgs = new List<RelationNotLoadedEventArgs>();
                this.Population.RelationNotLoaded += (o, args) =>
                    notLoadedEventArgs.Add(args);

                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                Assert.Single(notLoadedEventArgs);
                var notLoadedEventArg = notLoadedEventArgs.First();
                Assert.Equal(2, notLoadedEventArg.AssociationId);
                Assert.Equal(new Guid("40000000000000000000000000000000"), notLoadedEventArg.RelationTypeId);
                Assert.Equal("T29wcw==", notLoadedEventArg.RoleContents);

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.c1A = (C1)transaction.Instantiate(1);
                    this.c1C = (C1)transaction.Instantiate(3);

                    Assert.Equal("A String", this.c1A.C1AllorsString);
                    Assert.Equal(true, this.c1C.C1AllorsBoolean);
                    Assert.Equal(new Guid("537f6823-d22c-4b3b-ab3c-e15a6b61b9d6"), this.c1A.C1AllorsUnique);
                }
            }
        }

        [Fact]
        public void CantLoadUnitRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i=""207138608abd4d718ccc2b4d1b88bce3"">
            <r a=""1"">QSBTdHJpbmc=</r>
        </rtu>
        <rtu i=""87eb0d1973a74aaeaeed66dc9163233c"">
            <r a=""99"">1.1</r>
        </rtu>
        <rtu i=""b4ee673fbba04e249cda3cf993c79a0a"">
            <r a=""1"">true</r>
        </rtu>
        <rtu i=""cef13620b7d74bfe8d3bc0f826da5989"">
          <r a=""1"">537f6823-d22c-4b3b-ab3c-e15a6b61b9d6</r>
        </rtu>
     </database>
    </relations>
  </population>
</allors>";

                var notLoadedEventArgs = new List<RelationNotLoadedEventArgs>();
                this.Population.RelationNotLoaded += (o, args) =>
                    notLoadedEventArgs.Add(args);

                // The file does not hold the object 99, so its relation stops the load and no handler hears of it.
                Assert.Throws<ArgumentException>(() => DoLoad(this.Population, xml));

                Assert.Empty(notLoadedEventArgs);
            }
        }

        [Fact]
        public void CantLoadCompositeRelation()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtc i=""2ff1c9ba0017466e9f11776086e6d0b0"">
          <r a=""1"">2</r>
        </rtc>
        <rtc i=""30000000000000000000000000000000"">
          <r a=""2"">3</r>
        </rtc>
        <rtc i=""4c77650277d745d9b10162dee27c0c2e"">
          <r a=""3"">4</r>
        </rtc>
        <rtc i=""ab6d11ccec86482888752e9a779ba627"">
          <r a=""1"">4</r>
        </rtc>
    </database>
    </relations>
  </population>
</allors>";

                var notLoadedEventArgs = new List<RelationNotLoadedEventArgs>();
                this.Population.RelationNotLoaded += (o, args) =>
                    notLoadedEventArgs.Add(args);

                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                Assert.Single(notLoadedEventArgs);
                var notLoadedEventArg = notLoadedEventArgs.First();
                Assert.Equal(2, notLoadedEventArg.AssociationId);
                Assert.Equal(new Guid("30000000000000000000000000000000"), notLoadedEventArg.RelationTypeId);
                Assert.Equal("3", notLoadedEventArg.RoleContents);

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.c1A = (C1)transaction.Instantiate(1);
                    this.c1B = (C1)transaction.Instantiate(2);
                    this.c1C = (C1)transaction.Instantiate(3);
                    this.c1D = (C1)transaction.Instantiate(4);

                    Assert.Single(this.c1A.C1C1many2manies);
                    Assert.Contains(this.c1B, this.c1A.C1C1many2manies);
                    Assert.Equal(this.c1D, this.c1C.C1C1one2one);
                    Assert.Single(this.c1A.C1C1one2manies);
                    Assert.Contains(this.c1D, this.c1A.C1C1one2manies);
                }
            }
        }

        [Fact]
        public void CantLoadCompositeRole()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtc i=""2ff1c9ba0017466e9f11776086e6d0b0"">
          <r a=""1"">2</r>
        </rtc>
        <rtc i=""2cd8b843-f1f5-413d-9d6d-0d2b9b3c5cf6"">
          <r a=""99"">3</r>
        </rtc>
        <rtc i=""4c776502-77d7-45d9-b101-62dee27c0c2e"">
          <r a=""3"">4</r>
        </rtc>
        <rtc i=""ab6d11ccec86482888752e9a779ba627"">
          <r a=""1"">4</r>
        </rtc>
    </database>
    </relations>
  </population>
</allors>";

                var notLoadedEventArgs = new List<RelationNotLoadedEventArgs>();
                this.Population.RelationNotLoaded += (o, args) =>
                    notLoadedEventArgs.Add(args);

                // The file does not hold the object 99, so its relation stops the load and no handler hears of it.
                Assert.Throws<ArgumentException>(() => DoLoad(this.Population, xml));

                Assert.Empty(notLoadedEventArgs);
            }
        }

        [Fact]
        public void CantLoadAdjacentRoles()
        {
            foreach (var init in this.Inits)
            {
                init();

                // The relations of an unknown relation type follow each other without whitespace, as Save
                // writes them. Every one of them is reported, and the known relations after them load.
                var xml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i=""30000000000000000000000000000001""><r a=""1"">x</r><r a=""2"">y</r><r a=""3"">z</r><r a=""4""/></rtu>
        <rtc i=""30000000000000000000000000000002""><r a=""1"">2</r><r a=""2"">3,4</r><r a=""3"">4</r><r a=""4""/></rtc>
        <rtc i=""ab6d11ccec86482888752e9a779ba627"">
          <r a=""1"">4</r>
        </rtc>
      </database>
    </relations>
  </population>
</allors>";

                var notLoadedEventArgs = new List<RelationNotLoadedEventArgs>();
                this.Population.RelationNotLoaded += (o, args) => notLoadedEventArgs.Add(args);

                DoLoad(this.Population, xml);

                var unitEvents = notLoadedEventArgs.Where(v => v.RelationTypeId == new Guid("30000000000000000000000000000001")).ToArray();
                Assert.Equal(new long[] { 1, 2, 3, 4 }, unitEvents.Select(v => v.AssociationId).ToArray());
                Assert.Equal(new[] { "x", "y", "z", string.Empty }, unitEvents.Select(v => v.RoleContents).ToArray());

                var compositeEvents = notLoadedEventArgs.Where(v => v.RelationTypeId == new Guid("30000000000000000000000000000002")).ToArray();
                Assert.Equal(new long[] { 1, 2, 2, 3, 4 }, compositeEvents.Select(v => v.AssociationId).ToArray());
                Assert.Equal(new[] { "2", "3", "4", "4", null }, compositeEvents.Select(v => v.RoleContents).ToArray());

                Assert.Equal(unitEvents.Length + compositeEvents.Length, notLoadedEventArgs.Count);

                using (var transaction = this.Population.CreateTransaction())
                {
                    var c1A = (C1)transaction.Instantiate(1);
                    var c1D = (C1)transaction.Instantiate(4);

                    Assert.Single(c1A.C1C1one2manies);
                    Assert.Contains(c1D, c1A.C1C1one2manies);
                }
            }
        }

        [Fact]
        public void SchemaChanges()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // Every change is loaded, so that one run names every change that an adapter does not handle.
                var failures = new List<string>();

                foreach (var schemaChange in SchemaChangesOf(m))
                {
                    // With handlers, what the schema no longer allows is reported and everything else is loaded.
                    try
                    {
                        var notLoaded = new List<string>();
                        ObjectNotLoadedEventHandler objectNotLoaded = (sender, args) => notLoaded.Add(NotLoaded(m, args.ObjectTypeId, args.ObjectId));
                        RelationNotLoadedEventHandler relationNotLoaded = (sender, args) => notLoaded.Add(NotLoaded(m, args.RelationTypeId, args.AssociationId, args.RoleContents));

                        this.Population.ObjectNotLoaded += objectNotLoaded;
                        this.Population.RelationNotLoaded += relationNotLoaded;
                        try
                        {
                            DoLoad(this.Population, schemaChange.File);
                        }
                        finally
                        {
                            this.Population.ObjectNotLoaded -= objectNotLoaded;
                            this.Population.RelationNotLoaded -= relationNotLoaded;
                        }

                        var difference = this.SchemaChangeDifference(schemaChange.Loaded, schemaChange.NotLoaded, notLoaded);
                        if (difference != null)
                        {
                            failures.Add($"{schemaChange.Name}, with handlers: {difference}");
                        }
                    }
                    catch (Exception e)
                    {
                        failures.Add($"{schemaChange.Name}, with handlers: {Summary(e)}");
                    }

                    // Without a handler the load fails.
                    var failed = false;
                    try
                    {
                        DoLoad(this.Population, schemaChange.File);
                    }
                    catch (Exception)
                    {
                        failed = true;
                    }

                    if (!failed)
                    {
                        // The failure says what else the load left out.
                        var difference = Difference(this.ExpectedDigest(schemaChange.Loaded), Digest(this.Population));
                        failures.Add($"{schemaChange.Name}, without a handler: the load did not fail" + (difference != null ? "; " + difference : string.Empty));
                    }
                }

                Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
            }
        }

        [Fact]
        public void DecimalRoleType()
        {
            // A decimal role type has a precision from 1 to 28 and a scale from 0 to its precision, which every
            // adapter holds. Without a precision it has the precision 19, and without a scale the scale 2.
            var cases = new (int? Precision, int? Scale, bool Valid)[]
            {
                (null, null, true),
                (19, 2, true),
                (10, 2, true),
                (1, 0, true),
                (1, 1, true),
                (2, null, true),
                (null, 19, true),
                (3, 3, true),
                (28, 0, true),
                (28, 28, true),
                (19, -1, false),
                (19, -2, false),
                (3, 5, false),
                (1, 2, false),
                (1, null, false),
                (null, 20, false),
                (28, 29, false),
                (29, 0, false),
                (29, 2, false),
                (30, 29, false),
                (38, 2, false),
                (39, 2, false),
                (0, 0, false),
                (0, null, false),
                (-1, -1, false),
            };

            // Every role type is validated, so that one run names every role type that is not handled.
            var failures = new List<string>();

            foreach (var (precision, scale, valid) in cases)
            {
                var m = new MetaBuilder().Build();
                m.C1.C1AllorsDecimal.Precision = precision;
                m.C1.C1AllorsDecimal.Scale = scale;

                // The validation names the role type that it refuses and says what a decimal role type has.
                var expected = valid ? string.Empty : DecimalRoleTypeError("C1C1AllorsDecimal", precision ?? 19, scale ?? 2);
                var name = $"precision {precision?.ToString(CultureInfo.InvariantCulture) ?? "none"}, scale {scale?.ToString(CultureInfo.InvariantCulture) ?? "none"}";

                var validation = string.Join(" | ", m.Validate().Messages);
                if (validation != expected)
                {
                    failures.Add($"{name}: the validation says [{validation}]");
                }

                // Every database is configured with an object factory, which refuses the meta population that the
                // validation refuses, and says why.
                var refusal = string.Empty;
                try
                {
                    _ = new ObjectFactory(m, typeof(C1));
                }
                catch (Exception e)
                {
                    refusal = e.Message;
                }

                if (refusal != expected)
                {
                    failures.Add($"{name}: the object factory says [{refusal}]");
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));

            // The object factory has the error of every role type that is refused, one on a line.
            var several = new MetaBuilder().Build();
            several.C1.C1AllorsDecimal.Precision = 3;
            several.C1.C1AllorsDecimal.Scale = 5;
            several.C2.C2AllorsDecimal.Precision = 0;
            several.C2.C2AllorsDecimal.Scale = 0;

            var errors = several.Validate().Messages;
            Assert.Equal(2, errors.Length);
            Assert.Contains(DecimalRoleTypeError("C1C1AllorsDecimal", 3, 5), errors);
            Assert.Contains(DecimalRoleTypeError("C2C2AllorsDecimal", 0, 0), errors);

            var exception = Assert.Throws<Exception>(() => new ObjectFactory(several, typeof(C1)));
            Assert.Equal(string.Join(Environment.NewLine, errors), exception.Message);

            // Only a decimal has a precision and a scale: for a role type of another type they do not count.
            var other = new MetaBuilder().Build();
            other.C1.C1AllorsString.Precision = 0;
            other.C1.C1AllorsString.Scale = -1;

            Assert.Empty(other.Validate().Messages);
            _ = new ObjectFactory(other, typeof(C1));
        }

        [Fact]
        public void ReadDecimal()
        {
            var m = new MetaBuilder().Build();
            var roleType = m.C1.C1AllorsDecimal;
            var cases = new (int Precision, int Scale, string Value, bool Fits)[]
            {
                (3, 3, "0.999", true),
                (3, 3, "-0.999", true),
                (3, 3, "0.9990", true),
                (3, 3, "0.9999", false),
                (3, 3, "1", false),
                (3, 3, "-1", false),
                (1, 0, "9", true),
                (1, 0, "9.0", true),
                (1, 0, "10", false),
                (1, 0, "0.5", false),
                (1, 1, "0.9", true),
                (1, 1, "1", false),
                (28, 0, new string('9', 28), true),
                (28, 0, "-" + new string('9', 28), true),
                (28, 0, "1" + new string('0', 28), false),
                (28, 0, XmlConvert.ToString(decimal.MaxValue), false),
                (28, 28, "0." + new string('9', 28), true),
                (28, 28, "-0." + new string('9', 28), true),
                (28, 28, "0." + new string('0', 27) + "1", true),
                (28, 28, "1", false),
            };

            // One rule decides for a load and for a role that is set: a decimal that does not fit is not rounded.
            foreach (var (precision, scale, value, fits) in cases)
            {
                roleType.Precision = precision;
                roleType.Scale = scale;
                Assert.True(m.IsValid);

                var @decimal = XmlConvert.ToDecimal(value);

                if (fits)
                {
                    Assert.Equal(@decimal, Serialization.ReadUnit(roleType, value));
                    Assert.Equal(@decimal, roleType.NormalizeRole(@decimal));
                }
                else
                {
                    Assert.Throws<ArgumentException>(() => Serialization.ReadUnit(roleType, value));
                    Assert.Throws<ArgumentException>(() => roleType.NormalizeRole(@decimal));
                }
            }

            // A role holds a decimal that fits with the decimals of the scale, and a zero without a sign. Decimal has
            // room for those decimals, also for the largest decimals of a role type with the precision 28.
            foreach (var (precision, scale, value, expected) in new (int Precision, int Scale, string Value, string Expected)[]
            {
                (19, 2, "1.5", "1.50"),
                (19, 2, "1", "1.00"),
                (19, 2, "-1", "-1.00"),
                (19, 2, "0", "0.00"),
                (19, 2, "-0.0", "0.00"),
                (19, 2, "1.500", "1.50"),
                (19, 0, "1.0", "1"),
                (28, 0, new string('9', 28), new string('9', 28)),
                (28, 2, new string('9', 26), new string('9', 26) + ".00"),
                (28, 27, "9", "9." + new string('0', 27)),
                (28, 28, "0.1", "0.1" + new string('0', 27)),
                (28, 28, "-0.0", "0." + new string('0', 28)),
            })
            {
                roleType.Precision = precision;
                roleType.Scale = scale;
                Assert.True(m.IsValid);

                var role = (decimal)roleType.NormalizeRole(XmlConvert.ToDecimal(value));

                Assert.Equal(expected, role.ToString(CultureInfo.InvariantCulture));
                Assert.False(role == 0 && decimal.IsNegative(role));
            }
        }

        [Fact]
        public void HandlerFailure()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var objects = ObjectsOf(m.C1.Id, 1, 2, 3, 4) + ObjectsOf(m.C2.Id, 5, 6, 7, 8);

                // An object, a unit relation and a composite relation that the schema does not allow.
                var files = new[]
                {
                    PopulationOf(objects + ObjectsOf(new Guid("71000000000000000000000000000000"), 9), string.Empty),
                    PopulationOf(objects, UnitsOf(m.C1.C1AllorsString.RelationType.Id, (5, Base64("x")))),
                    PopulationOf(objects, CompositesOf(m.C1.C1C1one2one.RelationType.Id, (1, "5"))),
                };

                foreach (var file in files)
                {
                    // The exception of a handler stops the load, and it is the exception that the caller gets.
                    var failure = new InvalidOperationException("The handler fails.");
                    ObjectNotLoadedEventHandler objectNotLoaded = (sender, args) => throw failure;
                    RelationNotLoadedEventHandler relationNotLoaded = (sender, args) => throw failure;

                    this.Population.ObjectNotLoaded += objectNotLoaded;
                    this.Population.RelationNotLoaded += relationNotLoaded;
                    try
                    {
                        var exception = Assert.ThrowsAny<Exception>(() => DoLoad(this.Population, file));
                        Assert.Same(failure, exception);
                    }
                    finally
                    {
                        this.Population.ObjectNotLoaded -= objectNotLoaded;
                        this.Population.RelationNotLoaded -= relationNotLoaded;
                    }
                }
            }
        }

        [Fact]
        public void CorruptFiles()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                // Every file is loaded, so that one run names every file that an adapter does not refuse.
                var failures = new List<string>();

                foreach (var (name, file, reported) in CorruptFilesOf(m))
                {
                    // Handlers recover what a schema change explains. A corrupt file stops the load with the
                    // handlers attached as well, and they hear nothing of what is wrong with it: only of what a
                    // schema change explains, where the file holds that before it.
                    var notLoaded = new List<string>();
                    ObjectNotLoadedEventHandler objectNotLoaded = (sender, args) => notLoaded.Add(NotLoaded(m, args.ObjectTypeId, args.ObjectId));
                    RelationNotLoadedEventHandler relationNotLoaded = (sender, args) => notLoaded.Add(NotLoaded(m, args.RelationTypeId, args.AssociationId, args.RoleContents));

                    this.Population.ObjectNotLoaded += objectNotLoaded;
                    this.Population.RelationNotLoaded += relationNotLoaded;

                    var failed = false;
                    try
                    {
                        DoLoad(this.Population, file);
                    }
                    catch (Exception)
                    {
                        failed = true;
                    }
                    finally
                    {
                        this.Population.ObjectNotLoaded -= objectNotLoaded;
                        this.Population.RelationNotLoaded -= relationNotLoaded;
                    }

                    if (!failed)
                    {
                        failures.Add($"{name}: the load did not fail");
                    }

                    if (!notLoaded.SequenceEqual(reported))
                    {
                        failures.Add($"{name}: reported as not loaded: [{string.Join(" | ", notLoaded)}]");
                    }
                }

                Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
            }
        }

        [Fact]
        public void LoadUnitBeforeEmptyElement()
        {
            foreach (var init in this.Inits)
            {
                init();

                long firstId;
                long emptyId;
                long lastId;
                string xml;

                var otherPopulation = this.CreatePopulation();
                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    var first = C1.Create(otherTransaction);
                    var empty = C1.Create(otherTransaction);
                    var last = C1.Create(otherTransaction);

                    first.C1AllorsString = "first";
                    empty.C1AllorsString = string.Empty;
                    last.C1AllorsString = "last";

                    first.C1AllorsBinary = new byte[] { 1 };
                    empty.C1AllorsBinary = Array.Empty<byte>();
                    last.C1AllorsBinary = new byte[] { 3 };

                    otherTransaction.Commit();

                    firstId = first.Id;
                    emptyId = empty.Id;
                    lastId = last.Id;

                    xml = DoSave(otherPopulation);
                }

                // A writer may render an empty value as a self-closing element.
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                foreach (XmlElement relation in xmlDocument.SelectNodes("//" + Serialization.Relation))
                {
                    if (!relation.HasChildNodes)
                    {
                        relation.IsEmpty = true;
                    }
                }

                xml = xmlDocument.OuterXml;
                Assert.Contains("/>", xml);

                DoLoad(this.Population, xml);

                using (var transaction = this.Population.CreateTransaction())
                {
                    var first = (C1)transaction.Instantiate(firstId);
                    var empty = (C1)transaction.Instantiate(emptyId);
                    var last = (C1)transaction.Instantiate(lastId);

                    Assert.Equal("first", first.C1AllorsString);
                    Assert.Equal("last", last.C1AllorsString);
                    if (this.EmptyStringIsNull)
                    {
                        Assert.False(empty.ExistC1AllorsString);
                    }
                    else
                    {
                        Assert.Equal(string.Empty, empty.C1AllorsString);
                    }

                    Assert.Equal(new byte[] { 1 }, first.C1AllorsBinary);
                    Assert.Equal(Array.Empty<byte>(), empty.C1AllorsBinary);
                    Assert.Equal(new byte[] { 3 }, last.C1AllorsBinary);
                }
            }
        }

        protected abstract IDatabase CreatePopulation();

        private static string DoSave(IDatabase otherPopulation)
        {
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                otherPopulation.Save(writer);
            }

            return stringWriter.ToString();
        }

        private static void DoLoad(IDatabase database, string xml)
        {
            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                database.Load(reader);
            }
        }

        // The in-memory adapters that every adapter exchanges populations with.
        private (string Name, Func<IDatabase> Create)[] Counterparts =>
            new (string, Func<IDatabase>)[]
            {
                ("Memory", this.CreatePopulation),
                ("Snapshot", () => ((Profile)this.Profile).CreateSnapshotDatabase()),
            };

        private static void CheckLoad(string exchange, string saved, string xml, IDatabase database, List<string> failures)
        {
            try
            {
                DoLoad(database, xml);

                var difference = Difference(saved, Digest(database));
                if (difference != null)
                {
                    failures.Add($"{exchange}: {difference}");
                }
            }
            catch (Exception e)
            {
                failures.Add($"{exchange}: {Summary(e)}");
            }
        }

        private static string Summary(Exception e)
        {
            var message = e.Message.Replace("\r", string.Empty).Replace("\n", " ");
            return e.GetType().Name + ": " + (message.Length > 400 ? message.Substring(0, 400) + "..." : message);
        }

        // The error of the validation for a decimal role type with a precision or a scale that it refuses.
        private static string DecimalRoleTypeError(string relationType, int precision, int scale) =>
            string.Create(CultureInfo.InvariantCulture, $"RoleType: {relationType} has the precision {precision} and the scale {scale}, but a decimal role type has a precision from 1 to 28 (19 by default) and a scale from 0 to its precision (2 by default)");

        private static string ChangeForm(string xml, IMetaPopulation meta, Action<XmlDocument, IMetaPopulation> change)
        {
            if (change == null)
            {
                return xml;
            }

            // Whitespace is kept, so that an indented file stays indented.
            var document = new XmlDocument { PreserveWhitespace = true };
            document.LoadXml(xml);
            change(document, meta);
            return document.OuterXml;
        }

        private static void SelfClosingElements(XmlDocument document, IMetaPopulation meta)
        {
            foreach (XmlElement element in document.SelectNodes("//*"))
            {
                if (!element.HasChildNodes)
                {
                    element.IsEmpty = true;
                }
            }
        }

        private static void ObjectTypesWithoutObjects(XmlDocument document, IMetaPopulation meta, bool selfClosing)
        {
            var present = TypeIdsIn(document);
            var classes = meta.DatabaseClasses.Where(v => !present.Contains(v.Id)).OrderBy(v => v.Id);
            var database = (XmlElement)document.SelectSingleNode("//" + Serialization.Objects + "/" + Serialization.Database);

            AddWithoutContent(database, Serialization.ObjectType, classes.Select(v => v.Id).Take(3).ToArray(), selfClosing);
        }

        private static void RelationTypesWithoutRelations(XmlDocument document, IMetaPopulation meta, bool selfClosing)
        {
            var present = TypeIdsIn(document);
            var relationTypes = meta.DatabaseRelationTypes.Where(v => !present.Contains(v.Id)).OrderBy(v => v.Id).ToArray();
            var database = (XmlElement)document.SelectSingleNode("//" + Serialization.Relations + "/" + Serialization.Database);

            AddWithoutContent(database, Serialization.RelationTypeUnit, relationTypes.Where(v => v.RoleType.ObjectType.IsUnit).Select(v => v.Id).Take(3).ToArray(), selfClosing);
            AddWithoutContent(database, Serialization.RelationTypeComposite, relationTypes.Where(v => v.RoleType.ObjectType.IsComposite).Select(v => v.Id).Take(3).ToArray(), selfClosing);
        }

        // Adds three elements without content: before the first element, after it and after the last element,
        // with no whitespace between them and the element next to them.
        private static void AddWithoutContent(XmlElement parent, string name, Guid[] ids, bool selfClosing)
        {
            XmlElement Create(Guid id)
            {
                var element = parent.OwnerDocument.CreateElement(name);
                element.SetAttribute(Serialization.Id, id.ToString("N"));
                element.IsEmpty = selfClosing;
                return element;
            }

            var first = parent.ChildNodes.OfType<XmlElement>().First();
            parent.InsertBefore(Create(ids[0]), first);
            parent.InsertAfter(Create(ids[1]), first);
            parent.InsertAfter(Create(ids[2]), parent.ChildNodes.OfType<XmlElement>().Last());
        }

        private static HashSet<Guid> TypeIdsIn(XmlDocument document) =>
            new HashSet<Guid>(document.SelectNodes("//@" + Serialization.Id).Cast<XmlAttribute>().Select(v => Guid.Parse(v.Value)));

        private static void CommentsBetweenElements(XmlDocument document, IMetaPopulation meta)
        {
            foreach (var element in document.SelectNodes("//*").Cast<XmlElement>().ToArray())
            {
                if (element.ParentNode is XmlElement parent)
                {
                    parent.InsertBefore(document.CreateComment(" before "), element);
                    parent.InsertAfter(document.CreateComment(" after "), element);
                }
            }
        }

        private static void CommentsInsideValues(XmlDocument document, IMetaPopulation meta)
        {
            foreach (var text in Values(document))
            {
                if (text.Length > 1)
                {
                    var tail = text.SplitText(text.Length / 2);
                    tail.ParentNode.InsertBefore(document.CreateComment(" inside "), tail);
                }
            }
        }

        private static void ValuesInCDataSections(XmlDocument document, IMetaPopulation meta)
        {
            foreach (var text in Values(document))
            {
                text.ParentNode.ReplaceChild(document.CreateCDataSection(text.Value), text);
            }
        }

        // A decimal as the Memory and Snapshot adapters saved it when a role kept the decimals in which it was set:
        // 1.5 where a role with the scale 2 has 1.50.
        private static void DecimalsWithoutZeros(XmlDocument document, IMetaPopulation meta)
        {
            foreach (XmlElement relationType in document.SelectNodes("//" + Serialization.RelationTypeUnit))
            {
                var roleType = ((IRelationType)meta.FindById(Guid.Parse(relationType.GetAttribute(Serialization.Id)))).RoleType;
                if (((IUnit)roleType.ObjectType).Tag != UnitTags.Decimal)
                {
                    continue;
                }

                foreach (var text in relationType.SelectNodes(Serialization.Relation + "/text()").OfType<XmlText>())
                {
                    if (text.Value.Contains('.'))
                    {
                        text.Value = text.Value.TrimEnd('0').TrimEnd('.');
                    }
                }
            }
        }

        // The text of the object types and of the relations.
        private static XmlText[] Values(XmlDocument document) =>
            document.SelectNodes("//" + Serialization.ObjectType + "/text()|//" + Serialization.Relation + "/text()").OfType<XmlText>().ToArray();

        private static void TypeIds(XmlDocument document, string format, bool upperCase)
        {
            foreach (XmlAttribute attribute in document.SelectNodes("//@" + Serialization.Id))
            {
                var id = Guid.Parse(attribute.Value).ToString(format);
                attribute.Value = upperCase ? id.ToUpperInvariant() : id;
            }
        }

        // A population in which every relation type of the domain occurs for every class that can have it. The unit
        // roles take the values at the limits of their type and of their size, precision and scale.
        private static void PopulateEveryRelationType(IDatabase database)
        {
            // Eight objects of every class take the unit values, the ninth has no unit role at all.
            const int WithUnits = 8;
            const int Count = WithUnits + 1;

            using (var transaction = database.CreateTransaction())
            {
                var meta = database.MetaPopulation;
                var objectsByClass = meta.DatabaseClasses.ToDictionary(v => v, v => transaction.Create(v, Count));

                var relationTypes = meta.DatabaseRelationTypes.OrderBy(v => v.Id).ToArray();
                for (var r = 0; r < relationTypes.Length; r++)
                {
                    var relationType = relationTypes[r];
                    var roleType = relationType.RoleType;
                    var associationClasses = relationType.AssociationType.ObjectType.DatabaseClasses.OrderBy(v => v.Id).ToArray();

                    if (roleType.ObjectType.IsUnit)
                    {
                        var values = UnitValues(roleType);
                        foreach (var associationClass in associationClasses)
                        {
                            var associations = objectsByClass[associationClass];
                            for (var k = 0; k < WithUnits; k++)
                            {
                                associations[k].Strategy.SetUnitRole(roleType, values[(k + r) % values.Length]);
                            }
                        }

                        continue;
                    }

                    var roleClasses = ((IComposite)roleType.ObjectType).DatabaseClasses.OrderBy(v => v.Id).ToArray();
                    for (var i = 0; i < associationClasses.Length; i++)
                    {
                        var associations = objectsByClass[associationClasses[i]];

                        if (relationType.IsOneToOne)
                        {
                            // An association has one role and a role has one association.
                            for (var j = 0; j < roleClasses.Length; j++)
                            {
                                associations[j].Strategy.SetCompositeRole(roleType, objectsByClass[roleClasses[j]][i]);
                            }
                        }
                        else if (relationType.IsManyToOne)
                        {
                            // Every association has a role, so that associations share a role.
                            for (var k = 0; k < associations.Length; k++)
                            {
                                associations[k].Strategy.SetCompositeRole(roleType, objectsByClass[roleClasses[k % roleClasses.Length]][i]);
                            }
                        }
                        else if (relationType.IsOneToMany)
                        {
                            // A role has one association: this association class takes every role whose position is
                            // its own position plus a multiple of the number of association classes.
                            for (var j = 0; j < roleClasses.Length; j++)
                            {
                                var roles = objectsByClass[roleClasses[j]];
                                for (var k = i; k < roles.Length; k += associationClasses.Length)
                                {
                                    associations[j].Strategy.AddCompositesRole(roleType, roles[k]);
                                }
                            }
                        }
                        else
                        {
                            for (var k = 0; k < associations.Length; k++)
                            {
                                var roles = objectsByClass[roleClasses[k % roleClasses.Length]];
                                for (var n = 0; n < 3; n++)
                                {
                                    associations[k].Strategy.AddCompositesRole(roleType, roles[(i + k + n) % roles.Length]);
                                }
                            }
                        }
                    }
                }

                transaction.Commit();
            }
        }

        private static object[] UnitValues(IRoleType roleType)
        {
            var unit = (IUnit)roleType.ObjectType;
            switch (unit.Tag)
            {
                case UnitTags.String:
                    var maximumLength = roleType.Size > -1 ? roleType.Size.Value : 5000;
                    return new object[]
                    {
                        string.Empty,
                        "a",
                        " leading and trailing ",
                        "<>&'\"]]>",
                        "line\r\nfeed\ttab",
                        "é中\U0001F600",
                        new string('é', maximumLength),
                        "NULL",
                    };

                case UnitTags.Integer:
                    return new object[] { 0, 1, -1, int.MinValue, int.MaxValue };

                case UnitTags.Decimal:
                    var step = 1m;
                    for (var i = 0; i < roleType.Scale.Value; i++)
                    {
                        step /= 10m;
                    }

                    var maximum = 1m;
                    for (var i = 0; i < roleType.Precision.Value - roleType.Scale.Value; i++)
                    {
                        maximum *= 10m;
                    }

                    maximum -= step;
                    return new object[] { 0m, 1m, -1m, step, -step, maximum, -maximum, 1 + (5 * step) };

                case UnitTags.Float:
                    return new object[] { 0d, 1.1d, -1.1d, 1d / 3d, Math.PI, double.MaxValue, double.MinValue, 2.2250738585072014E-308 };

                case UnitTags.Boolean:
                    return new object[] { true, false };

                case UnitTags.DateTime:
                    return new object[]
                    {
                        new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc),
                        new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc),
                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        new DateTime(2000, 2, 29, 23, 59, 59, 999, DateTimeKind.Utc),
                        DateTime.MinValue,
                        DateTime.MaxValue,
                    };

                case UnitTags.Unique:
                    return new object[] { Guid.Empty, new Guid("537f6823-d22c-4b3b-ab3c-e15a6b61b9d6"), new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") };

                case UnitTags.Binary:
                    var everyByte = Enumerable.Range(0, 256).Select(v => (byte)v).ToArray();
                    var large = Enumerable.Range(0, 10000).Select(v => (byte)(v % 251)).ToArray();
                    return new object[] { Array.Empty<byte>(), new byte[] { 0 }, new byte[] { 255 }, Fit(everyByte, roleType), Fit(large, roleType) };

                default:
                    throw new ArgumentException("Unknown unit tag: " + unit.Tag);
            }
        }

        private static byte[] Fit(byte[] binary, IRoleType roleType) =>
            roleType.Size > -1 && binary.Length > roleType.Size ? binary.Take(roleType.Size.Value).ToArray() : binary;

        // Everything that a population holds, read through the API: every object with its version and every role.
        private static string Digest(IDatabase database)
        {
            var digest = new StringBuilder();

            using (var transaction = database.CreateTransaction())
            {
                foreach (var @class in database.MetaPopulation.DatabaseClasses.OrderBy(v => v.Id))
                {
                    var roleTypes = @class.DatabaseRoleTypes.OrderBy(v => v.RelationType.Id).ToArray();

                    IObject[] objects = transaction.Extent(@class);
                    foreach (var @object in objects.OrderBy(v => v.Id))
                    {
                        var strategy = @object.Strategy;
                        var name = @class.Name + " " + strategy.ObjectId.ToString(CultureInfo.InvariantCulture);

                        digest.Append(name).Append(" version ").Append(strategy.ObjectVersion.ToString(CultureInfo.InvariantCulture)).Append('\n');

                        foreach (var roleType in roleTypes)
                        {
                            if (!strategy.ExistRole(roleType))
                            {
                                continue;
                            }

                            digest.Append(name).Append(' ').Append(roleType.Name).Append(" = ");

                            if (roleType.ObjectType.IsUnit)
                            {
                                digest.Append(Format(strategy.GetUnitRole(roleType)));
                            }
                            else if (roleType.IsOne)
                            {
                                digest.Append(strategy.GetCompositeRole(roleType).Id.ToString(CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                digest.Append(string.Join(",", strategy.GetCompositesRole<IObject>(roleType).Select(v => v.Id).OrderBy(v => v).Select(v => v.ToString(CultureInfo.InvariantCulture))));
                            }

                            digest.Append('\n');
                        }
                    }
                }
            }

            return digest.ToString();
        }

        // A unit as a caller sees it: two units are the same when nothing that a caller can observe tells them apart,
        // so a decimal is taken by its text, with its decimals, and a date by its ticks and its kind.
        private static string Format(object unit) =>
            unit switch
            {
                string value when value.Length <= 64 => "\"" + string.Concat(value.Select(v => v < 0x20 || v > 0x7e ? "\\u" + ((int)v).ToString("x4", CultureInfo.InvariantCulture) : v.ToString())) + "\"",
                string value => "string of " + value.Length.ToString(CultureInfo.InvariantCulture) + " characters " + Hash(Encoding.Unicode.GetBytes(value)),
                byte[] value when value.Length <= 32 => "0x" + Convert.ToHexString(value),
                byte[] value => "binary of " + value.Length.ToString(CultureInfo.InvariantCulture) + " bytes " + Hash(value),
                decimal value => value.ToString(CultureInfo.InvariantCulture),
                double value => value.ToString("R", CultureInfo.InvariantCulture),
                DateTime value => value.Ticks.ToString(CultureInfo.InvariantCulture) + " ticks " + value.Kind.ToString(),
                Guid value => value.ToString("D"),
                int value => value.ToString(CultureInfo.InvariantCulture),
                bool value => value ? "true" : "false",
                _ => throw new ArgumentException("Unknown unit: " + unit),
            };

        private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes), 0, 8);

        // The lines by which the digest of a loaded population differs from the digest of the saved one, or null.
        private static string Difference(string saved, string loaded)
        {
            if (saved == loaded)
            {
                return null;
            }

            var savedLines = saved.Split('\n');
            var loadedLines = loaded.Split('\n');
            var lost = savedLines.Except(loadedLines).ToArray();
            var added = loadedLines.Except(savedLines).ToArray();

            return $"{lost.Length} of {savedLines.Length} saved lines are not loaded, {added.Length} loaded lines were not saved; "
                   + $"not loaded: [{string.Join(" | ", lost.Take(4))}]; not saved: [{string.Join(" | ", added.Take(4))}]";
        }

        // Files as an earlier schema saved them. Each comes with the population that a load has to load from it, as
        // a file, and with what the load has to report as not loaded.
        private static (string Name, string File, string Loaded, string[] NotLoaded)[] SchemaChangesOf(MetaPopulation m)
        {
            var unknownClass = new Guid("71000000000000000000000000000000");
            var unknownUnitRelationType = new Guid("72000000000000000000000000000000");
            var unknownCompositeRelationType = new Guid("73000000000000000000000000000000");

            static Guid Id(IRoleType roleType) => roleType.RelationType.Id;

            string Relation(IRoleType roleType, long association, string role) => NotLoaded(m, Id(roleType), association, role);

            // Four objects of C1 and four of C2. Both classes implement I12.
            var objects = ObjectsOf(m.C1.Id, 1, 2, 3, 4) + ObjectsOf(m.C2.Id, 5, 6, 7, 8);

            // Relations before and after the ones that the schema no longer allows. They are loaded.
            var before = CompositesOf(Id(m.C1.C1C2one2one), (1, "5"));
            var after = CompositesOf(Id(m.C1.C1C2many2one), (4, "8")) + UnitsOf(Id(m.C1.C1StringEquals), (2, Base64("after")));

            (string Name, string File, string Loaded, string[] NotLoaded) Change(string name, string objectsNotLoaded, string relations, string relationsLoaded, params string[] notLoaded) =>
                (
                    name,
                    PopulationOf(objects + objectsNotLoaded, before + relations + after),
                    PopulationOf(objects, before + relationsLoaded + after),
                    notLoaded
                );

            var x = Base64("x");
            var kept = Base64("kept");
            var unique = "537f6823-d22c-4b3b-ab3c-e15a6b61b9d6";

            // The longest string that the size of the role type allows, and a string that is one character longer.
            var size = m.C1.C1AllorsString.Size.Value;
            var longest = Base64(new string('x', size));
            var tooLong = Base64(new string('x', size + 1));

            // A string with the character U+0000, which is not part of a string value.
            var withNull = Base64("a\0b");

            // The longest binary that the size of the role type allows, and a binary that is one byte longer.
            var binarySize = m.C1.C1BinarySized.Size.Value;
            var longestBinary = Convert.ToBase64String(new byte[binarySize]);
            var tooLongBinary = Convert.ToBase64String(new byte[binarySize + 1]);

            // The decimals with the most digits and the most decimals that the precision and the scale of the role
            // type allow, and decimals with one digit and one decimal more.
            var scale = m.C1.C1AllorsDecimal.Scale.Value;
            var digits = m.C1.C1AllorsDecimal.Precision.Value - scale;
            var mostDigits = new string('9', digits) + "." + new string('9', scale);
            var tooManyDigits = "1" + new string('0', digits);
            var mostDecimals = "1." + new string('0', scale - 1) + "5";
            var tooManyDecimals = "1." + new string('0', scale) + "5";

            return new[]
            {
                Change(
                    "a class that no longer exists",
                    ObjectsOf(unknownClass, 9, 10),
                    UnitsOf(Id(m.I12.I12AllorsString), (9, x), (1, kept))
                    + CompositesOf(Id(m.I12.I12C2one2one), (9, "5"), (2, "6"))
                    + CompositesOf(Id(m.C1.C1I12many2one), (2, "10"), (3, "4"))
                    + CompositesOf(Id(m.C1.C1I12many2manies), (3, "4,10")),
                    UnitsOf(Id(m.I12.I12AllorsString), (1, kept))
                    + CompositesOf(Id(m.I12.I12C2one2one), (2, "6"))
                    + CompositesOf(Id(m.C1.C1I12many2one), (3, "4"))
                    + CompositesOf(Id(m.C1.C1I12many2manies), (3, "4")),
                    NotLoaded(m, unknownClass, 9),
                    NotLoaded(m, unknownClass, 10),
                    Relation(m.I12.I12AllorsString, 9, x),
                    Relation(m.I12.I12C2one2one, 9, "5"),
                    Relation(m.C1.C1I12many2one, 2, "10"),
                    Relation(m.C1.C1I12many2manies, 3, "10")),
                Change(
                    "a class that became an interface",
                    ObjectsOf(m.I1.Id, 9),
                    string.Empty,
                    string.Empty,
                    NotLoaded(m, m.I1.Id, 9)),
                Change(
                    "unit relation types that no longer exist or became composite",
                    string.Empty,
                    UnitsOf(unknownUnitRelationType, (1, "x"), (2, "y")) + UnitsOf(Id(m.C1.C1C1one2one), (1, "x")),
                    string.Empty,
                    NotLoaded(m, unknownUnitRelationType, 1, "x"),
                    NotLoaded(m, unknownUnitRelationType, 2, "y"),
                    Relation(m.C1.C1C1one2one, 1, "x")),
                Change(
                    "composite relation types that no longer exist or became unit",
                    string.Empty,
                    CompositesOf(unknownCompositeRelationType, (1, "2"), (2, "3,4")) + CompositesOf(Id(m.C1.C1AllorsInteger), (1, "2,3")),
                    string.Empty,
                    NotLoaded(m, unknownCompositeRelationType, 1, "2"),
                    NotLoaded(m, unknownCompositeRelationType, 2, "3"),
                    NotLoaded(m, unknownCompositeRelationType, 2, "4"),
                    Relation(m.C1.C1AllorsInteger, 1, "2"),
                    Relation(m.C1.C1AllorsInteger, 1, "3")),
                Change(
                    "a unit role of a class that no longer has it",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsString), (5, x), (1, kept)),
                    UnitsOf(Id(m.C1.C1AllorsString), (1, kept)),
                    Relation(m.C1.C1AllorsString, 5, x)),
                Change(
                    "a role of a class that no longer has it",
                    string.Empty,
                    CompositesOf(Id(m.C1.C1C1one2one), (5, "2"), (3, "4")),
                    CompositesOf(Id(m.C1.C1C1one2one), (3, "4")),
                    Relation(m.C1.C1C1one2one, 5, "2")),
                Change(
                    "several roles of a class that no longer has them",
                    string.Empty,
                    CompositesOf(Id(m.C1.C1C1many2manies), (5, "1,2"), (3, "4")),
                    CompositesOf(Id(m.C1.C1C1many2manies), (3, "4")),
                    Relation(m.C1.C1C1many2manies, 5, "1"),
                    Relation(m.C1.C1C1many2manies, 5, "2")),
                Change(
                    "a role of a class that its role type no longer allows",
                    string.Empty,
                    CompositesOf(Id(m.C1.C1C1one2one), (1, "5"), (3, "4")),
                    CompositesOf(Id(m.C1.C1C1one2one), (3, "4")),
                    Relation(m.C1.C1C1one2one, 1, "5")),
                Change(
                    "one of several roles of a class that its role type no longer allows",
                    string.Empty,
                    CompositesOf(Id(m.C1.C1C1many2manies), (2, "3,5,4")),
                    CompositesOf(Id(m.C1.C1C1many2manies), (2, "3,4")),
                    Relation(m.C1.C1C1many2manies, 2, "5")),
                Change(
                    "several roles where a one to one role type allows one",
                    string.Empty,
                    CompositesOf(Id(m.C1.C1C1one2one), (1, "2,3"), (3, "4")),
                    CompositesOf(Id(m.C1.C1C1one2one), (3, "4")),
                    Relation(m.C1.C1C1one2one, 1, "2"),
                    Relation(m.C1.C1C1one2one, 1, "3")),
                Change(
                    "several roles where a many to one role type allows one",
                    string.Empty,
                    CompositesOf(Id(m.C1.C1C1many2one), (1, "2,3"), (4, "2")),
                    CompositesOf(Id(m.C1.C1C1many2one), (4, "2")),
                    Relation(m.C1.C1C1many2one, 1, "2"),
                    Relation(m.C1.C1C1many2one, 1, "3")),
                Change(
                    "values that their unit type cannot read",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsInteger), (1, "abc"), (2, "7"))
                    + UnitsOf(Id(m.C1.C1AllorsDecimal), (1, "abc"), (2, "1.5"))
                    + UnitsOf(Id(m.C1.C1AllorsDouble), (1, "abc"), (2, "1.5"))
                    + UnitsOf(Id(m.C1.C1AllorsBoolean), (1, "abc"), (2, "true"))
                    + UnitsOf(Id(m.C1.C1AllorsDateTime), (1, "abc"), (2, "2000-01-01T00:00:00Z"))
                    + UnitsOf(Id(m.C1.C1AllorsUnique), (1, "abc"), (2, unique))
                    + UnitsOf(Id(m.C1.C1AllorsString), (1, "!"), (2, kept))
                    + UnitsOf(Id(m.C1.C1AllorsBinary), (1, "!"), (2, "AQID")),
                    UnitsOf(Id(m.C1.C1AllorsInteger), (2, "7"))
                    + UnitsOf(Id(m.C1.C1AllorsDecimal), (2, "1.5"))
                    + UnitsOf(Id(m.C1.C1AllorsDouble), (2, "1.5"))
                    + UnitsOf(Id(m.C1.C1AllorsBoolean), (2, "true"))
                    + UnitsOf(Id(m.C1.C1AllorsDateTime), (2, "2000-01-01T00:00:00Z"))
                    + UnitsOf(Id(m.C1.C1AllorsUnique), (2, unique))
                    + UnitsOf(Id(m.C1.C1AllorsString), (2, kept))
                    + UnitsOf(Id(m.C1.C1AllorsBinary), (2, "AQID")),
                    Relation(m.C1.C1AllorsInteger, 1, "abc"),
                    Relation(m.C1.C1AllorsDecimal, 1, "abc"),
                    Relation(m.C1.C1AllorsDouble, 1, "abc"),
                    Relation(m.C1.C1AllorsBoolean, 1, "abc"),
                    Relation(m.C1.C1AllorsDateTime, 1, "abc"),
                    Relation(m.C1.C1AllorsUnique, 1, "abc"),
                    Relation(m.C1.C1AllorsString, 1, "!"),
                    Relation(m.C1.C1AllorsBinary, 1, "!")),
                Change(
                    "a string that is longer than its role type allows",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsString), (1, tooLong), (2, longest)),
                    UnitsOf(Id(m.C1.C1AllorsString), (2, longest)),
                    Relation(m.C1.C1AllorsString, 1, tooLong)),
                Change(
                    "a string with the character U+0000",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsString), (1, withNull), (2, kept)),
                    UnitsOf(Id(m.C1.C1AllorsString), (2, kept)),
                    Relation(m.C1.C1AllorsString, 1, withNull)),
                Change(
                    "a binary that is longer than its role type allows",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1BinarySized), (1, tooLongBinary), (2, longestBinary)),
                    UnitsOf(Id(m.C1.C1BinarySized), (2, longestBinary)),
                    Relation(m.C1.C1BinarySized, 1, tooLongBinary)),
                Change(
                    "floats that are not a finite number",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsDouble), (1, "NaN"), (2, "1.5"), (3, "INF"), (4, "-INF")),
                    UnitsOf(Id(m.C1.C1AllorsDouble), (2, "1.5")),
                    Relation(m.C1.C1AllorsDouble, 1, "NaN"),
                    Relation(m.C1.C1AllorsDouble, 3, "INF"),
                    Relation(m.C1.C1AllorsDouble, 4, "-INF")),
                Change(
                    "decimals with more digits than their role type allows",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsDecimal), (1, tooManyDigits), (2, mostDigits), (3, "-" + tooManyDigits), (4, "-" + mostDigits)),
                    UnitsOf(Id(m.C1.C1AllorsDecimal), (2, mostDigits), (4, "-" + mostDigits)),
                    Relation(m.C1.C1AllorsDecimal, 1, tooManyDigits),
                    Relation(m.C1.C1AllorsDecimal, 3, "-" + tooManyDigits)),

                // A decimal that is written with more decimals than the scale fits when the extra decimals are zero.
                Change(
                    "decimals with more decimals than their role type allows",
                    string.Empty,
                    UnitsOf(Id(m.C1.C1AllorsDecimal), (1, tooManyDecimals), (2, mostDecimals), (3, "-" + tooManyDecimals), (4, mostDecimals + "0")),
                    UnitsOf(Id(m.C1.C1AllorsDecimal), (2, mostDecimals), (4, mostDecimals + "0")),
                    Relation(m.C1.C1AllorsDecimal, 1, tooManyDecimals),
                    Relation(m.C1.C1AllorsDecimal, 3, "-" + tooManyDecimals)),
            };
        }

        // Files that no save writes and that no schema change explains. A load has to refuse each of them. A file that
        // also holds what a schema change does explain comes with what the load reports before it stops.
        private static (string Name, string File, string[] Reported)[] CorruptFilesOf(MetaPopulation m)
        {
            var unknownClass = new Guid("71000000000000000000000000000000");
            var unknownUnitRelationType = new Guid("72000000000000000000000000000000");
            var unknownCompositeRelationType = new Guid("73000000000000000000000000000000");

            static Guid Id(IRoleType roleType) => roleType.RelationType.Id;

            var c1 = m.C1.Id;
            var c2 = m.C2.Id;
            var unit = Id(m.C1.C1AllorsString);
            var one = Id(m.C1.C1C1many2one);
            var many = Id(m.C1.C1C1many2manies);
            var x = Base64("x");

            // Four objects of C1 and four of C2, with a unit relation and a composite relation.
            var objects = ObjectsOf(c1, 1, 2, 3, 4) + ObjectsOf(c2, 5, 6, 7, 8);
            var file = PopulationOf(objects, UnitsOf(unit, (1, x)) + CompositesOf(one, (1, "2")));

            (string Name, string File, string[] Reported) Corrupt(string name, string corruptFile, params string[] reported) => (name, corruptFile, reported);

            (string Name, string File, string[] Reported) Objects(string name, string objectTypes) => Corrupt(name, PopulationOf(objectTypes, string.Empty));

            (string Name, string File, string[] Reported) Relations(string name, string relationTypes) => Corrupt(name, PopulationOf(objects, relationTypes));

            // Where an element allows it, what is wrong follows an object or a relation that a load accepts, so that
            // the load has started on the element when it meets it.
            return new[]
            {
                Corrupt("a file without a population", @"<?xml version=""1.0"" encoding=""utf-16""?><allors></allors>"),
                Corrupt("a file that ends in the objects", file.Substring(0, file.IndexOf("</ot>", StringComparison.Ordinal))),
                Corrupt("a file that ends in the relations", file.Substring(0, file.IndexOf("</rtc>", StringComparison.Ordinal))),
                Corrupt("workspace objects", file.Replace("<objects>", "<objects><workspace/>")),
                Corrupt("workspace relations", file.Replace("<relations>", "<relations><workspace/>")),
                Corrupt("an unknown element in the population", file.Replace("<relations>", "<x/><relations>")),
                Corrupt("an unknown element in the objects", file.Replace("<objects>", "<objects><x/>")),
                Corrupt("an unknown element in the relations", file.Replace("<relations>", "<relations><x/>")),
                Objects("an unknown element among the object types", ObjectsOf(c1, 1, 2) + "<x/>"),
                Relations("an unknown element among the relation types", UnitsOf(unit, (1, x)) + "<x/>"),
                Relations("an unknown element in a unit relation type", $@"<rtu i=""{unit:N}""><r a=""1"">{x}</r><x/></rtu>"),
                Relations("an unknown element in a composite relation type", $@"<rtc i=""{one:N}""><r a=""1"">2</r><x/></rtc>"),
                Objects("an object type without an id", "<ot>1:0</ot>"),
                Objects("an object type whose id is not an id", @"<ot i=""x"">1:0</ot>"),
                Objects("an object id that is not a number", $@"<ot i=""{c1:N}"">1:0,x:0</ot>"),
                Objects("an object id that is too large", $@"<ot i=""{c1:N}"">1:0,9223372036854775808:0</ot>"),
                Objects("an object with the id 0", ObjectsOf(c1, 1, 0)),
                Objects("an object with a negative id", ObjectsOf(c1, 1, -1)),
                Objects("an object version that is not a number", $@"<ot i=""{c1:N}"">1:0,2:x</ot>"),
                Objects("an empty item among the objects", $@"<ot i=""{c1:N}"">1:0,,2:0</ot>"),
                Objects("an element among the objects", $@"<ot i=""{c1:N}"">1:0<x/></ot>"),
                Objects("an object twice in an object type", ObjectsOf(c1, 1, 2, 1)),
                Objects("an object in two object types", ObjectsOf(c1, 1, 2) + ObjectsOf(c2, 2, 3)),
                Relations("a unit relation type without an id", $@"<rtu><r a=""1"">{x}</r></rtu>"),
                Relations("a composite relation type without an id", @"<rtc><r a=""1"">2</r></rtc>"),
                Relations("a relation type whose id is not an id", $@"<rtu i=""x""><r a=""1"">{x}</r></rtu>"),
                Relations("a unit relation without an association", $@"<rtu i=""{unit:N}""><r a=""1"">{x}</r><r>{x}</r></rtu>"),
                Relations("a unit relation whose association is not a number", $@"<rtu i=""{unit:N}""><r a=""1"">{x}</r><r a=""x"">{x}</r></rtu>"),
                Relations("an element in a unit relation", $@"<rtu i=""{unit:N}""><r a=""1"">{x}</r><r a=""2"">{x}<x/></r></rtu>"),
                Relations("a composite relation without an association", $@"<rtc i=""{one:N}""><r a=""1"">2</r><r>4</r></rtc>"),
                Relations("a composite relation whose association is not a number", $@"<rtc i=""{one:N}""><r a=""1"">2</r><r a=""x"">4</r></rtc>"),
                Relations("an element in a composite relation", $@"<rtc i=""{one:N}""><r a=""1"">2</r><r a=""3"">4<x/></r></rtc>"),
                Relations("a role that is not a number", CompositesOf(one, (1, "2"), (3, "x"))),
                Relations("one of several roles that is not a number", CompositesOf(many, (1, "2"), (3, "4,x"))),
                Relations("an empty item among the roles", CompositesOf(many, (1, "2"), (3, "4,,1"))),
                Relations("a unit relation of an object that the file does not hold", UnitsOf(unit, (1, x), (99, x))),
                Relations("a composite relation of an object that the file does not hold", CompositesOf(one, (1, "2"), (99, "3"))),
                Relations("a role that the file does not hold", CompositesOf(one, (1, "2"), (3, "99"))),
                Relations("one of several roles that the file does not hold", CompositesOf(many, (1, "2"), (3, "4,99"))),
                Relations("a missing role where a one to one role type allows only one", CompositesOf(Id(m.C1.C1C1one2one), (1, "2"), (3, "4,99"))),
                Relations("a missing role where a many to one role type allows only one", CompositesOf(one, (1, "2"), (3, "4,99"))),
                Relations("a missing role of a class that no longer has it", CompositesOf(one, (1, "2"), (5, "99"))),
                Relations("a missing role among several of a class that no longer has them", CompositesOf(many, (1, "2"), (5, "4,99"))),
                Relations("a missing role after a role of a class that the role type no longer allows", CompositesOf(many, (1, "2"), (2, "5,99"))),
                Corrupt(
                    "a missing role of an object of a class that no longer exists",
                    PopulationOf(objects + ObjectsOf(unknownClass, 9), CompositesOf(Id(m.I12.I12C2one2one), (1, "5"), (9, "99"))),
                    NotLoaded(m, unknownClass, 9)),
                Relations("a missing object in a unit relation type that no longer exists", UnitsOf(unknownUnitRelationType, (99, x))),
                Relations("a missing object in a composite relation type that no longer exists", CompositesOf(unknownCompositeRelationType, (99, "2"))),
                Relations("a missing role in a composite relation type that no longer exists", CompositesOf(unknownCompositeRelationType, (1, "2,99"))),
                Corrupt("relations before the objects", $@"<?xml version=""1.0"" encoding=""utf-16""?><allors><population version=""1""><relations><database>{UnitsOf(unit, (1, x))}</database></relations><objects><database>{objects}</database></objects></population></allors>"),
            };
        }

        private static string PopulationOf(string objects, string relations) =>
$@"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>{objects}</database>
    </objects>
    <relations>
      <database>{relations}</database>
    </relations>
  </population>
</allors>";

        private static string ObjectsOf(Guid objectTypeId, params long[] ids) =>
            $@"<ot i=""{objectTypeId:N}"">{string.Join(",", ids.Select(v => v.ToString(CultureInfo.InvariantCulture) + ":0"))}</ot>";

        private static string UnitsOf(Guid relationTypeId, params (long Association, string Value)[] relations) =>
            $@"<rtu i=""{relationTypeId:N}"">{string.Concat(relations.Select(v => $@"<r a=""{v.Association.ToString(CultureInfo.InvariantCulture)}"">{v.Value}</r>"))}</rtu>";

        private static string CompositesOf(Guid relationTypeId, params (long Association, string Roles)[] relations) =>
            $@"<rtc i=""{relationTypeId:N}"">{string.Concat(relations.Select(v => $@"<r a=""{v.Association.ToString(CultureInfo.InvariantCulture)}"">{v.Roles}</r>"))}</rtc>";

        private static string Base64(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

        private static string NotLoaded(IMetaPopulation meta, Guid objectTypeId, long objectId) =>
            $"object {objectId.ToString(CultureInfo.InvariantCulture)} of {TypeName(meta, objectTypeId)}";

        private static string NotLoaded(IMetaPopulation meta, Guid relationTypeId, long associationId, string roleContents) =>
            $"{TypeName(meta, relationTypeId)} of object {associationId.ToString(CultureInfo.InvariantCulture)}: {roleContents}";

        // The name of a type of the schema, or its id when the schema does not have it.
        private static string TypeName(IMetaPopulation meta, Guid id) =>
            meta.FindById(id) switch
            {
                IRelationType relationType => relationType.RoleType.Name,
                IObjectType objectType => objectType.Name,
                _ => id.ToString("N"),
            };

        // What a load with handlers did differently from what a schema change requires, or null: what it reported
        // as not loaded, and the population that it loaded.
        private string SchemaChangeDifference(string loaded, string[] notLoaded, List<string> reported)
        {
            static string Short(string value) => value.Length > 100 ? value.Substring(0, 100) + "..." : value;

            var notReported = notLoaded.ToList();
            foreach (var item in reported)
            {
                notReported.Remove(item);
            }

            var withoutReason = reported.ToList();
            foreach (var item in notLoaded)
            {
                withoutReason.Remove(item);
            }

            var differences = new List<string>();

            if (notReported.Count > 0 || withoutReason.Count > 0)
            {
                differences.Add($"not reported: [{string.Join(" | ", notReported.Select(Short))}]; reported without a reason: [{string.Join(" | ", withoutReason.Select(Short))}]");
            }

            var population = Difference(this.ExpectedDigest(loaded), Digest(this.Population));
            if (population != null)
            {
                differences.Add(population);
            }

            return differences.Count > 0 ? string.Join("; ", differences) : null;
        }

        // The digest of the population that a file holds, as the Memory adapter loads it.
        private string ExpectedDigest(string file)
        {
            var expected = this.CreatePopulation();
            DoLoad(expected, file);

            return Digest(expected);
        }

        private void AssertPopulation(ITransaction transaction)
        {
            var m = transaction.Database.Context().M;

            Assert.Equal(4, this.GetExtent(transaction, m.C1).Length);
            Assert.Equal(4, this.GetExtent(transaction, m.C2).Length);
            Assert.Equal(4, this.GetExtent(transaction, m.C3).Length);
            Assert.Equal(4, this.GetExtent(transaction, m.C4).Length);

            var c1ACopy = C1.Instantiate(transaction, this.c1A.Strategy.ObjectId);
            var c1BCopy = C1.Instantiate(transaction, this.c1B.Strategy.ObjectId);
            var c1CCopy = C1.Instantiate(transaction, this.c1C.Strategy.ObjectId);
            var c1DCopy = C1.Instantiate(transaction, this.c1D.Strategy.ObjectId);
            var c2ACopy = C2.Instantiate(transaction, this.c2A.Strategy.ObjectId);
            var c2BCopy = C2.Instantiate(transaction, this.c2B.Strategy.ObjectId);
            var c2CCopy = C2.Instantiate(transaction, this.c2C.Strategy.ObjectId);
            var c2DCopy = C2.Instantiate(transaction, this.c2D.Strategy.ObjectId);
            var c3ACopy = C3.Instantiate(transaction, this.c3A.Strategy.ObjectId);
            var c3BCopy = C3.Instantiate(transaction, this.c3B.Strategy.ObjectId);
            var c3CCopy = C3.Instantiate(transaction, this.c3C.Strategy.ObjectId);
            var c3DCopy = C3.Instantiate(transaction, this.c3D.Strategy.ObjectId);
            var c4ACopy = C4.Instantiate(transaction, this.c4A.Strategy.ObjectId);
            var c4BCopy = C4.Instantiate(transaction, this.c4B.Strategy.ObjectId);
            var c4CCopy = C4.Instantiate(transaction, this.c4C.Strategy.ObjectId);
            var c4DCopy = C4.Instantiate(transaction, this.c4D.Strategy.ObjectId);

            IObject[] everyC1 = { c1ACopy, c1BCopy, c1CCopy, c1DCopy };
            IObject[] everyC2 = { c2ACopy, c2BCopy, c2CCopy, c2DCopy };
            IObject[] everyC3 = { c3ACopy, c3BCopy, c3CCopy, c3DCopy };
            IObject[] everyC4 = { c4ACopy, c4BCopy, c4CCopy, c4DCopy };
            IObject[] everyObject =
                                    {
                                        c1ACopy, c1BCopy, c1CCopy, c1DCopy, c2ACopy, c2BCopy, c2CCopy, c2DCopy, c3ACopy,
                                        c3BCopy, c3CCopy, c3DCopy, c4ACopy, c4BCopy, c4CCopy, c4DCopy,
                                    };

            foreach (var allorsObject in everyObject)
            {
                Assert.NotNull(allorsObject);
            }

            if (this.EmptyStringIsNull)
            {
                Assert.False(c1ACopy.ExistC1AllorsString);
            }
            else
            {
                Assert.Equal(string.Empty, c1ACopy.C1AllorsString);
            }

            Assert.Equal(-1, c1ACopy.C1AllorsInteger);
            Assert.Equal(1.1m, c1ACopy.C1AllorsDecimal);
            Assert.Equal(1.1d, c1ACopy.C1AllorsDouble);
            Assert.True(c1ACopy.C1AllorsBoolean);
            Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1ACopy.C1AllorsDateTime);
            Assert.Equal(new Guid(GuidString), c1ACopy.C1AllorsUnique);

            Assert.Equal(Array.Empty<byte>(), c1ACopy.C1AllorsBinary);
            Assert.Equal(new byte[] { 0, 1, 2, 3 }, c1BCopy.C1AllorsBinary);
            Assert.Null(c1CCopy.C1AllorsBinary);

            Assert.Equal("c1b", c2ACopy.C1WhereC1C2one2one.C1AllorsString);
            Assert.Equal("c1b", c2ACopy.C1WhereC1C2one2many.C1AllorsString);
            Assert.Equal("c1b", c2BCopy.C1WhereC1C2one2many.C1AllorsString);

            Assert.Equal("c3a", c3ACopy.I34AllorsString);
            Assert.Equal("c4a", c4ACopy.I34AllorsString);

            Assert.Equal(2, c2ACopy.C1sWhereC1C2many2one.Count());
            Assert.Empty(c2BCopy.C1sWhereC1C2many2one);
            Assert.Single(c2ACopy.C1sWhereC1C2many2many);
            Assert.Single(c2BCopy.C1sWhereC1C2many2many);

            foreach (S1234 allorsObject in everyObject)
            {
                Assert.Equal(everyObject.Length, allorsObject.S1234many2manies.Count());
                foreach (S1234 addObject in everyObject)
                {
                    var objects = allorsObject.S1234many2manies.ToArray();
                    Assert.Contains(addObject, objects);
                }
            }
        }

        private void Populate(ITransaction transaction)
        {
            this.c1A = C1.Create(transaction);
            this.c1B = C1.Create(transaction);
            this.c1C = C1.Create(transaction);
            this.c1D = C1.Create(transaction);
            this.c2A = C2.Create(transaction);
            this.c2B = C2.Create(transaction);
            this.c2C = C2.Create(transaction);
            this.c2D = C2.Create(transaction);
            this.c3A = C3.Create(transaction);
            this.c3B = C3.Create(transaction);
            this.c3C = C3.Create(transaction);
            this.c3D = C3.Create(transaction);
            this.c4A = C4.Create(transaction);
            this.c4B = C4.Create(transaction);
            this.c4C = C4.Create(transaction);
            this.c4D = C4.Create(transaction);

            IObject[] allObjects =
                                   {
                                       this.c1A, this.c1B, this.c1C, this.c1D, this.c2A, this.c2B, this.c2C, this.c2D,
                                       this.c3A, this.c3B, this.c3C, this.c3D, this.c4A, this.c4B, this.c4C, this.c4D,
                                   };

            this.c1A.C1AllorsString = string.Empty; // emtpy string
            this.c1A.C1AllorsInteger = -1;
            this.c1A.C1AllorsDecimal = 1.1m;
            this.c1A.C1AllorsDouble = 1.1d;
            this.c1A.C1AllorsBoolean = true;
            this.c1A.C1AllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
            this.c1A.C1AllorsUnique = new Guid(GuidString);
            this.c1A.C1AllorsBinary = Array.Empty<byte>();

            this.c1B.C1AllorsString = "c1b";
            this.c1B.C1AllorsBinary = new byte[] { 0, 1, 2, 3 };
            this.c1B.C1C2one2one = this.c2A;
            this.c1B.C1C2many2one = this.c2A;
            this.c1C.C1C2many2one = this.c2A;
            this.c1B.AddC1C2one2many(this.c2A);
            this.c1B.AddC1C2one2many(this.c2B);
            this.c1B.AddC1C2one2many(this.c2C);
            this.c1B.AddC1C2one2many(this.c2D);
            this.c1B.AddC1C2many2many(this.c2A);
            this.c1B.AddC1C2many2many(this.c2B);
            this.c1B.AddC1C2many2many(this.c2C);
            this.c1B.AddC1C2many2many(this.c2D);

            this.c1C.C1AllorsString = "c1c";
            this.c1C.C1AllorsBinary = null;

            this.c3A.I34AllorsString = "c3a";
            this.c4A.I34AllorsString = "c4a";

            foreach (S1234 allorsObject in allObjects)
            {
                foreach (S1234 addObject in allObjects)
                {
                    allorsObject.AddS1234many2many(addObject);
                }
            }

            transaction.Commit();
        }

        private IObject[] GetExtent(ITransaction transaction, IComposite objectType) => transaction.Extent(objectType);
    }
}
