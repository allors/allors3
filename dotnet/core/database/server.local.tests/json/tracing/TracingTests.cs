// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Linq;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Adapters.Sql.Tracing;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;

    public class TracingTests : ApiTest, IClassFixture<Fixture>
    {
        private TraceX[] x;
        private TraceY[] y;
        private TraceZ[] z;

        public TracingTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public void PullManyObjects()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            tree.Clear();

            //sink.Breaker = v =>
            //{
            //    return v.Kind == EventKind.CommandsInstantiateObject;
            //};

            var pullRequest = new PullRequest
            {
                l = Enumerable.Range(0, 100).Select(v => new Allors.Protocol.Json.Data.Pull
                {
                    o = this.x[v].Id
                }).ToArray()
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            Assert.Single(tree.Nodes.Where(v => v.Event is SqlInstantiateObjectEvent));
            Assert.Equal(2, tree.Nodes.Count(v => v.Event is SqlInstantiateReferencesEvent));
            Assert.All(tree.Nodes, v => Assert.Empty(v.Nodes));

            this.Transaction.Rollback();

            tree.Clear();

            pullResponse = api.Pull(pullRequest);

            Assert.Single(tree.Nodes.Where(v => v.Event is SqlGetVersionsEvent));
            Assert.Empty(tree.Nodes.Where(v => v.Event is SqlInstantiateObjectEvent));
            Assert.Empty(tree.Nodes.Where(v => v.Event is SqlInstantiateReferencesEvent));
            Assert.All(tree.Nodes, v => Assert.Empty(v.Nodes));
        }

        [Fact]
        public void PullInclude()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            tree.Clear();

            var pullRequest = new PullRequest
            {
                d = new[]
                {
                    new PullDependency
                    {
                        o = this.M.TraceX.Tag,
                        r = this.M.TraceX.One2One.RelationType.Tag,
                    },
                    new PullDependency
                    {
                        o = this.M.TraceY.Tag,
                        r = this.M.TraceY.One2One.RelationType.Tag,
                    },
                },
                l = Enumerable.Range(0, 100).Select(v => new Allors.Protocol.Json.Data.Pull
                {
                    o = this.x[v].Id
                }).ToArray()
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            tree.Clear();
            pullResponse = api.Pull(pullRequest);

            Assert.All(tree.Nodes, v => Assert.StartsWith("SqlPrefetch", v.Event.GetType().Name));
            Assert.All(tree.Nodes, v => Assert.Empty(v.Nodes));

            this.Transaction.Rollback();
            tree.Clear();

            //sink.Breaker = v =>
            //{
            //    return v.Kind == EventKind.PrefetcherPrefetchCompositesRoleRelationTable;
            //};

            pullResponse = api.Pull(pullRequest);

            Assert.All(tree.Nodes, v => Assert.Empty(v.Nodes));
        }

        [Fact]
        public void Sync()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            tree.Clear();

            var syncRequest = new SyncRequest
            {
                o = new[] { this.x[0].Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            tree.Clear();
            syncResponse = api.Sync(syncRequest);

            Assert.All(tree.Nodes, v => Assert.StartsWith("SqlPrefetch", v.Event.GetType().Name));
            Assert.All(tree.Nodes, v => Assert.Empty(v.Nodes));
        }

        private void Populate()
        {
            this.x = new TraceX[100];
            this.y = new TraceY[100];
            this.z = new TraceZ[100];

            for (var i = 0; i < 100; i++)
            {
                this.x[i] = new TraceXBuilder(this.Transaction)
                    .WithAllorsString($"X{i}")
                    .Build();

                this.y[i] = new TraceYBuilder(this.Transaction)
                    .WithAllorsString($"Y{i}")
                    .Build();

                this.z[i] = new TraceZBuilder(this.Transaction)
                    .WithAllorsString($"Z{i}")
                    .Build();
            }

            for (var i = 0; i < 100; i++)
            {
                this.x[i].One2One = this.y[i];
                this.y[i].One2One = this.z[i];
            }

            this.Transaction.Commit();
        }
    }
}
