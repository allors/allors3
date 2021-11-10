// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Adapters.Sql.Tracing;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;

    public class TracingTests : ApiTest, IClassFixture<Fixture>
    {
        private TraceX x1;
        private TraceY y1;
        private TraceZ z1;

        public TracingTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public void Pull()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            tree.Clear();

            var pull = new Pull
            {
                Extent = new Extent(this.M.TraceX)
                {
                    Predicate = new Equals(this.M.TraceX.AllorsString) { Value = "X1" }
                },
                Results = new[]
                {
                    new  Result
                    {
                        Include = new []
                        {
                            new Node(this.M.TraceX.One2One)
                        }
                    },
                }
            };

            var pullRequest = new PullRequest
            {
                d = new[]
                {
                    new PullDependency
                    {
                        o = this.M.TraceY.Tag,
                        r = this.M.TraceY.One2One.RelationType.Tag,
                    }
                },
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default");
            var pullResponse = api.Pull(pullRequest);

            tree.Clear();
            pullResponse = api.Pull(pullRequest);

            Assert.All(tree.Nodes, v => Assert.True(v.Event.Source == EventSource.Prefetcher));
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
                o = new[] { this.x1.Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            tree.Clear();
            syncResponse = api.Sync(syncRequest);

            Assert.All(tree.Nodes, v => Assert.True(v.Event.Source == EventSource.Prefetcher));
            Assert.All(tree.Nodes, v => Assert.Empty(v.Nodes));
        }

        private void Populate()
        {
            this.x1 = new TraceXBuilder(this.Transaction)
                .WithAllorsString("X1")
                .Build();
            this.y1 = new TraceYBuilder(this.Transaction)
                .WithAllorsString("Y1")
                .Build();
            this.z1 = new TraceZBuilder(this.Transaction)
                .WithAllorsString("Z1")
                .Build();


            this.x1.One2One = this.y1;
            this.y1.One2One = this.z1;

            this.Transaction.Commit();
        }
    }
}
