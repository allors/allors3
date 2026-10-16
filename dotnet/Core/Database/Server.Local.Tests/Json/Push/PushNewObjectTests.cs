// <copyright file="PushTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Push;
    using Xunit;

    public class PushNewObjectTests : ApiTest, IClassFixture<Fixture>
    {
        public PushNewObjectTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void PushReadReturnsCommittedNewIdMappingsAndFullCreatedObjects()
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var scope = new Allors.Protocol.Json.Api.Read.ReadScope { p = population.Incarnation.ToString("D"), u = user.Id, w = "X", r = "revision-1" };
            var request = new
            {
                v = 1, i = "push-read-new-object", sc = scope,
                push = new { n = new[] { new PushRequestNewObject
                {
                    t = this.M.WorkspaceXObject1.Tag, w = -1,
                    r = new[] { new PushRequestRole { t = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag, u = "Created by PushRead" } },
                } } },
                returnChanged = true,
            };
            var executor = new PushReadExecutor(this.Transaction.Database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" }));

            using var response = JsonDocument.Parse(executor.Execute(JsonSerializer.Serialize(request), "X", new LocalReadIdentity(population.Incarnation, user.Id)));

            Assert.Equal("committed", response.RootElement.GetProperty("push").GetProperty("k").GetString());
            var mapping = Assert.Single(response.RootElement.GetProperty("push").GetProperty("n").EnumerateArray());
            Assert.Equal(-1, mapping.GetProperty("w").GetInt64());
            var id = mapping.GetProperty("d").GetInt64();
            Assert.True(id > 0);
            var record = Assert.Single(response.RootElement.GetProperty("changed").GetProperty("p").EnumerateArray());
            Assert.Equal(id, record.GetProperty("i").GetInt64());
            Assert.Equal("Created by PushRead", record.GetProperty("p").GetProperty("ro").EnumerateArray().Single(value => value.GetProperty("t").GetString() == this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag).GetProperty("v").GetString());
            this.Transaction.Rollback();
            Assert.Equal("Created by PushRead", ((WorkspaceXObject1)this.Transaction.Instantiate(id)).WorkspaceXString);
        }

        [Fact]
        public void PushReadReportsAccessErrorsUsingTheRejectedNewObjectsWorkspaceId()
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var request = new
            {
                v = 1, i = "push-read-rejected-new-object",
                sc = new Allors.Protocol.Json.Api.Read.ReadScope { p = population.Incarnation.ToString("D"), u = user.Id, w = "X", r = "revision-1" },
                push = new { n = new[] { new PushRequestNewObject { t = this.M.WorkspaceNoneObject1.Tag, w = -1 } } },
                returnChanged = true,
            };
            var executor = new PushReadExecutor(this.Transaction.Database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" }));

            using var response = JsonDocument.Parse(executor.Execute(JsonSerializer.Serialize(request), "X", new LocalReadIdentity(population.Incarnation, user.Id)));

            var push = response.RootElement.GetProperty("push");
            Assert.Equal("rejected", push.GetProperty("k").GetString());
            Assert.Equal(-1, Assert.Single(push.GetProperty("_a").EnumerateArray()).GetInt64());
            Assert.False(push.TryGetProperty("n", out var mappings) && mappings.ValueKind != JsonValueKind.Null);
            Assert.False(response.RootElement.TryGetProperty("changed", out var changed) && changed.ValueKind != JsonValueKind.Null);
        }

        [Fact]
        public void PushReadCommitReceiptOwnsMappingsBeforePostCommitTracingCanChangeThem()
        {
            this.Transaction.Rollback();
            var database = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            var population = database.PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var request = new
            {
                v = 1, i = "push-read-owned-receipt",
                sc = new Allors.Protocol.Json.Api.Read.ReadScope { p = population.Incarnation.ToString("D"), u = user.Id, w = "X", r = "revision-1" },
                push = new { n = new[] { new PushRequestNewObject { t = this.M.WorkspaceXObject1.Tag, w = -1 } } },
                returnChanged = true,
            };
            long committedId = 0;
            var traces = 0;
            database.Sink = new ReceiptMutatingSink(response =>
            {
                traces++;
                committedId = Assert.Single(response.n).d;
                response.n[0].d = user.Id;
                response.n = new[] { new PushResponseNewObject { w = -1, d = user.Id + 1 } };
            });
            var executor = new PushReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" }));

            using var response = JsonDocument.Parse(executor.Execute(JsonSerializer.Serialize(request), "X", new LocalReadIdentity(population.Incarnation, user.Id)));

            Assert.Equal(1, traces);
            Assert.Equal("committed", response.RootElement.GetProperty("push").GetProperty("k").GetString());
            var mapping = Assert.Single(response.RootElement.GetProperty("push").GetProperty("n").EnumerateArray());
            Assert.Equal(-1, mapping.GetProperty("w").GetInt64());
            Assert.Equal(committedId, mapping.GetProperty("d").GetInt64());
            Assert.Equal(committedId, Assert.Single(response.RootElement.GetProperty("changed").GetProperty("p").EnumerateArray()).GetProperty("i").GetInt64());
        }

        [Fact]
        public void PushReadOwnsCapturedChangesBeforeTracingCanChangeTheRequest()
        {
            this.Transaction.Rollback();
            var database = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            var population = database.PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            var request = new
            {
                v = 1, i = "push-read-owned-changes",
                sc = new Allors.Protocol.Json.Api.Read.ReadScope { p = population.Incarnation.ToString("D"), u = user.Id, w = "X", r = "revision-1" },
                push = new { n = new[] { new PushRequestNewObject
                {
                    t = this.M.WorkspaceXObject1.Tag, w = -1,
                    r = new[] { new PushRequestRole { t = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag, u = "Captured value" } },
                } } },
                returnChanged = true,
            };
            var traces = 0;
            database.Sink = new ReceiptMutatingSink(before: traced =>
            {
                traces++;
                traced.n[0].w = -2;
                traced.n[0].r[0].u = "Mutated trace value";
                traced.n = new[] { new PushRequestNewObject { t = this.M.WorkspaceXObject1.Tag, w = -3 } };
            });
            var executor = new PushReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" }));

            using var response = JsonDocument.Parse(executor.Execute(JsonSerializer.Serialize(request), "X", new LocalReadIdentity(population.Incarnation, user.Id)));

            Assert.Equal(1, traces);
            Assert.Equal("committed", response.RootElement.GetProperty("push").GetProperty("k").GetString());
            var mapping = Assert.Single(response.RootElement.GetProperty("push").GetProperty("n").EnumerateArray());
            Assert.Equal(-1, mapping.GetProperty("w").GetInt64());
            var record = Assert.Single(response.RootElement.GetProperty("changed").GetProperty("p").EnumerateArray());
            Assert.Equal(mapping.GetProperty("d").GetInt64(), record.GetProperty("i").GetInt64());
            Assert.Equal("Captured value", record.GetProperty("p").GetProperty("ro").EnumerateArray().Single(value => value.GetProperty("t").GetString() == this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag).GetProperty("v").GetString());
        }

        private sealed class ReceiptMutatingSink : Allors.Database.Tracing.ISink
        {
            private readonly Action<PushResponse> after;
            private readonly Action<PushRequest> before;
            internal ReceiptMutatingSink(Action<PushResponse> after = null, Action<PushRequest> before = null) { this.after = after; this.before = before; }
            public void OnBefore(Allors.Database.Tracing.IEvent value)
            {
                if (value is PushEvent push) { this.before?.Invoke(push.PushRequest); }
            }
            public void OnAfter(Allors.Database.Tracing.IEvent value)
            {
                if (value is PushEvent push) { this.after?.Invoke(push.PushResponse); }
            }
        }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceX()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceXObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "X", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            var x1 = (WorkspaceXObject1)this.Transaction.Instantiate(pushResponse.n[0].d);

            Assert.NotNull(x1);
        }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceY()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceXObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "Y", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Null(pushResponse.n);
        }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceNone()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceXObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "None", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Null(pushResponse.n);
        }

        [Fact]
        public void WorkspaceY1ObjectInWorkspaceNone()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceYObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "None", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Null(pushResponse.n);
        }

        [Fact]
        public void WorkspaceNoneObjectInWorkspaceX()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceNoneObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "X", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Null(pushResponse.n);
        }

        [Fact]
        public void WorkspaceNoneObjectInWorkspaceY()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceNoneObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "Y", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Null(pushResponse.n);
        }

        [Fact]
        public void WorkspaceNoneObjectInWorkspaceNone()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                n = new[] { new PushRequestNewObject { t = this.M.WorkspaceNoneObject1.Tag, w = -1 }, },
            };

            var api = new Api(this.Transaction, "None", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            Assert.True(pushResponse.HasErrors);
            Assert.Null(pushResponse.n);
        }
    }
}
