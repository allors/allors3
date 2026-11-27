// <copyright file="IndexTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using Meta;
    using Xunit;

    // The adapter ensures the indexes of its schema when it is asked to, at the end of Init and at the end of a
    // load. A transaction creates and drops no index: the first transaction of a database object reads the indexes
    // of the schema, and none opens while one is missing. Every composite relation gets an index, and a unit role
    // gets one when the domain marks it as indexed. The test domain marks C1.C1AllorsInteger, C1.Many2One and
    // Company.IndexedMany2ManyPersons, and not C1.C1AllorsString, C1.C1AllorsDouble, C1.C1C2one2one,
    // C1.C1C2one2manies and C1.C1C2many2manies. The adapter names its tables, columns, keys and indexes by the ids
    // of the model: the facts take the names from the mapping, and TheNamesOfTheSchemaCarryTheIdsOfTheModel
    // writes them out.
    public abstract class IndexTest : IDisposable
    {
        private Database names;

        // The table of C1, and of the other classes and relations, without its schema.
        protected string TableOfC1 => this.Table(this.M.C1);

        protected string UnitColumn => this.Column(this.M.C1.C1AllorsInteger);

        protected string RoleColumn => this.Column(this.M.C1.Many2One);

        protected string RoleIndex => this.Index(this.M.C1, this.M.C1.Many2One);

        protected string StringColumn => this.Column(this.M.C1.C1AllorsString);

        // The name that the adapter gives to an index for C1.C1AllorsString, which is not marked.
        protected string StringIndex => this.Index(this.M.C1, this.M.C1.C1AllorsString);

        protected string DoubleColumn => this.Column(this.M.C1.C1AllorsDouble);

        private string RelationTable => this.Table(this.M.Company.IndexedMany2ManyPersons);

        private MetaPopulation M => (MetaPopulation)this.Names.MetaPopulation;

        // A database object that is asked for its mapping only.
        private Database Names => this.names ??= (Database)this.CreateDatabase();

        public abstract void Dispose();

        protected abstract IDatabase CreateDatabase(MetaPopulation metaPopulation = null);

        protected abstract object ExecuteScalar(string sql);

        // The number of indexes of the schema that lead with the column.
        protected abstract int IndexCount(string table, string column);

        protected abstract bool ExistIndex(string name);

        protected abstract void DropIndex(string table, string name);

        // Drops every index of the schema that is not the primary key of its table.
        protected abstract void DropIndexes();

        protected abstract void WithIndexLock(Action action);

        // Whether a session waits for the lock of the indexes of the schema.
        protected abstract bool IsIndexLockAwaited();

        [Fact]
        public void InitCreatesTheIndexes()
        {
            this.Rebuild();

            Assert.Equal(1, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        // A name carries an id of the model: its sixteen bytes in network byte order, in the base32hex alphabet
        // of RFC 4648 in lower case and without padding. A table has the id of its class or of its relation type,
        // a column the id of its relation type after the end of the relation that it holds, a key the id of its
        // table, and the index on a column of the table of a class the id of that class and the id of the
        // relation type. No name holds a name of the model.
        [Fact]
        public void TheNamesOfTheSchemaCarryTheIdsOfTheModel()
        {
            this.Rebuild();

            // The objects table keeps its name, and its key has a name of its own.
            Assert.True(this.ExistPrimaryKey("_o", "pk__o"));

            // C1 is 7041c691-d896-4628-8f50-1c24f5d03414, and its unit role C1AllorsInteger, which is marked as
            // indexed, f4920d94-8cd0-45b6-be00-f18d377368fd. The column holds the role.
            Assert.True(this.ExistPrimaryKey("c_e10sd4eoip32h3qg3gifbk1k2g", "pk_e10sd4eoip32h3qg3gifbk1k2g"));
            Assert.True(this.ExistColumn("c_e10sd4eoip32h3qg3gifbk1k2g", "r_ui90r54cq12rdfg0u66jesr8vk"));
            Assert.True(this.ExistIndex("idx_e10sd4eoip32h3qg3gifbk1k2g_ui90r54cq12rdfg0u66jesr8vk"));
            Assert.Equal(1, this.IndexCount("c_e10sd4eoip32h3qg3gifbk1k2g", "r_ui90r54cq12rdfg0u66jesr8vk"));

            // The one-to-many role C1.C1C2one2manies, f39739d2-e8fc-406e-be6a-c92acee07686, is a column of the
            // table of C2, 72c07e8a-03f5-4da8-ab37-236333d4f74e, that holds the association: its index has the id
            // of C2.
            Assert.True(this.ExistColumn("c_eb07t2g3ul6qhapn4dhj7l7n9o", "a_uebjjkn8vh06tfjap4lcto3mgo"));
            Assert.True(this.ExistIndex("idx_eb07t2g3ul6qhapn4dhj7l7n9o_uebjjkn8vh06tfjap4lcto3mgo"));
            Assert.Equal(1, this.IndexCount("c_eb07t2g3ul6qhapn4dhj7l7n9o", "a_uebjjkn8vh06tfjap4lcto3mgo"));

            // The many-to-one role Work.FlowLead, 174f146d-789d-4428-a632-1acd3ad93537, which an interface
            // declares, is a column of the table of Job, 31a7013d-b29c-4cd3-b286-d623b58d300b, that holds the role.
            Assert.True(this.ExistColumn("c_66jg2fdijh6d7ck6qohrb39g1c", "r_2t7h8rbojl22h9hi3b6jlm9l6s"));
            Assert.True(this.ExistIndex("idx_66jg2fdijh6d7ck6qohrb39g1c_2t7h8rbojl22h9hi3b6jlm9l6s"));
            Assert.Equal(1, this.IndexCount("c_66jg2fdijh6d7ck6qohrb39g1c", "r_2t7h8rbojl22h9hi3b6jlm9l6s"));

            // The many-to-many role C1.C1C2many2manies, 71abe169-dea4-4834-8d37-34cbcffa6cee, has a table of its
            // own.
            Assert.True(this.ExistPrimaryKey("r_e6lu2qeukh43939n6j5svujcto", "pk_e6lu2qeukh43939n6j5svujcto"));
            Assert.True(this.ExistColumn("r_e6lu2qeukh43939n6j5svujcto", "a"));
            Assert.True(this.ExistColumn("r_e6lu2qeukh43939n6j5svujcto", "r"));
            Assert.True(this.ExistIndex("idx_e6lu2qeukh43939n6j5svujcto_r"));
            Assert.Equal(1, this.IndexCount("r_e6lu2qeukh43939n6j5svujcto", "r"));

            // The mapping gives the same names.
            var m = this.M;
            Assert.Equal("c_e10sd4eoip32h3qg3gifbk1k2g", this.TableOfC1);
            Assert.Equal("r_ui90r54cq12rdfg0u66jesr8vk", this.UnitColumn);
            Assert.Equal("c_eb07t2g3ul6qhapn4dhj7l7n9o", this.Table(m.C2));
            Assert.Equal("a_uebjjkn8vh06tfjap4lcto3mgo", this.Column(m.C1.C1C2one2manies));
            Assert.Equal("r_2t7h8rbojl22h9hi3b6jlm9l6s", this.Column(m.Work.FlowLead));
            Assert.Equal("r_e6lu2qeukh43939n6j5svujcto", this.Table(m.C1.C1C2many2manies));

            // Every table and every column of a relation, whatever the length of the names of the model. Only a
            // one-to-many relation holds its association in the table of a class.
            var mapping = this.Names.Mapping;
            Assert.All(mapping.TableNameForObjectByClass.Values, v => Assert.Matches("^allors[.]c_[0-9a-v]{26}$", v));
            Assert.All(mapping.TableNameForRelationByRelationType.Values, v => Assert.Matches("^allors[.]r_[0-9a-v]{26}$", v));
            Assert.All(mapping.ColumnNameByRelationType, v => Assert.Matches(v.Key.RoleType.IsMany ? "^a_[0-9a-v]{26}$" : "^r_[0-9a-v]{26}$", v.Value));
        }

        // A relation of C1 with itself has its column in the table of C1, whichever end the column holds: a
        // one-to-many relation holds its association there, and a many-to-one relation its role. A relation type
        // that keeps its id and changes between the two finds no column of its own in that table, and no
        // transaction opens. With one name for both ends, the column of the earlier multiplicity would pass, and
        // the adapter would read the one end as the other.
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ARelationTypeWhoseColumnHoldsTheOtherEndRefusesTheTransaction(bool oneToMany)
        {
            this.Rebuild();

            var meta = new MetaBuilder().Build();
            IRoleType roleType = oneToMany ? meta.C1.C1C1one2manies : meta.C1.C1C1many2one;
            ((RelationType)roleType.RelationType).AssignedMultiplicity = oneToMany ? Multiplicity.ManyToOne : Multiplicity.OneToMany;

            var exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase(meta).CreateTransaction());
            Assert.Contains($"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n", exception.Message);
        }

        [Fact]
        public void EnsureIndexesCreatesThemWithoutATransaction()
        {
            var database = this.RebuildWithoutIndexes();

            Assert.Equal(0, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(0, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(0, this.IndexCount(RelationTable, "r"));

            database.EnsureIndexes();

            Assert.Equal(1, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        [Fact]
        public void AMissingIndexRefusesTheTransactionUntilItIsCreated()
        {
            this.Rebuild();
            this.DropIndex(this.TableOfC1, RoleIndex);

            // The transaction creates nothing. Its message names the statement for the index, with the relation
            // type and the class that the index is for, and how to ensure the indexes.
            var database = (Database)this.CreateDatabase();
            var exception = Assert.ThrowsAny<Exception>(() => database.CreateTransaction());
            Assert.Contains($"CREATE INDEX {this.RoleIndex} ON allors.{this.TableOfC1} ({this.RoleColumn}) /* the relation type C1.Many2One in the table of the class C1 */", exception.Message);
            Assert.Contains("EnsureIndexes()", exception.Message);
            Assert.Equal(0, this.IndexCount(this.TableOfC1, RoleColumn));

            // The next transaction is refused as well.
            Assert.ThrowsAny<Exception>(() => database.CreateTransaction());

            // A role name can contain overlapping comment delimiters. Its description must stay inside
            // the comment when the operator copies the suggested statement.
            var meta = new MetaBuilder().Build();
            meta.C1.Many2One.SingularName = "Many/*/To*/*One";
            var renamed = this.CreateDatabase(meta);
            var renamedException = Assert.ThrowsAny<Exception>(() => renamed.CreateTransaction());
            Assert.Contains($"CREATE INDEX {this.RoleIndex} ON allors.{this.TableOfC1} ({this.RoleColumn}) /* the relation type C1.Many/ * /To* / *One in the table of the class C1 */", renamedException.Message);

            // Another database object ensures the indexes, as a deployment does. The refusal is not kept.
            ((Database)this.CreateDatabase()).EnsureIndexes();

            using (database.CreateTransaction())
            {
            }

            Assert.True(this.ExistIndex(RoleIndex));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
        }

        [Fact]
        public void AnIndexOfAnotherNameCounts()
        {
            this.Rebuild();
            this.DropIndex(this.TableOfC1, RoleIndex);
            this.ExecuteScalar($"CREATE INDEX made_by_hand ON allors.{this.TableOfC1} ({this.RoleColumn})");

            var database = (Database)this.CreateDatabase();
            using (database.CreateTransaction())
            {
            }

            database.EnsureIndexes();

            Assert.False(this.ExistIndex(RoleIndex));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AnIndexOfTheAdapterForAUnitRoleThatIsNotMarkedIsDropped(bool initialize)
        {
            this.Rebuild();

            // The name that the adapter gives to an index for C1.C1AllorsString, and an index of somebody else
            // on the same column.
            this.ExecuteScalar($"CREATE INDEX {this.StringIndex} ON allors.{this.TableOfC1} ({this.StringColumn})");
            this.ExecuteScalar($"CREATE INDEX made_by_hand ON allors.{this.TableOfC1} ({this.StringColumn})");

            // A transaction drops nothing.
            var database = (Database)this.CreateDatabase();
            using (database.CreateTransaction())
            {
            }

            Assert.True(this.ExistIndex(this.StringIndex));

            // Both operations drop it. Init does so after it has emptied a valid schema.
            Action ensure = initialize ? database.Init : database.EnsureIndexes;
            ensure();

            Assert.False(this.ExistIndex(this.StringIndex));
            Assert.True(this.ExistIndex("made_by_hand"));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, this.StringColumn));
        }

        [Fact]
        public void InitEmptiesTheTablesWhenAnOptionalIndexCannotBeDropped()
        {
            var database = this.Rebuild();
            var name = this.Index(this.M.C1, this.M.C1.C1AllorsDouble);
            var m = (MetaPopulation)database.MetaPopulation;
            using (var transaction = database.CreateTransaction())
            {
                transaction.Create(m.C1);
                transaction.Commit();
            }

            // A unique constraint owns its index, so the server refuses to drop the index directly.
            this.ExecuteScalar($"ALTER TABLE allors.{this.TableOfC1} ADD CONSTRAINT {name} UNIQUE ({this.DoubleColumn})");
            try
            {
                this.DropIndex(this.TableOfC1, RoleIndex);
                database.Init();

                Assert.True(this.ExistIndex(name));
                Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
                using (var transaction = database.CreateTransaction())
                {
                    Assert.Empty(transaction.Extent(m.C1));
                }
            }
            finally
            {
                this.ExecuteScalar($"ALTER TABLE allors.{this.TableOfC1} DROP CONSTRAINT {name}");
            }
        }

        [Fact]
        public void AnIndexThatCannotAcquireTheSchemaLockStays()
        {
            var indexName = this.StringIndex;
            this.Rebuild();
            this.ExecuteScalar($"CREATE INDEX {indexName} ON allors.{this.TableOfC1} ({this.StringColumn})");

            var database = (Database)this.CreateDatabase();
            Assert.True(database.IsValid);

            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    database.EnsureIndexes();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            var started = false;
            try
            {
                this.WithIndexLock(() =>
                {
                    thread.Start();
                    started = true;
                    Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "Optional index cleanup did not finish while the schema lock was held.");
                    Assert.Null(exception);
                    Assert.True(this.ExistIndex(indexName));
                });
            }
            finally
            {
                // The helper releases the lock before a blocked worker is joined, even when the assertion fails.
                if (started)
                {
                    thread.Join();
                }
            }

            ((Database)this.CreateDatabase()).EnsureIndexes();

            Assert.False(this.ExistIndex(indexName));
        }

        [Fact]
        public void AnIndexThatTakesTheNameOfALeftoverBeforeItsDropStays()
        {
            var indexName = this.StringIndex;
            var database = this.Rebuild();
            this.ExecuteScalar($"CREATE INDEX {indexName} ON allors.{this.TableOfC1} ({this.StringColumn})");

            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    database.EnsureIndexes();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            var started = false;
            try
            {
                this.WithIndexLock(() =>
                {
                    thread.Start();
                    started = true;

                    // The step has read the leftover and waits for the lock to drop it, two seconds at most.
                    // Meanwhile somebody drops that index and gives its name to an index on another column.
                    var until = Environment.TickCount64 + 1500;
                    while (!this.IsIndexLockAwaited() && Environment.TickCount64 < until)
                    {
                        Thread.Sleep(25);
                    }

                    this.DropIndex(this.TableOfC1, indexName);
                    this.ExecuteScalar($"CREATE INDEX {indexName} ON allors.{this.TableOfC1} ({this.DoubleColumn})");
                });
            }
            finally
            {
                // The helper releases the lock before a blocked worker is joined, even when a statement fails.
                if (started)
                {
                    thread.Join();
                }
            }

            // The drop looks again once it holds the lock, and the index of that name is no longer the leftover.
            Assert.Null(exception);
            Assert.True(this.ExistIndex(indexName));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, this.DoubleColumn));
        }

        [Fact]
        public void ACompositeRelationThatIsNotMarkedGetsAnIndex()
        {
            this.Rebuild();
            var m = this.M;

            // C1.C1C2one2one is a column of the table of C1, and its index has the name of the adapter.
            Assert.True(this.ExistIndex(this.Index(m.C1, m.C1.C1C2one2one)));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, this.Column(m.C1.C1C2one2one)));

            // C1.C1C2one2manies is a column of the table of C2, and C1.C1C2many2manies has a table of its own.
            Assert.Equal(1, this.IndexCount(this.Table(m.C2), this.Column(m.C1.C1C2one2manies)));
            Assert.Equal(1, this.IndexCount(this.Table(m.C1.C1C2many2manies), "r"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ANameThatIsTakenRefusesTheTransaction(bool initialize)
        {
            var database = this.Rebuild();
            var m = (MetaPopulation)database.MetaPopulation;
            using (var transaction = database.CreateTransaction())
            {
                transaction.Create(m.C1);
                transaction.Commit();
            }

            this.DropIndex(this.TableOfC1, RoleIndex);

            // The name of the index for C1.Many2One, on another column.
            this.ExecuteScalar($"CREATE INDEX {this.RoleIndex} ON allors.{this.TableOfC1} ({this.DoubleColumn})");

            // The message names the index that is needed and the column of the index that has its name, each
            // with its relation type and its class.
            var exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase().CreateTransaction());
            Assert.Contains(RoleIndex, exception.Message);
            Assert.Contains(RoleColumn, exception.Message);
            Assert.Contains(this.DoubleColumn, exception.Message);
            Assert.Contains("the relation type C1.Many2One in the table of the class C1", exception.Message);
            Assert.Contains("the relation type C1.C1AllorsDouble in the table of the class C1", exception.Message);

            // Both operations report the conflict and invalidate the earlier check on this database object.
            Action ensure = initialize ? database.Init : database.EnsureIndexes;
            exception = Assert.ThrowsAny<Exception>(ensure);
            Assert.Contains(RoleIndex, exception.Message);
            Assert.Contains(RoleColumn, exception.Message);
            Assert.Contains(this.DoubleColumn, exception.Message);
            Assert.ThrowsAny<Exception>(() => database.CreateTransaction());

            // C1 declares this one-to-many role, but C2 stores its association. A column of that name
            // added to C1 by hand has no relation type in C1's table.
            var extraColumn = this.Column(m.C1.C1C2one2manies);
            this.DropIndex(this.TableOfC1, RoleIndex);
            this.ExecuteScalar($"ALTER TABLE allors.{this.TableOfC1} ADD {extraColumn} BIGINT NULL");
            this.ExecuteScalar($"CREATE INDEX {this.RoleIndex} ON allors.{this.TableOfC1} ({extraColumn})");

            exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase().CreateTransaction());
            Assert.Contains($"an index on allors.{this.TableOfC1} ({extraColumn}), which is the table of the class C1.", exception.Message);

            this.DropIndex(this.TableOfC1, RoleIndex);
            this.ExecuteScalar($"ALTER TABLE allors.{this.TableOfC1} DROP COLUMN {extraColumn}");
            ((Database)this.CreateDatabase()).EnsureIndexes();

            using (var transaction = database.CreateTransaction())
            {
                Assert.Single(transaction.Extent(m.C1));
            }

            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
        }

        [Fact]
        public void InitForgetsThatATransactionWasRefused()
        {
            this.Rebuild();
            var m = this.M;

            // The objects table, the table of a class and the table of a relation are missing, and a table lacks a
            // column.
            this.ExecuteScalar("DROP TABLE allors._o");
            this.ExecuteScalar($"DROP TABLE allors.{this.TableOfC1}");
            this.ExecuteScalar($"DROP TABLE allors.{this.RelationTable}");
            this.ExecuteScalar($"ALTER TABLE allors.{this.Table(m.C2)} DROP COLUMN {this.Column(m.C2.C2AllorsString)}");

            // The message says what each table is: it names the class or the relation type.
            var database = this.CreateDatabase();
            var exception = Assert.ThrowsAny<Exception>(() => database.CreateTransaction());
            Assert.Contains("- allors._o: the objects table\n", exception.Message);
            Assert.Contains($"- allors.{this.TableOfC1}: the table of the class C1\n", exception.Message);
            Assert.Contains($"- allors.{this.RelationTable}: the table of the relation type Company.IndexedMany2ManyPersons\n", exception.Message);
            Assert.Contains($"- allors.{this.Table(m.C2)}: the table of the class C2\n", exception.Message);

            database.Init();

            using (database.CreateTransaction())
            {
            }
        }

        [Fact]
        public void InitEnsuresTheIndexesAgain()
        {
            var database = this.Rebuild();
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));

            // Init builds the schema again, and with it tables without indexes.
            this.ExecuteScalar($"DROP TABLE allors.{this.TableOfC1}");
            database.Init();
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));

            // Init of a valid schema only empties its tables. It brings back an index that the schema has lost.
            this.DropIndex(this.TableOfC1, RoleIndex);
            database.Init();
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));

            using (database.CreateTransaction())
            {
            }
        }

        [Fact]
        public void LoadEnsuresTheIndexes()
        {
            var database = this.Rebuild();
            var m = (MetaPopulation)database.MetaPopulation;
            using (var transaction = database.CreateTransaction())
            {
                transaction.Create(m.C1);
                transaction.Commit();
            }

            var saved = new StringWriter();
            using (var writer = XmlWriter.Create(saved))
            {
                database.Save(writer);
            }

            // A schema that the load has to build again.
            this.ExecuteScalar($"DROP TABLE allors.{this.TableOfC1}");

            var loaded = this.CreateDatabase();
            using (var reader = XmlReader.Create(new StringReader(saved.ToString())))
            {
                loaded.Load(reader);
            }

            Assert.Equal(1, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));

            using (var transaction = loaded.CreateTransaction())
            {
                Assert.Single(transaction.Extent(m.C1));
            }
        }

        // A load that fails initializes the database again, which ensures the indexes: a transaction opens.
        [Fact]
        public void ALoadThatFailsLeavesTheIndexes()
        {
            this.Rebuild();

            // A schema that the load has to build again, and a file that is no population.
            this.ExecuteScalar($"DROP TABLE allors.{this.TableOfC1}");

            var database = this.CreateDatabase();
            using (var reader = XmlReader.Create(new StringReader("<allors/>")))
            {
                Assert.Throws<ArgumentException>(() => database.Load(reader));
            }

            Assert.Equal(1, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));

            using (database.CreateTransaction())
            {
            }
        }

        [Fact]
        public void ALoadThatFailsReportsTheInputErrorAndTheRecoveryError()
        {
            var database = this.Rebuild();
            this.DropIndex(this.TableOfC1, RoleIndex);
            this.ExecuteScalar($"CREATE INDEX {this.RoleIndex} ON allors.{this.TableOfC1} ({this.DoubleColumn})");

            // The objects have been loaded when the mismatched closing tag stops the reader.
            var m = (MetaPopulation)database.MetaPopulation;
            var file = $"<population version=\"1\"><objects><database><ot i=\"{m.C1.Id:N}\">1:1</ot></database></objects><relations></population>";
            using (var reader = XmlReader.Create(new StringReader(file)))
            {
                var exception = Assert.Throws<AggregateException>(() => database.Load(reader));
                Assert.Collection(
                    exception.InnerExceptions,
                    input => Assert.IsType<XmlException>(input),
                    recovery => Assert.Contains(RoleIndex, recovery.Message));
            }

            this.DropIndex(this.TableOfC1, RoleIndex);
            database.EnsureIndexes();
            using (var transaction = database.CreateTransaction())
            {
                Assert.Empty(transaction.Extent(m.C1));
                Assert.Equal(1, transaction.Create(m.C1).Id);
            }
        }

        [Fact]
        public void DatabaseObjectsEnsureTheIndexesAtTheSameTime()
        {
            this.RebuildWithoutIndexes();

            var databases = Enumerable.Range(0, 4).Select(v => (Database)this.CreateDatabase()).ToArray();

            // Validated beforehand and at the same time, so that the pool holds a connection for every object:
            // they then read the indexes of the schema at the same moment and find them all missing.
            Assert.Empty(AtTheSameTime(databases, database => Assert.True(database.IsValid)));

            Assert.Empty(AtTheSameTime(databases, database => database.EnsureIndexes()));

            Assert.Equal(1, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        // The exceptions of an action that every database object starts at the same moment, on a thread of its own.
        private static Exception[] AtTheSameTime(Database[] databases, Action<Database> action)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            using (var barrier = new Barrier(databases.Length))
            {
                var threads = databases
                    .Select(database => new Thread(() =>
                    {
                        try
                        {
                            barrier.SignalAndWait();
                            action(database);
                        }
                        catch (Exception e)
                        {
                            exceptions.Enqueue(e);
                        }
                    }))
                    .ToArray();

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }

            return exceptions.ToArray();
        }

        // The table of a class or of a relation as the mapping names it, without its schema.
        protected string Table(IClass @class) => WithoutSchema(this.Names.Mapping.TableNameForObjectByClass[@class]);

        protected string Table(IRoleType roleType) => WithoutSchema(this.Names.Mapping.TableNameForRelationByRelationType[roleType.RelationType]);

        // The column of a relation in the table of a class.
        protected string Column(IRoleType roleType) => this.Names.Mapping.ColumnNameByRelationType[roleType.RelationType];

        // The name that the adapter gives to the index on the column of a relation in the table of a class.
        protected string Index(IClass @class, IRoleType roleType) => $"idx_{ModelId.Encode(@class.Id)}_{ModelId.Encode(roleType.RelationType.Id)}";

        private static string WithoutSchema(string name) => name.Substring(name.IndexOf('.') + 1);

        // Read from the views of information_schema that both servers have.
        private bool ExistColumn(string table, string column) => Convert.ToInt32(this.ExecuteScalar($"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = 'allors' AND table_name = '{table}' AND column_name = '{column}'"), CultureInfo.InvariantCulture) == 1;

        private bool ExistPrimaryKey(string table, string name) => Convert.ToInt32(this.ExecuteScalar($"SELECT COUNT(*) FROM information_schema.table_constraints WHERE table_schema = 'allors' AND table_name = '{table}' AND constraint_name = '{name}' AND constraint_type = 'PRIMARY KEY'"), CultureInfo.InvariantCulture) == 1;

        // A schema that Init has just built: its tables are new, and have the indexes of the adapter and no other.
        protected Database Rebuild()
        {
            this.ExecuteScalar($"DROP TABLE IF EXISTS allors.{this.TableOfC1}");
            var database = (Database)this.CreateDatabase();
            database.Init();
            return database;
        }

        // A schema that Init has just built, whose indexes were then dropped behind the adapter.
        private Database RebuildWithoutIndexes()
        {
            var database = this.Rebuild();
            this.DropIndexes();
            return database;
        }
    }
}
