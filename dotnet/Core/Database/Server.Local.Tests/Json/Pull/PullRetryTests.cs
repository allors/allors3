// <copyright file="PullRetryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Threading;
    using Allors.Database;
    using Allors.Database.Adapters.Sql.Tracing;
    using Allors.Database.Configuration;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Services;
    using Xunit;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using SqlDatabase = Allors.Database.Adapters.Sql.Database;

    public class PullRetryTests : ApiTest, IClassFixture<Fixture>
    {
        public PullRetryTests(Fixture fixture) : base(fixture)
        {
        }

        // Reads are idempotent: a pull that fails with a transient DbException is retried. The retry
        // reuses the request's transaction, which the failed attempt has disposed (rolled back).
        [Fact]
        public void PullRetriesOnDbException()
        {
            var sink = new Sink();
            var database = (SqlDatabase)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            // Fail the first SQL command of the pull, where a deadlock or a timeout surfaces.
            var failed = false;
            sink.PreOnBefore = @event =>
            {
                if (!failed && @event is not PullEvent)
                {
                    failed = true;
                    throw new TestDbException();
                }
            };

            var controller = new PullController(new StubTransactionService { Transaction = this.Transaction }, new StubWorkspaceService { Name = "Default" }, new PolicyService());

            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    new Pull
                    {
                        er = PreparedExtents.OrganisationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var pullResponse = controller.Post(pullRequest, CancellationToken.None).Value;

            Assert.True(failed);
            Assert.Single(pullResponse.c["Organisations"]);
        }

        private sealed class TestDbException : DbException
        {
        }

        private sealed class StubTransactionService : ITransactionService
        {
            public ITransaction Transaction { get; set; }
        }

        private sealed class StubWorkspaceService : IWorkspaceService
        {
            public string Name { get; set; }
        }
    }
}
