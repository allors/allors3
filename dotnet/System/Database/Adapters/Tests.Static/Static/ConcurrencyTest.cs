// <copyright file="ConcurrencyTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections.Concurrent;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using Domain;
    using Xunit;

    /// <summary>
    /// The behaviour of a database that several transactions share, the way a server uses it: one
    /// transaction per request, on any thread. The Memory adapter has one transaction and does not
    /// take part.
    /// </summary>
    public abstract class ConcurrencyTest : IDisposable
    {
        private const int WriterCount = 8;
        private const int ObjectsPerWriter = 10;

        protected abstract IProfile Profile { get; }

        protected IDatabase Population => this.Profile.Database;

        protected ITransaction Transaction => this.Profile.Transaction;

        protected Action[] Inits => this.Profile.Inits;

        public abstract void Dispose();

        [Fact]
        public void UncommittedChangesAreInvisible()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var reader = this.Transaction;
                var changed = C1.Create(reader);
                changed.C1AllorsString = "before";
                var deleted = C1.Create(reader);
                deleted.C1AllorsString = "deleted";
                reader.Commit();

                // The reader takes its view of the database before the writer changes anything.
                Assert.Equal(2, reader.Extent(m.C1).Count);

                using (var writer = this.Population.CreateTransaction())
                {
                    var created = C1.Create(writer);
                    created.C1AllorsString = "created";
                    ((C1)writer.Instantiate(changed)).C1AllorsString = "after";
                    writer.Instantiate(deleted).Strategy.Delete();

                    Assert.Null(reader.Instantiate(created.Id));
                    Assert.Equal("before", changed.C1AllorsString);
                    Assert.Equal("deleted", deleted.C1AllorsString);
                    Assert.Equal(2, reader.Extent(m.C1).Count);

                    writer.Commit();

                    reader.Rollback();

                    Assert.Equal("created", ((C1)reader.Instantiate(created.Id)).C1AllorsString);
                    Assert.Equal("after", changed.C1AllorsString);
                    Assert.Null(reader.Instantiate(deleted.Id));
                    Assert.Equal(2, reader.Extent(m.C1).Count);
                }
            }
        }

        [Fact]
        public void ReadsAreStableUntilTheReaderCommitsOrRollsBack()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var reader = this.Transaction;
                var c1 = C1.Create(reader);
                c1.C1AllorsString = "v1";
                reader.Commit();

                // The reader takes its view of the database now.
                Assert.Single(reader.Extent(m.C1));
                Assert.Equal("v1", c1.C1AllorsString);
                var version = c1.Strategy.ObjectVersion;

                using (var writer = this.Population.CreateTransaction())
                {
                    ((C1)writer.Instantiate(c1)).C1AllorsString = "v2";
                    writer.Commit();
                }

                Assert.Equal("v1", c1.C1AllorsString);
                Assert.Equal(version, c1.Strategy.ObjectVersion);

                var v2 = reader.Extent(m.C1);
                v2.Filter.AddEquals(m.C1.C1AllorsString, "v2");
                Assert.Equal(0, v2.Count);

                reader.Rollback();

                Assert.Equal("v2", c1.C1AllorsString);
                Assert.Equal(version + 1, c1.Strategy.ObjectVersion);
            }
        }

        [Fact]
        public void TheSecondWriterOfTheSameObjectFails()
        {
            foreach (var init in this.Inits)
            {
                init();

                var c1 = C1.Create(this.Transaction);
                c1.C1AllorsString = "v0";
                this.Transaction.Commit();
                var version0 = c1.Strategy.ObjectVersion;

                using (var first = this.Population.CreateTransaction())
                using (var second = this.Population.CreateTransaction())
                {
                    // Both transactions read the object before either of them commits.
                    var c1First = (C1)first.Instantiate(c1);
                    var c1Second = (C1)second.Instantiate(c1);
                    Assert.Equal("v0", c1First.C1AllorsString);
                    Assert.Equal("v0", c1Second.C1AllorsString);

                    c1First.C1AllorsString = "first";
                    c1Second.C1AllorsString = "second";

                    first.Commit();
                    Assert.Equal(version0 + 1, c1First.Strategy.ObjectVersion);

                    // The database reports the conflict the way a database server does.
                    Assert.ThrowsAny<DbException>(() => second.Commit());

                    // After a rollback the second transaction sees the first one's work and can redo its own.
                    second.Rollback();
                    Assert.Equal("first", c1Second.C1AllorsString);
                    c1Second.C1AllorsString = "second";
                    second.Commit();
                    Assert.Equal(version0 + 2, c1Second.Strategy.ObjectVersion);
                }

                this.Transaction.Rollback();

                Assert.Equal("second", c1.C1AllorsString);
                Assert.Equal(version0 + 2, c1.Strategy.ObjectVersion);
            }
        }

        [Fact]
        public void ChangingAnObjectAnotherTransactionDeletedFails()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var deleted = C1.Create(this.Transaction);
                deleted.C1AllorsString = "v0";
                var other = C1.Create(this.Transaction);
                this.Transaction.Commit();

                using (var changer = this.Population.CreateTransaction())
                {
                    var deletedInChanger = (C1)changer.Instantiate(deleted);
                    Assert.Equal("v0", deletedInChanger.C1AllorsString);
                    deletedInChanger.C1AllorsString = "changed";
                    ((C1)changer.Instantiate(other)).C1C1one2one = deletedInChanger;

                    using (var deleter = this.Population.CreateTransaction())
                    {
                        deleter.Instantiate(deleted).Strategy.Delete();
                        deleter.Commit();
                    }

                    Assert.ThrowsAny<DbException>(() => changer.Commit());
                }

                this.Transaction.Rollback();

                Assert.Null(this.Transaction.Instantiate(deleted.Id));
                Assert.Single(this.Transaction.Extent(m.C1));
                Assert.False(other.ExistC1C1one2one);
            }
        }

        [Fact]
        public void DeletingAnObjectAnotherTransactionChangedFails()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var c1 = C1.Create(this.Transaction);
                c1.C1AllorsString = "v0";
                this.Transaction.Commit();

                using (var deleter = this.Population.CreateTransaction())
                using (var changer = this.Population.CreateTransaction())
                {
                    // The deleter reads the object before the changer commits.
                    var c1Deleter = (C1)deleter.Instantiate(c1);
                    Assert.Equal("v0", c1Deleter.C1AllorsString);

                    ((C1)changer.Instantiate(c1)).C1AllorsString = "changed";
                    changer.Commit();

                    // A database server refuses the deletion itself; the in-memory database refuses the commit.
                    Assert.ThrowsAny<DbException>(() =>
                    {
                        c1Deleter.Strategy.Delete();
                        deleter.Commit();
                    });

                    // After a rollback the deleter sees the change and can delete the object after all.
                    deleter.Rollback();
                    Assert.Equal("changed", ((C1)deleter.Instantiate(c1)).C1AllorsString);
                    deleter.Instantiate(c1).Strategy.Delete();
                    deleter.Commit();
                }

                this.Transaction.Rollback();

                Assert.Null(this.Transaction.Instantiate(c1.Id));
                Assert.Equal(0, this.Transaction.Extent(m.C1).Count);
            }
        }

        [Fact]
        public void SaveLeavesOutARelationToAnObjectAnotherTransactionDeleted()
        {
            foreach (var init in this.Inits)
            {
                init();

                var association = C1.Create(this.Transaction);
                var deleted = C1.Create(this.Transaction);
                deleted.C1AllorsString = "deleted";
                var kept = C1.Create(this.Transaction);
                this.Transaction.Commit();

                using (var setter = this.Population.CreateTransaction())
                {
                    // The setter takes its view of the database before the deleter commits.
                    var deletedInSetter = (C1)setter.Instantiate(deleted);
                    Assert.Equal("deleted", deletedInSetter.C1AllorsString);

                    using (var deleter = this.Population.CreateTransaction())
                    {
                        deleter.Instantiate(deleted).Strategy.Delete();
                        deleter.Commit();
                    }

                    var associationInSetter = (C1)setter.Instantiate(association);
                    associationInSetter.C1C1many2one = deletedInSetter;
                    associationInSetter.AddC1C1many2many(deletedInSetter);
                    associationInSetter.AddC1C1many2many((C1)setter.Instantiate(kept));
                    setter.Commit();
                }

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    this.Population.Save(writer);
                }

                // A load refuses a relation to an object that the file does not hold.
                using (var reader = XmlReader.Create(new StringReader(stringWriter.ToString())))
                {
                    this.Population.Load(reader);
                }

                using (var transaction = this.Population.CreateTransaction())
                {
                    Assert.Null(transaction.Instantiate(deleted.Id));

                    var loaded = (C1)transaction.Instantiate(association.Id);
                    Assert.False(loaded.ExistC1C1many2one);
                    Assert.Equal(new[] { kept.Id }, loaded.C1C1many2manies.Select(v => v.Id).ToArray());
                }
            }
        }

        [Fact]
        public void ParallelCommitsOnDistinctObjects()
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                var ids = new ConcurrentBag<long>();
                var exceptions = new ConcurrentBag<Exception>();

                using (var start = new Barrier(WriterCount))
                {
                    var writers = Enumerable.Range(0, WriterCount).Select(writerIndex => new Thread(() =>
                    {
                        try
                        {
                            start.SignalAndWait();

                            using (var transaction = this.Population.CreateTransaction())
                            {
                                var objects = new C1[ObjectsPerWriter];
                                for (var i = 0; i < ObjectsPerWriter; i++)
                                {
                                    var c1 = C1.Create(transaction);
                                    c1.C1AllorsInteger = writerIndex;
                                    c1.C1AllorsString = writerIndex + "-" + i;
                                    objects[i] = c1;
                                }

                                transaction.Commit();

                                foreach (var c1 in objects)
                                {
                                    ids.Add(c1.Id);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            exceptions.Add(e);
                        }
                    })).ToArray();

                    foreach (var writer in writers)
                    {
                        writer.Start();
                    }

                    foreach (var writer in writers)
                    {
                        writer.Join();
                    }
                }

                Assert.Empty(exceptions);
                Assert.Equal(WriterCount * ObjectsPerWriter, ids.Count);
                Assert.Equal(ids.Count, ids.Distinct().Count());

                this.Transaction.Rollback();

                Assert.Equal(WriterCount * ObjectsPerWriter, this.Transaction.Extent(m.C1).Count);

                for (var writerIndex = 0; writerIndex < WriterCount; writerIndex++)
                {
                    var extent = this.Transaction.Extent(m.C1);
                    extent.Filter.AddEquals(m.C1.C1AllorsInteger, writerIndex);
                    var objects = extent.ToArray();

                    Assert.Equal(ObjectsPerWriter, objects.Length);
                    foreach (C1 c1 in objects)
                    {
                        Assert.StartsWith(writerIndex + "-", c1.C1AllorsString, StringComparison.Ordinal);
                    }
                }
            }
        }

        [Fact]
        public void ParallelExtentsWhileCommitting()
        {
            const int readerCount = 4;
            const int commitCount = 20;

            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Context().M;

                for (var i = 0; i < ObjectsPerWriter; i++)
                {
                    C1.Create(this.Transaction).C1AllorsInteger = i;
                }

                this.Transaction.Commit();

                var exceptions = new ConcurrentBag<Exception>();
                var writerCompleted = false;

                var writer = new Thread(() =>
                {
                    try
                    {
                        for (var i = 0; i < commitCount; i++)
                        {
                            using (var transaction = this.Population.CreateTransaction())
                            {
                                C1.Create(transaction).C1AllorsInteger = ObjectsPerWriter + i;

                                var extent = transaction.Extent(m.C1);
                                extent.Filter.AddEquals(m.C1.C1AllorsInteger, i % ObjectsPerWriter);
                                foreach (C1 c1 in extent)
                                {
                                    c1.C1AllorsString = "commit " + i;
                                }

                                transaction.Commit();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                    finally
                    {
                        Volatile.Write(ref writerCompleted, true);
                    }
                });

                var readers = Enumerable.Range(0, readerCount).Select(_ => new Thread(() =>
                {
                    try
                    {
                        do
                        {
                            using (var transaction = this.Population.CreateTransaction())
                            {
                                var extent = transaction.Extent(m.C1);
                                extent.Filter.AddExists(m.C1.C1AllorsInteger);
                                var objects = extent.ToArray();

                                Assert.True(objects.Length >= ObjectsPerWriter);
                                foreach (C1 c1 in objects)
                                {
                                    Assert.True(c1.ExistC1AllorsInteger);
                                }
                            }
                        }
                        while (!Volatile.Read(ref writerCompleted));
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                })).ToArray();

                writer.Start();
                foreach (var reader in readers)
                {
                    reader.Start();
                }

                writer.Join();
                foreach (var reader in readers)
                {
                    reader.Join();
                }

                Assert.Empty(exceptions);

                this.Transaction.Rollback();
                Assert.Equal(ObjectsPerWriter + commitCount, this.Transaction.Extent(m.C1).Count);
            }
        }
    }
}
