// <copyright file="CacheTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Linq;
    using Meta;
    using Tracing;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    public abstract class TracingTest : IDisposable
    {
        private TestPopulation population;

        private void Init()
        {
            var database = (Database)this.CreateDatabase();
            database.Init();
            using var transaction = database.CreateTransaction();
            this.population = new TestPopulation(transaction);
            transaction.Commit();

            this.M = (MetaPopulation)database.MetaPopulation;
        }

        public MetaPopulation M { get; private set; }

        public abstract void Dispose();

        [Fact]
        public void Initial()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            Assert.Empty(sink.TreeByTransaction);
        }

        [Fact]
        public void Instantiate()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            var c1 = (C1)transaction.Instantiate(this.population.C1A);

            var transactionSink = sink.TreeByTransaction[transaction];

            Assert.Equal(1, transactionSink.Nodes.Count);
            Assert.Equal(typeof(SqlInstantiateObjectEvent), transactionSink.Nodes[0].Event.GetType());
            Assert.Empty(transactionSink.Nodes[0].Nodes);
        }

        [Fact]
        public void Prefetch()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            var c1b = (C1)transaction.Instantiate(this.population.C1B);

            var transactionSink = sink.TreeByTransaction[transaction];
            transactionSink.Clear();

            var prefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(this.M.C1.C1C2one2one)
                .Build();

            transaction.Prefetch(prefetchPolicy, c1b);

            var events = transactionSink.Nodes;
            Assert.Equal(2, events.Count);
            Assert.Equal(typeof(SqlPrefetchCompositeRoleObjectTableEvent), events[0].Event.GetType());
            Assert.Empty(events[0].Nodes);
            Assert.Equal(typeof(SqlInstantiateReferencesEvent), events[1].Event.GetType());
            Assert.Empty(events[1].Nodes);

            transactionSink.Clear();

            var c2b = c1b.C1C2one2one;

            Assert.Empty(transactionSink.Nodes);

            // A composite role in each of the four ways in which the SQL adapters store one: to-one and to-many,
            // in a column of a class table and in a relation table.
            var roleTypes = new IRoleType[]
            {
                this.M.C1.C1C2one2one,
                this.M.C1.I12C2one2one,
                this.M.C1.C1C2one2manies,
                this.M.C1.C1C2many2manies,
            };

            foreach (var roleType in roleTypes)
            {
                // The database that creates the objects knows their classes.
                var creator = (Database)this.CreateDatabase();

                var c1Ids = new long[2];
                using (var setup = creator.CreateTransaction())
                {
                    for (var i = 0; i < c1Ids.Length; i++)
                    {
                        var setupC1 = C1.Create(setup);
                        var setupC2 = C2.Create(setup);
                        if (roleType.IsOne)
                        {
                            setupC1.Strategy.SetCompositeRole(roleType, setupC2);
                        }
                        else
                        {
                            setupC1.Strategy.AddCompositesRole(roleType, setupC2);
                        }

                        c1Ids[i] = setupC1.Id;
                    }

                    setup.Commit();
                }

                // A database of its own for each role type and each state of its cache, so that no reference is left
                // of an earlier case: a database that does not know the classes of the role objects, and the one
                // that does.
                foreach (var cachedDatabase in new[] { (Database)this.CreateDatabase(), creator })
                {
                    var cachedSink = new Sink();
                    cachedDatabase.Sink = cachedSink;

                    // An earlier transaction asks for the roles, so that the cache of the database holds them.
                    using (var earlier = cachedDatabase.CreateTransaction())
                    {
                        foreach (var c1 in earlier.Instantiate(c1Ids))
                        {
                            Assert.True(c1.Strategy.ExistRole(roleType));
                        }
                    }

                    using var cachedTransaction = (Transaction)cachedDatabase.CreateTransaction();

                    var c1s = cachedTransaction.Instantiate(c1Ids);

                    var cachedTransactionSink = cachedSink.TreeByTransaction[cachedTransaction];
                    cachedTransactionSink.Clear();

                    cachedTransaction.Prefetch(new PrefetchPolicyBuilder().WithRule(roleType).Build(), c1s);

                    // The prefetch loads the references of the roles that the cache holds, in one command.
                    var cachedEvents = cachedTransactionSink.Nodes;
                    Assert.Equal(2, cachedEvents.Count);
                    Assert.Equal(typeof(SqlInstantiateReferencesEvent), cachedEvents[1].Event.GetType());

                    cachedTransactionSink.Clear();

                    foreach (var c1 in c1s)
                    {
                        if (roleType.IsOne)
                        {
                            Assert.NotNull(c1.Strategy.GetCompositeRole(roleType));
                        }
                        else
                        {
                            Assert.Single(c1.Strategy.GetCompositesRole<C2>(roleType));
                        }
                    }

                    // Reading the roles therefore issues no database command.
                    Assert.Empty(cachedTransactionSink.Nodes);
                }
            }
        }

        [Fact]
        public void PrefetchUsesModifiedCompositesRole()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            long c1Id;
            long c2bId;
            using (var setup = database.CreateTransaction())
            {
                var setupC1 = C1.Create(setup);
                var setupC2a = C2.Create(setup);
                var setupC2b = C2.Create(setup);
                var setupC2x = C2.Create(setup);
                setupC1.AddC1C2one2many(setupC2a);    // committed: c1 -> [c2a]
                setupC2b.AddC2C2one2many(setupC2x);   // committed nested: c2b -> [c2x]
                setup.Commit();
                c1Id = setupC1.Id;
                c2bId = setupC2b.Id;
            }

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            var c1 = (C1)transaction.Instantiate(c1Id);
            var c2b = (C2)transaction.Instantiate(c2bId);

            // Modify (uncommitted): c1 -> [c2a, c2b].
            c1.AddC1C2one2many(c2b);

            var nestedPolicy = new PrefetchPolicyBuilder().WithRule(this.M.C2.C2C2one2manies).Build();
            var prefetchPolicy = new PrefetchPolicyBuilder().WithRule(this.M.C1.C1C2one2manies, nestedPolicy).Build();
            transaction.Prefetch(prefetchPolicy, c1);

            var transactionSink = sink.TreeByTransaction[transaction];
            transactionSink.Clear();

            // c2b was added to c1's composites role in-memory; the prefetch must include it in the
            // transitive prefetch, so reading its nested role issues no database command.
            _ = c2b.C2C2one2manies.ToArray();

            Assert.Empty(transactionSink.Nodes);
        }

        [Fact]
        public void PrefetchUsesKnownAssociation()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();

            // An association in each of the four ways in which the SQL adapters store one: to-one and to-many,
            // in a column of a class table and in a relation table.
            var associationTypes = new IAssociationType[]
            {
                this.M.C2.C1WhereC1C2one2one,
                this.M.C2.I12WhereI12C2one2one,
                this.M.C2.C1sWhereC1C2many2one,
                this.M.C2.C1sWhereC1C2many2many,
            };

            var c2Ids = new long[associationTypes.Length];
            using (var setup = database.CreateTransaction())
            {
                for (var i = 0; i < associationTypes.Length; i++)
                {
                    var setupC1 = C1.Create(setup);
                    var setupC2 = C2.Create(setup);
                    var roleType = associationTypes[i].RoleType;
                    if (roleType.IsOne)
                    {
                        setupC1.Strategy.SetCompositeRole(roleType, setupC2);
                    }
                    else
                    {
                        setupC1.Strategy.AddCompositesRole(roleType, setupC2);
                    }

                    c2Ids[i] = setupC2.Id;
                }

                setup.Commit();
            }

            // An existence check knows the association IDs without instantiating their objects.
            // A fresh database cache ensures that a flat prefetch must load those references.
            for (var i = 0; i < associationTypes.Length; i++)
            {
                var associationType = associationTypes[i];
                if (!associationType.IsMany)
                {
                    continue;
                }

                var flatDatabase = (Database)this.CreateDatabase();
                var flatSink = new Sink();
                flatDatabase.Sink = flatSink;
                using var flatTransaction = (Transaction)flatDatabase.CreateTransaction();
                var c2 = (C2)flatTransaction.Instantiate(c2Ids[i]);
                Assert.True(c2.Strategy.ExistCompositesAssociation(associationType));

                var prefetchPolicy = new PrefetchPolicyBuilder().WithRule(associationType).Build();
                flatTransaction.Prefetch(prefetchPolicy, c2);

                var transactionSink = flatSink.TreeByTransaction[flatTransaction];
                transactionSink.Clear();

                Assert.Single(c2.Strategy.GetCompositesAssociation<C1>(associationType));
                Assert.Empty(transactionSink.Nodes);
            }

            var sink = new Sink();
            database.Sink = sink;

            using var transaction = (Transaction)database.CreateTransaction();

            for (var i = 0; i < associationTypes.Length; i++)
            {
                var associationType = associationTypes[i];
                var c2 = (C2)transaction.Instantiate(c2Ids[i]);

                // The transaction reads the association, and so knows it before the prefetch.
                var c1 = associationType.IsOne
                    ? (C1)c2.Strategy.GetCompositeAssociation(associationType)
                    : c2.Strategy.GetCompositesAssociation<C1>(associationType).Single();

                var prefetchPolicy = new PrefetchPolicyBuilder()
                    .WithRule(associationType, new IPropertyType[] { this.M.C1.C1AllorsString })
                    .Build();
                transaction.Prefetch(prefetchPolicy, c2);

                var transactionSink = sink.TreeByTransaction[transaction];
                transactionSink.Clear();

                // The nested rule applies to the association that the transaction knew, so reading its unit role
                // issues no database command.
                _ = c1.C1AllorsString;

                Assert.Empty(transactionSink.Nodes);
            }
        }

        // The database checks a statement against its tables and columns when a command executes it, and the other
        // facts do not execute every statement of the test domain. This fact executes each statement that the mapping
        // holds once, with the parameters that the adapter gives it and with ids that no object has, on a connection
        // that it rolls back.
        [Fact]
        public void EveryStatementOfTheMappingExecutes()
        {
            this.Init();
            var database = (Database)this.CreateDatabase();
            var mapping = database.Mapping;

            // An id that no object has. Every table parameter gets one row: SqlClient refuses one without rows.
            const long NoObject = long.MaxValue - 7;
            var objects = new[] { NoObject };
            var relations = new[] { new CompositeRelation(NoObject, NoObject - 1) };

            var executed = 0;
            var connection = database.ConnectionFactory.Create();

            void Execute(string member, string statement, bool returnsRows, Action<ICommand> parameters)
            {
                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = statement;
                    parameters(command);

                    if (returnsRows)
                    {
                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                        }
                    }
                    else
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"The statement {member} of the mapping does not execute: {e.Message}\n{statement}", e);
                }

                ++executed;
            }

            try
            {
                Execute(nameof(mapping.CommandTextForInstantiate), mapping.CommandTextForInstantiate, true, v => v.ObjectTableParameter(objects));
                Execute(nameof(mapping.CommandTextForGetVersion), mapping.CommandTextForGetVersion, true, v => v.AddCompositesRoleTableParameter(objects));
                Execute(nameof(mapping.CommandTextForUpdateVersion), mapping.CommandTextForUpdateVersion, false, v => v.ObjectTableParameter(objects));

                foreach (var (@class, statement) in mapping.CommandTextForCreateObjectByClass)
                {
                    Execute($"{nameof(mapping.CommandTextForCreateObjectByClass)}[{@class}]", statement, true, v => v.AddTypeParameter(@class));
                }

                foreach (var (@class, statement) in mapping.CommandTextForCreateObjectsByClass)
                {
                    Execute(
                        $"{nameof(mapping.CommandTextForCreateObjectsByClass)}[{@class}]",
                        statement,
                        true,
                        v =>
                        {
                            v.AddTypeParameter(@class);
                            v.AddCountParameter(2);
                        });
                }

                foreach (var (@class, statement) in mapping.CommandTextForDeleteObjectByClass)
                {
                    Execute($"{nameof(mapping.CommandTextForDeleteObjectByClass)}[{@class}]", statement, false, v => v.ObjectParameter(NoObject));
                }

                foreach (var (@class, statement) in mapping.CommandTextForGetUnitRolesByClass)
                {
                    Execute($"{nameof(mapping.CommandTextForGetUnitRolesByClass)}[{@class}]", statement, true, v => v.ObjectParameter(NoObject));
                }

                foreach (var (@class, statement) in mapping.CommandTextForPrefetchUnitRolesByClass)
                {
                    Execute($"{nameof(mapping.CommandTextForPrefetchUnitRolesByClass)}[{@class}]", statement, true, v => v.AddCompositesRoleTableParameter(objects));
                }

                foreach (var (@class, statementByRelationType) in mapping.CommandTextForSetUnitRoleByRelationTypeByClass)
                {
                    foreach (var (relationType, statement) in statementByRelationType)
                    {
                        var roleType = relationType.RoleType;
                        var units = new[] { new UnitRelation(NoObject, UnitFor(roleType)) };
                        Execute($"{nameof(mapping.CommandTextForSetUnitRoleByRelationTypeByClass)}[{@class}][{relationType}]", statement, false, v => v.UnitTableParameter(roleType, units));
                    }
                }

                foreach (var (relationType, statement) in mapping.CommandTextForGetRoleByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForGetRoleByRelationType)}[{relationType}]", statement, true, v => v.AddAssociationParameter(NoObject));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForPrefetchRoleByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForPrefetchRoleByRelationType)}[{relationType}]", statement, true, v => v.AddCompositesRoleTableParameter(objects));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForSetRoleByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForSetRoleByRelationType)}[{relationType}]", statement, false, v => v.AddCompositeRoleTableParameter(relations));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForAddRoleByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForAddRoleByRelationType)}[{relationType}]", statement, false, v => v.AddCompositeRoleTableParameter(relations));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForRemoveRoleByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForRemoveRoleByRelationType)}[{relationType}]", statement, false, v => v.AddCompositeRoleTableParameter(relations));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForClearRoleByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForClearRoleByRelationType)}[{relationType}]", statement, false, v => v.ObjectTableParameter(objects));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForGetAssociationByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForGetAssociationByRelationType)}[{relationType}]", statement, true, v => v.AddCompositeRoleParameter(NoObject));
                }

                foreach (var (relationType, statement) in mapping.CommandTextForPrefetchAssociationByRelationType)
                {
                    Execute($"{nameof(mapping.CommandTextForPrefetchAssociationByRelationType)}[{relationType}]", statement, true, v => v.ObjectTableParameter(objects));
                }
            }
            finally
            {
                connection.Rollback();
            }

            Assert.NotEqual(0, executed);
        }

        // The parameter of a unit role carries the id of its relation type: the sixteen bytes of the id in network
        // byte order, in the base32hex alphabet of RFC 4648 in lower case and without padding. The full names of
        // these two roles, WorkFlowOwner and WorkflowOwner, differ only in case.
        [Fact]
        public void TheParameterOfAUnitRoleCarriesTheIdOfItsRelationType()
        {
            var database = (Database)this.CreateDatabase();
            var m = (MetaPopulation)database.MetaPopulation;
            var mapping = database.Mapping;

            // ed9cb638-553c-4018-a01e-ff73821a8e26 and 70aec4b0-86ad-4b92-bd5f-cda40fb55776
            Assert.Matches("^[:@]p_tmebce2l7h01h80uvtpo46ke4o$", mapping.ParamInvocationNameByRoleType[m.Work.FlowOwner]);
            Assert.Matches("^[:@]p_e2nc9c46ll5p5favpmi0vdaneo$", mapping.ParamInvocationNameByRoleType[m.Workflow.Owner]);
        }

        protected abstract IDatabase CreateDatabase();

        // A value that a unit role of the given type takes.
        private static object UnitFor(IRoleType roleType) =>
            ((IUnit)roleType.ObjectType).Tag switch
            {
                UnitTags.String => "a",
                UnitTags.Integer => 1,
                UnitTags.Decimal => 0m,
                UnitTags.Float => 0d,
                UnitTags.Boolean => false,
                UnitTags.DateTime => new System.DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UnitTags.Unique => Guid.Empty,
                UnitTags.Binary => new byte[] { 1 },
                _ => throw new ArgumentException("Unknown unit: " + roleType.ObjectType),
            };
    }
}
