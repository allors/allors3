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

                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                Assert.Single(notLoadedEventArgs);
                var notLoadedEventArg = notLoadedEventArgs.First();
                Assert.Equal(99, notLoadedEventArg.AssociationId);
                Assert.Equal(new Guid("87eb0d1973a74aaeaeed66dc9163233c"), notLoadedEventArg.RelationTypeId);
                Assert.Equal("1.1", notLoadedEventArg.RoleContents);

                using (var transaction = this.Population.CreateTransaction())
                {
                    this.c1A = (C1)transaction.Instantiate(1);

                    Assert.Equal("A String", this.c1A.C1AllorsString);
                    Assert.Equal(true, this.c1A.C1AllorsBoolean);
                    Assert.Equal(new Guid("537f6823-d22c-4b3b-ab3c-e15a6b61b9d6"), this.c1A.C1AllorsUnique);
                }
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

                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Load(reader);
                }

                Assert.Single(notLoadedEventArgs);
                var notLoadedEventArg = notLoadedEventArgs.First();
                Assert.Equal(99, notLoadedEventArg.AssociationId);
                Assert.Equal(new Guid("2cd8b843-f1f5-413d-9d6d-0d2b9b3c5cf6"), notLoadedEventArg.RelationTypeId);
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

        private static string Format(object unit) =>
            unit switch
            {
                string value when value.Length <= 64 => "\"" + string.Concat(value.Select(v => v < 0x20 || v > 0x7e ? "\\u" + ((int)v).ToString("x4", CultureInfo.InvariantCulture) : v.ToString())) + "\"",
                string value => "string of " + value.Length.ToString(CultureInfo.InvariantCulture) + " characters " + Hash(Encoding.Unicode.GetBytes(value)),
                byte[] value when value.Length <= 32 => "0x" + Convert.ToHexString(value),
                byte[] value => "binary of " + value.Length.ToString(CultureInfo.InvariantCulture) + " bytes " + Hash(value),
                decimal value => value.ToString("G29", CultureInfo.InvariantCulture),
                double value => value.ToString("R", CultureInfo.InvariantCulture),
                DateTime value => value.Ticks.ToString(CultureInfo.InvariantCulture) + " ticks",
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
