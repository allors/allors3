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
    // C1.C1C2one2manies and C1.C1C2many2manies. The adapter names its tables, columns and indexes by the ids of the
    // model: the facts take the names from the mapping, and TheNamesOfTheSchemaCarryTheIdsOfTheModel writes them
    // out.
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

        // The role column of the table of Company.IndexedMany2ManyPersons.
        private string RelationTableRoleColumn => RoleColumnOfRelationTable(this.M.Company.IndexedMany2ManyPersons);

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
            Assert.Equal(1, this.IndexCount(RelationTable, this.RelationTableRoleColumn));
        }

        // A name carries an id of the model: its sixteen bytes in network byte order, in the base32hex alphabet
        // of RFC 4648 in lower case and without padding. The name of a table and of an index starts with allors_.
        // A table has the id of its class, or the id of its relation type and its multiplicity, one letter for
        // each end, the association first. A column of the table of a class has the id of its relation type after
        // the end of the relation that it holds, and for a composite relation the id of the object type of that
        // end and the multiplicity. A column of the table of a relation type has the id of the object type of its
        // end. The index on a column of the table of a class has the id of that class and the id of the relation
        // type. The server names the primary keys. No name holds a name of the model.
        [Fact]
        public void TheNamesOfTheSchemaCarryTheIdsOfTheModel()
        {
            this.Rebuild();

            // The objects table, and its key, which the server names: PostgreSQL after the table, SQL Server after
            // the start of the table and a number.
            Assert.Matches("^(allors_o_pkey|PK__allors_o__[0-9A-F]{16})$", this.PrimaryKey("allors_o"));

            // C1 is 7041c691-d896-4628-8f50-1c24f5d03414, and its unit role C1AllorsInteger, which is marked as
            // indexed, f4920d94-8cd0-45b6-be00-f18d377368fd. The column holds the role.
            Assert.Matches("^(allors_e10sd4eoip32h3qg3gifbk1k2g_pkey|PK__allors_e__[0-9A-F]{16})$", this.PrimaryKey("allors_e10sd4eoip32h3qg3gifbk1k2g"));
            Assert.True(this.ExistColumn("allors_e10sd4eoip32h3qg3gifbk1k2g", "r_ui90r54cq12rdfg0u66jesr8vk"));
            Assert.True(this.ExistIndex("allors_i_e10sd4eoip32h3qg3gifbk1k2g_ui90r54cq12rdfg0u66jesr8vk"));
            Assert.Equal(1, this.IndexCount("allors_e10sd4eoip32h3qg3gifbk1k2g", "r_ui90r54cq12rdfg0u66jesr8vk"));

            // The one-to-many role C1.C1C2one2manies, f39739d2-e8fc-406e-be6a-c92acee07686, is a column of the
            // table of C2, 72c07e8a-03f5-4da8-ab37-236333d4f74e, that holds the association, of C1: its index has
            // the id of C2.
            Assert.True(this.ExistColumn("allors_eb07t2g3ul6qhapn4dhj7l7n9o", "a_uebjjkn8vh06tfjap4lcto3mgo_e10sd4eoip32h3qg3gifbk1k2g_om"));
            Assert.True(this.ExistIndex("allors_i_eb07t2g3ul6qhapn4dhj7l7n9o_uebjjkn8vh06tfjap4lcto3mgo"));
            Assert.Equal(1, this.IndexCount("allors_eb07t2g3ul6qhapn4dhj7l7n9o", "a_uebjjkn8vh06tfjap4lcto3mgo_e10sd4eoip32h3qg3gifbk1k2g_om"));

            // The many-to-one role Work.FlowLead, 174f146d-789d-4428-a632-1acd3ad93537, which an interface
            // declares, is a column of the table of Job, 31a7013d-b29c-4cd3-b286-d623b58d300b, that holds the role,
            // of Job.
            Assert.True(this.ExistColumn("allors_66jg2fdijh6d7ck6qohrb39g1c", "r_2t7h8rbojl22h9hi3b6jlm9l6s_66jg2fdijh6d7ck6qohrb39g1c_mo"));
            Assert.True(this.ExistIndex("allors_i_66jg2fdijh6d7ck6qohrb39g1c_2t7h8rbojl22h9hi3b6jlm9l6s"));
            Assert.Equal(1, this.IndexCount("allors_66jg2fdijh6d7ck6qohrb39g1c", "r_2t7h8rbojl22h9hi3b6jlm9l6s_66jg2fdijh6d7ck6qohrb39g1c_mo"));

            // The many-to-many role C1.C1C2many2manies, 71abe169-dea4-4834-8d37-34cbcffa6cee, has a table of its
            // own, with a column for the association, of C1, and one for the role, of C2.
            Assert.Matches("^(allors_e6lu2qeukh43939n6j5svujcto_mm_pkey|PK__allors_e__[0-9A-F]{16})$", this.PrimaryKey("allors_e6lu2qeukh43939n6j5svujcto_mm"));
            Assert.True(this.ExistColumn("allors_e6lu2qeukh43939n6j5svujcto_mm", "a_e10sd4eoip32h3qg3gifbk1k2g"));
            Assert.True(this.ExistColumn("allors_e6lu2qeukh43939n6j5svujcto_mm", "r_eb07t2g3ul6qhapn4dhj7l7n9o"));
            Assert.True(this.ExistIndex("allors_i_e6lu2qeukh43939n6j5svujcto_r"));
            Assert.Equal(1, this.IndexCount("allors_e6lu2qeukh43939n6j5svujcto_mm", "r_eb07t2g3ul6qhapn4dhj7l7n9o"));

            // The mapping gives the same names.
            var m = this.M;
            Assert.Equal("allors.allors_o", this.Names.Mapping.TableNameForObjects);
            Assert.Equal("allors_e10sd4eoip32h3qg3gifbk1k2g", this.TableOfC1);
            Assert.Equal("r_ui90r54cq12rdfg0u66jesr8vk", this.UnitColumn);
            Assert.Equal("allors_eb07t2g3ul6qhapn4dhj7l7n9o", this.Table(m.C2));
            Assert.Equal("a_uebjjkn8vh06tfjap4lcto3mgo_e10sd4eoip32h3qg3gifbk1k2g_om", this.Column(m.C1.C1C2one2manies));
            Assert.Equal("r_2t7h8rbojl22h9hi3b6jlm9l6s_66jg2fdijh6d7ck6qohrb39g1c_mo", this.Column(m.Work.FlowLead));
            Assert.Equal("allors_e6lu2qeukh43939n6j5svujcto_mm", this.Table(m.C1.C1C2many2manies));

            // Every table and every column of a relation, whatever the length of the names of the model. Only a
            // one-to-many relation holds its association in the table of a class.
            var mapping = this.Names.Mapping;
            Assert.All(mapping.TableNameForObjectByClass.Values, v => Assert.Matches("^allors[.]allors_[0-9a-v]{26}$", v));
            Assert.All(mapping.TableNameForRelationByRelationType.Values, v => Assert.Matches("^allors[.]allors_[0-9a-v]{26}_(oo|om|mo|mm)$", v));
            Assert.All(mapping.ColumnNameByRelationType, v => Assert.Matches(
                v.Key.RoleType.ObjectType.IsUnit ? "^r_[0-9a-v]{26}$" : v.Key.RoleType.IsMany ? "^a_[0-9a-v]{26}_[0-9a-v]{26}_om$" : "^r_[0-9a-v]{26}_[0-9a-v]{26}_(oo|mo)$",
                v.Value));
        }

        // A relation type keeps its id when its multiplicity changes, when the object type at one of its ends
        // changes, and when it leaves the database. The name of its table or of its column carries the multiplicity
        // and the object types, so the schema of the earlier model has a table or a column that the changed model
        // lacks, and no transaction opens. The message lists them, each with what it holds: a missing one as the
        // model has it, and an obsolete one as its name says. A relation of C1 with itself has its column in the
        // table of C1 whichever end the column holds: a one-to-many relation holds its association there, and a
        // many-to-one relation its role. With one name for both ends, the column of the earlier multiplicity would
        // pass, and the adapter would read the one end as the other.
        [Theory]
        [InlineData("OneToManyBecomesManyToOne")]
        [InlineData("ManyToOneBecomesOneToMany")]
        [InlineData("ManyToOneBecomesOneToOne")]
        [InlineData("ManyToManyBecomesOneToMany")]
        [InlineData("TheRoleOfAColumnBecomesC3")]
        [InlineData("TheRoleOfATableBecomesC3")]
        [InlineData("AColumnLeavesTheDatabase")]
        [InlineData("ATableLeavesTheDatabase")]
        public void ARelationTypeWhoseStorageHasChangedRefusesTheTransaction(string change)
        {
            this.Rebuild();
            var original = this.M;

            var meta = new MetaBuilder().Build();
            string[] expected;
            switch (change)
            {
                case "OneToManyBecomesManyToOne":
                {
                    var role = meta.C1.C1C1one2manies;
                    ((RelationType)role.RelationType).AssignedMultiplicity = Multiplicity.ManyToOne;
                    var name = "C1." + role.Name;
                    expected = new[]
                    {
                        $"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n",
                        $"  - {RoleColumnOf(role, meta.C1, "mo")} is missing: the role of the relation type {name}, ManyToOne, whose object type is C1\n",
                        $"  - {AssociationColumnOf(role, meta.C1)} is obsolete: the association of the relation type {name}, OneToMany, whose object type is C1\n",
                    };
                    break;
                }

                case "ManyToOneBecomesOneToMany":
                {
                    var role = meta.C1.C1C1many2one;
                    ((RelationType)role.RelationType).AssignedMultiplicity = Multiplicity.OneToMany;
                    var name = "C1." + role.Name;
                    expected = new[]
                    {
                        $"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n",
                        $"  - {AssociationColumnOf(role, meta.C1)} is missing: the association of the relation type {name}, OneToMany, whose object type is C1\n",
                        $"  - {RoleColumnOf(role, meta.C1, "mo")} is obsolete: the role of the relation type {name}, ManyToOne, whose object type is C1\n",
                    };
                    break;
                }

                case "ManyToOneBecomesOneToOne":
                {
                    var role = meta.C1.C1C2many2one;
                    ((RelationType)role.RelationType).AssignedMultiplicity = Multiplicity.OneToOne;
                    expected = new[]
                    {
                        $"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n",
                        $"  - {RoleColumnOf(role, meta.C2, "oo")} is missing: the role of the relation type C1.C1C2many2one, OneToOne, whose object type is C2\n",
                        $"  - {RoleColumnOf(role, meta.C2, "mo")} is obsolete: the role of the relation type C1.C1C2many2one, ManyToOne, whose object type is C2\n",
                    };
                    break;
                }

                case "ManyToManyBecomesOneToMany":
                {
                    // I12 has two classes, so the relation has a table of its own in either multiplicity.
                    var role = meta.C1.C1I12many2manies;
                    ((RelationType)role.RelationType).AssignedMultiplicity = Multiplicity.OneToMany;
                    expected = new[]
                    {
                        $"Missing Tables:\n- allors.{RelationTableOf(role, "om")}: the table of the relation type C1.C1I12many2manies, OneToMany\n",
                        $"Obsolete Tables:\n- allors.{RelationTableOf(role, "mm")}: the table of the relation type C1.C1I12many2manies, ManyToMany\n",
                    };
                    break;
                }

                case "TheRoleOfAColumnBecomesC3":
                {
                    var role = meta.C1.C1C2many2one;
                    ((RoleType)role).ObjectType = meta.C3;
                    expected = new[]
                    {
                        $"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n",
                        $"  - {RoleColumnOf(role, meta.C3, "mo")} is missing: the role of the relation type C1.C1C2many2one, ManyToOne, whose object type is C3\n",
                        $"  - {RoleColumnOf(role, meta.C2, "mo")} is obsolete: the role of the relation type C1.C1C2many2one, ManyToOne, whose object type is C2\n",
                    };
                    break;
                }

                case "TheRoleOfATableBecomesC3":
                {
                    var role = meta.C1.C1C2many2manies;
                    ((RoleType)role).ObjectType = meta.C3;
                    expected = new[]
                    {
                        $"Invalid Tables:\n- allors.{this.Table(original.C1.C1C2many2manies)}: the table of the relation type C1.C1C2many2manies, ManyToMany\n",
                        $"  - r_{ModelId.Encode(meta.C3.Id)} is missing: the role, whose object type is C3\n",
                        $"  - {RoleColumnOfRelationTable(original.C1.C1C2many2manies)} is obsolete: the role, whose object type is C2\n",
                    };
                    break;
                }

                case "AColumnLeavesTheDatabase":
                {
                    var role = meta.C1.C1C2many2one;
                    LeaveTheDatabase(meta, role);
                    expected = new[]
                    {
                        $"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n",
                        $"  - {RoleColumnOf(role, meta.C2, "mo")} is obsolete: the role of the relation type C1.C1C2many2one, ManyToOne, whose object type is C2\n",
                    };
                    break;
                }

                case "ATableLeavesTheDatabase":
                {
                    var role = meta.C1.C1C2many2manies;
                    LeaveTheDatabase(meta, role);
                    expected = new[]
                    {
                        $"Obsolete Tables:\n- allors.{this.Table(original.C1.C1C2many2manies)}: the table of the relation type C1.C1C2many2manies, ManyToMany\n",
                    };
                    break;
                }

                default:
                    throw new ArgumentException(change);
            }

            var exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase(meta).CreateTransaction());
            Assert.All(expected, v => Assert.Contains(v, exception.Message));
        }

        // A changed multiplicity gives a relation type another table, or a column in the table of a class. The
        // relation types C1.C1I12many2one and C1.C1I12many2manies have a table of their own in either multiplicity,
        // as I12 has two classes, and C1.C1C2many2manies a table that becomes a column of the table of C1. The
        // schema of the earlier model is refused, and a load in place builds the schema of the changed model and
        // drops the table of the earlier one. Afterwards the earlier model, as a rollback of the code brings it
        // back, refuses the schema in turn.
        [Theory]
        [InlineData("ManyToOneBecomesManyToMany")]
        [InlineData("ManyToManyBecomesManyToOne")]
        [InlineData("ATableBecomesAColumn")]
        public void ARelationTypeWhoseMultiplicityHasChangedIsRebuiltOnLoad(string change)
        {
            var toMany = change == "ManyToOneBecomesManyToMany";
            var toAColumn = change == "ATableBecomesAColumn";

            var original = this.Rebuild();
            var originalMeta = (MetaPopulation)original.MetaPopulation;
            IRoleType originalRole = toMany ? originalMeta.C1.C1I12many2one : toAColumn ? originalMeta.C1.C1C2many2manies : originalMeta.C1.C1I12many2manies;
            var originalTable = this.Table(originalRole);
            long associationId;
            long memberId;
            using (var transaction = original.CreateTransaction())
            {
                var association = transaction.Create(originalMeta.C1);
                var member = transaction.Create(originalMeta.C2);
                association.Strategy.SetRole(originalRole, toMany ? member : new[] { member });
                transaction.Commit();
                associationId = association.Id;
                memberId = member.Id;
            }

            var saved = new StringWriter();
            using (var writer = XmlWriter.Create(saved))
            {
                original.Save(writer);
            }

            var meta = new MetaBuilder().Build();
            RoleType role = toMany ? meta.C1.C1I12many2one : toAColumn ? meta.C1.C1C2many2manies : meta.C1.C1I12many2manies;
            if (!toAColumn)
            {
                role.SingularName = "RevisedMember";
                role.PluralName = "RevisedMembers";
            }

            ((RelationType)role.RelationType).AssignedMultiplicity = toMany ? Multiplicity.ManyToMany : Multiplicity.ManyToOne;
            var name = "C1." + role.Name;

            var changed = this.CreateDatabase(meta);
            var exception = Assert.ThrowsAny<Exception>(() => changed.CreateTransaction());
            Assert.Contains($"Obsolete Tables:\n- allors.{originalTable}: the table of the relation type {name}, {originalRole.RelationType.Multiplicity}\n", exception.Message);
            Assert.Contains(
                toAColumn
                    ? $"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n  - {RoleColumnOf(role, meta.C2, "mo")} is missing: the role of the relation type {name}, ManyToOne, whose object type is C2\n"
                    : $"Missing Tables:\n- allors.{RelationTableOf(role, toMany ? "mm" : "mo")}: the table of the relation type {name}, {role.RelationType.Multiplicity}\n",
                exception.Message);

            using (var reader = XmlReader.Create(new StringReader(saved.ToString())))
            {
                changed.Load(reader);
            }

            using (var transaction = this.CreateDatabase(meta).CreateTransaction())
            {
                var association = transaction.Instantiate(associationId);
                if (toMany)
                {
                    Assert.Equal(memberId, Assert.Single(association.Strategy.GetCompositesRole<IObject>(role)).Id);
                    association.Strategy.AddCompositesRole(role, transaction.Create(meta.C2));
                    transaction.Commit();
                }
                else
                {
                    Assert.Equal(memberId, association.Strategy.GetCompositeRole(role).Id);
                }
            }

            // The load dropped the table of the earlier model, and the earlier model finds it missing.
            exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase().CreateTransaction());
            Assert.Contains($"Missing Tables:\n- allors.{originalTable}: the table of the relation type C1.{originalRole.Name}, {originalRole.RelationType.Multiplicity}\n", exception.Message);
        }

        [Fact]
        public void EnsureIndexesCreatesThemWithoutATransaction()
        {
            var database = this.RebuildWithoutIndexes();

            Assert.Equal(0, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(0, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(0, this.IndexCount(RelationTable, this.RelationTableRoleColumn));

            database.EnsureIndexes();

            Assert.Equal(1, this.IndexCount(this.TableOfC1, UnitColumn));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, this.RelationTableRoleColumn));
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
            Assert.Equal(1, this.IndexCount(this.Table(m.C1.C1C2many2manies), RoleColumnOfRelationTable(m.C1.C1C2many2manies)));
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

            // C1 declares this one-to-many role, but C2 stores its association. A column of that name added to C1
            // by hand is a column that the table of C1 does not have in the model: the schema is refused before
            // its indexes are read.
            var extraColumn = this.Column(m.C1.C1C2one2manies);
            this.DropIndex(this.TableOfC1, RoleIndex);
            this.ExecuteScalar($"ALTER TABLE allors.{this.TableOfC1} ADD {extraColumn} BIGINT NULL");
            this.ExecuteScalar($"CREATE INDEX {this.RoleIndex} ON allors.{this.TableOfC1} ({extraColumn})");

            exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase().CreateTransaction());
            Assert.Contains($"Invalid Tables:\n- allors.{this.TableOfC1}: the table of the class C1\n  - {extraColumn} is obsolete: the association of the relation type C1.C1C2one2manies, OneToMany, whose object type is C1\n", exception.Message);

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
            var many2many = this.Table(m.C1.C1C2many2manies);
            var association = AssociationColumnOfRelationTable(m.C1.C1C2many2manies);
            var role = RoleColumnOfRelationTable(m.C1.C1C2many2manies);
            var integerColumn = this.Column(m.C2.C2AllorsInteger);
            var keyless = this.Table(m.C1.C1C3many2manies);
            var keylessAssociation = AssociationColumnOfRelationTable(m.C1.C1C3many2manies);
            var keylessRole = RoleColumnOfRelationTable(m.C1.C1C3many2manies);

            // The table of a class that the model lacks, whose id is f4c8a6e2-3b1d-4e7a-9c5f-2d8b6a1e0c93.
            const string tableOfAClassThatTheModelLacks = "allors_uj4adohr3l77l72v5m5mk7gcic";

            // The objects table, the table of a class and the table of a relation are missing. The table of C2
            // lacks a column, has one that the model lacks and one of another type, the table of another
            // relation has its key on the association only, and that of a third has no key. The schema holds
            // tables whose names start with allors_ and that the model lacks: one made by hand, one whose name
            // holds a quote and a bracket, and the table of a class. It also holds tables whose names do not
            // start with allors_: one without the prefix, one that differs from it in case only and one that
            // lacks its underscore.
            this.ExecuteScalar("DROP TABLE allors.allors_o");
            this.ExecuteScalar($"DROP TABLE allors.{this.TableOfC1}");
            this.ExecuteScalar($"DROP TABLE allors.{this.RelationTable}");
            this.ExecuteScalar($"ALTER TABLE allors.{this.Table(m.C2)} DROP COLUMN {this.Column(m.C2.C2AllorsString)}");
            this.ExecuteScalar($"ALTER TABLE allors.{this.Table(m.C2)} ADD made_by_hand BIGINT");
            this.ExecuteScalar($"ALTER TABLE allors.{this.Table(m.C2)} DROP COLUMN {integerColumn}");
            this.ExecuteScalar($"ALTER TABLE allors.{this.Table(m.C2)} ADD {integerColumn} BIGINT");
            this.ExecuteScalar($"DROP TABLE allors.{many2many}");
            this.ExecuteScalar($"CREATE TABLE allors.{many2many} ({association} BIGINT NOT NULL, {role} BIGINT NOT NULL, PRIMARY KEY ({association}))");
            this.ExecuteScalar($"DROP TABLE allors.{keyless}");
            this.ExecuteScalar($"CREATE TABLE allors.{keyless} ({keylessAssociation} BIGINT NOT NULL, {keylessRole} BIGINT NOT NULL)");
            this.ExecuteScalar("CREATE TABLE allors.allors_made_by_hand (o BIGINT)");
            this.ExecuteScalar("CREATE TABLE allors.made_by_hand (o BIGINT)");
            this.ExecuteScalar("CREATE TABLE allors.\"allors_quoted\"\"]by_hand\" (o BIGINT)");
            this.ExecuteScalar($"CREATE TABLE allors.{tableOfAClassThatTheModelLacks} (o BIGINT)");
            this.ExecuteScalar("CREATE TABLE allors.\"ALLORS_X_BY_HAND\" (o BIGINT)");
            this.ExecuteScalar("CREATE TABLE allors.allorsx_by_hand (o BIGINT)");
            try
            {
                // The message says what each table is: it names the class or the relation type, and shows the id of
                // a class that the model lacks. Under a table that is invalid it lists the columns, each with its
                // relation type, and the primary key.
                var database = this.CreateDatabase();
                var exception = Assert.ThrowsAny<Exception>(() => database.CreateTransaction());
                Assert.Contains("- allors.allors_o: the objects table\n", exception.Message);
                Assert.Contains($"- allors.{this.TableOfC1}: the table of the class C1\n", exception.Message);
                Assert.Contains($"- allors.{this.RelationTable}: the table of the relation type Company.IndexedMany2ManyPersons, ManyToMany\n", exception.Message);
                Assert.Contains("Obsolete Tables:\n- allors.allors_made_by_hand: a table that is not of the model\n", exception.Message);
                Assert.DoesNotContain("allors.made_by_hand", exception.Message);
                Assert.Contains($"- allors.{this.Table(m.C2)}: the table of the class C2\n", exception.Message);
                Assert.Contains($"  - {this.Column(m.C2.C2AllorsString)} is missing: the role of the relation type C2.C2AllorsString\n", exception.Message);
                Assert.Contains($"  - {integerColumn} has the type bigint and needs ", exception.Message);
                Assert.Contains(": the role of the relation type C2.C2AllorsInteger\n", exception.Message);
                Assert.Contains("  - made_by_hand is obsolete: a column that is not of the model\n", exception.Message);
                Assert.Contains($"- allors.{many2many}: the table of the relation type C1.C1C2many2manies, ManyToMany\n  - the primary key is on ({association}) and needs to be on ({association}, {role})\n", exception.Message);
                Assert.Contains($"- allors.{keyless}: the table of the relation type C1.C1C3many2manies, ManyToMany\n  - the primary key is missing and needs to be on ({keylessAssociation}, {keylessRole})\n", exception.Message);
                Assert.Contains("- allors.allors_quoted\"]by_hand: a table that is not of the model\n", exception.Message);
                Assert.Contains($"- allors.{tableOfAClassThatTheModelLacks}: the table of the class f4c8a6e2-3b1d-4e7a-9c5f-2d8b6a1e0c93\n", exception.Message);
                Assert.DoesNotContain("ALLORS_X_BY_HAND", exception.Message);
                Assert.DoesNotContain("allorsx_by_hand", exception.Message);

                // Init drops every table whose name starts with allors_, whatever else the name holds, and builds
                // the tables of the model. It leaves the other tables.
                database.Init();

                using (database.CreateTransaction())
                {
                }

                Assert.False(this.ExistTable("allors_made_by_hand"));
                Assert.True(this.ExistTable("made_by_hand"));
                Assert.False(this.ExistTable("allors_quoted\"]by_hand"));
                Assert.False(this.ExistTable(tableOfAClassThatTheModelLacks));
                Assert.True(this.ExistTable("ALLORS_X_BY_HAND"));
                Assert.True(this.ExistTable("allorsx_by_hand"));
            }
            finally
            {
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.allors_made_by_hand");
                this.ExecuteScalar("DROP TABLE allors.made_by_hand");
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.\"allors_quoted\"\"]by_hand\"");
                this.ExecuteScalar($"DROP TABLE IF EXISTS allors.{tableOfAClassThatTheModelLacks}");
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.\"ALLORS_X_BY_HAND\"");
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.allorsx_by_hand");
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
            Assert.Equal(1, this.IndexCount(RelationTable, this.RelationTableRoleColumn));

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
            Assert.Equal(1, this.IndexCount(RelationTable, this.RelationTableRoleColumn));

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
            Assert.Equal(1, this.IndexCount(RelationTable, this.RelationTableRoleColumn));
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
        protected string Index(IClass @class, IRoleType roleType) => $"allors_i_{ModelId.Encode(@class.Id)}_{ModelId.Encode(roleType.RelationType.Id)}";

        // The columns of the table of a relation: the association and the role, each with the id of its object type.
        protected static string AssociationColumnOfRelationTable(IRoleType roleType) => "a_" + ModelId.Encode(roleType.AssociationType.ObjectType.Id);

        protected static string RoleColumnOfRelationTable(IRoleType roleType) => "r_" + ModelId.Encode(roleType.ObjectType.Id);

        // The names of a relation type with a multiplicity and an object type that a fact chooses: a table of its own,
        // the column of its role, and the column of its association, which only a one-to-many relation has.
        private static string RelationTableOf(IRoleType roleType, string multiplicity) => $"allors_{ModelId.Encode(roleType.RelationType.Id)}_{multiplicity}";

        private static string RoleColumnOf(IRoleType roleType, IObjectType objectType, string multiplicity) => $"r_{ModelId.Encode(roleType.RelationType.Id)}_{ModelId.Encode(objectType.Id)}_{multiplicity}";

        private static string AssociationColumnOf(IRoleType roleType, IObjectType objectType) => $"a_{ModelId.Encode(roleType.RelationType.Id)}_{ModelId.Encode(objectType.Id)}_om";

        // A relation type that leaves the database stays in the model, in the session.
        private static void LeaveTheDatabase(MetaPopulation meta, IRoleType roleType)
        {
            var relationType = (RelationType)roleType.RelationType;
            relationType.AssignedOrigin = Origin.Session;
            meta.StructuralDerive();

            // The assignment makes the model derive again.
            relationType.AssignedMultiplicity = relationType.AssignedMultiplicity;
        }

        private static string WithoutSchema(string name) => name.Substring(name.IndexOf('.') + 1);

        // Read from the views of information_schema that both servers have.
        private bool ExistTable(string table) => Convert.ToInt32(this.ExecuteScalar($"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'allors' AND table_name = '{table}'"), CultureInfo.InvariantCulture) == 1;

        private bool ExistColumn(string table, string column) => Convert.ToInt32(this.ExecuteScalar($"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = 'allors' AND table_name = '{table}' AND column_name = '{column}'"), CultureInfo.InvariantCulture) == 1;

        // The name of the primary key of the table.
        private string PrimaryKey(string table) => (string)this.ExecuteScalar($"SELECT constraint_name FROM information_schema.table_constraints WHERE table_schema = 'allors' AND table_name = '{table}' AND constraint_type = 'PRIMARY KEY'");

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
