// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Adapters.Sql.Tracing;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Sync;
    using Xunit;

    public class TracingTests : ApiTest, IClassFixture<Fixture>
    {
        private TraceX x1;
        private TraceY y1;
        private TraceZ z1;

        public TracingTests(Fixture fixture) : base(fixture) { }

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

            var syncRequest = new SyncRequest
            {
                o = new[] { this.x1.Id },
            };

            var api = new Api(this.Transaction, "Default");
            var syncResponse = api.Sync(syncRequest);

            //Assert.Equal(1, tree.Nodes.Count);
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
