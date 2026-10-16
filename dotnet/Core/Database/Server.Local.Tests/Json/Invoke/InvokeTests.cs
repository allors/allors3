// <copyright file="SyncTests.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.InvokeRead;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Database.Tracing;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Xunit;

    public class InvokeTests : ApiTest, IClassFixture<Fixture>
    {
        private WorkspaceXObject1 x1;
        private long x1Version;

        private WorkspaceYObject1 y1;
        private long y1Version;

        private WorkspaceNoneObject1 none1;
        private long none1Version;

        public InvokeTests(Fixture fixture) : base(fixture)
        {
            this.x1 = new WorkspaceXObject1Builder(this.Transaction).Build();
            this.y1 = new WorkspaceYObject1Builder(this.Transaction).Build();
            this.none1 = new WorkspaceNoneObject1Builder(this.Transaction).Build();

            this.x1Version = this.x1.Strategy.ObjectVersion;
            this.y1Version = this.y1.Strategy.ObjectVersion;
            this.none1Version = this.none1.Strategy.ObjectVersion;

            this.Transaction.Commit();
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void LegacyInvokeRemovedModesCannotPartiallyCommit(string codec)
        {
            var first = new OrganisationBuilder(this.Transaction).WithName("Atomic first").Build();
            var second = new OrganisationBuilder(this.Transaction).WithName("Atomic second").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var calls = 0;
            var database = this.InvokeDatabase(new InvokeObjectFactory(((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).ObjectFactory, organisation =>
            {
                calls++;
                if (organisation.Id == second.Id) { throw new System.InvalidOperationException("The second method failed."); }
                organisation.Name = "Must roll back";
            }));
            using var transaction = database.CreateTransaction();
            transaction.Services.Get<Allors.Database.Services.IUserService>().User = transaction.Instantiate(this.SetUser("jane@example.com").Id) as User;
            var raw = System.Text.Json.JsonSerializer.Serialize(new
            {
                l = new[]
                {
                    new Invocation { i = first.Id, v = first.Strategy.ObjectVersion, m = this.M.Organisation.JustDoIt.Tag },
                    new Invocation { i = second.Id, v = second.Strategy.ObjectVersion, m = this.M.Organisation.JustDoIt.Tag },
                },
                o = new { i = true, c = true },
            });
            var request = codec == "SystemText"
                ? System.Text.Json.JsonSerializer.Deserialize<InvokeRequest>(raw)
                : Newtonsoft.Json.JsonConvert.DeserializeObject<InvokeRequest>(raw);
            var result = new Api(transaction, "Default", CancellationToken.None).Invoke(request);
            Assert.True(result.HasErrors);
            Assert.Equal(2, calls);
            this.Transaction.Rollback();
            Assert.Equal("Atomic first", first.Name);
        }

        [Theory]
        [InlineData("missing-target")]
        [InlineData("stale-version")]
        [InlineData("class-outside-workspace")]
        [InlineData("method-outside-workspace")]
        [InlineData("unknown-method")]
        [InlineData("denied-execute")]
        public void InvokeReadPreflightsEveryInvocationBeforeEffects(string failure)
        {
            var target = new WorkspaceXYObject1Builder(this.Transaction).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var first = new Invocation { i = this.x1.Id, v = this.x1.Strategy.ObjectVersion, m = this.M.WorkspaceXObject1.DoX.Tag };
            var second = new Invocation { i = target.Id, v = target.Strategy.ObjectVersion, m = this.M.WorkspaceXYObject1.DoX.Tag };
            switch (failure)
            {
                case "missing-target": second.i = 9007199254740991; break;
                case "stale-version": second.v++; break;
                case "class-outside-workspace": second.i = this.y1.Id; second.v = this.y1.Strategy.ObjectVersion; second.m = this.M.WorkspaceYObject1.DoY.Tag; break;
                case "method-outside-workspace": second.m = this.M.WorkspaceXYObject1.DoY.Tag; break;
                case "unknown-method": second.m = "unknown-method"; break;
            }
            var request = this.InvokeRequest(out var identity, "X", first, second);
            var services = new InvokeServices(this.M)
            {
                ExecutePermission = (id, _) => failure == "method-outside-workspace" ? true : failure == "denied-execute" && id == second.i ? false : null,
            };
            var response = this.ExecuteInvoke(request, identity, this.InvokeDatabase(services));
            Assert.Equal("rejected", response.invoke.k);
            Assert.Null(response.changed);
            Assert.Equal(0, services.Derivations);
            switch (failure)
            {
                case "missing-target": Assert.Equal(new[] { second.i }, response.invoke._m); break;
                case "stale-version": Assert.Equal(new[] { second.i }, response.invoke._v); break;
                case "unknown-method": Assert.Equal("The requested method is not available for this object in the selected workspace.", response.invoke._e); break;
                default: Assert.Equal(new[] { second.i }, response.invoke._a); break;
            }
        }

        [Theory]
        [InlineData("population")]
        [InlineData("user")]
        [InlineData("workspace")]
        [InlineData("revision")]
        [InlineData("context")]
        public void InvokeReadRejectsScopeMismatchBeforeEffects(string mismatch)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var calls = 0;
            var services = new InvokeServices(this.M);
            var database = this.InvokeDatabase(services, _ => calls++);
            switch (mismatch)
            {
                case "population": request.sc.p = Guid.NewGuid().ToString("D"); break;
                case "user": request.sc.u++; break;
                case "workspace": request.sc.w = "X"; break;
                case "revision": request.sc.r = "different"; break;
                case "context": request.sc.x = "different"; break;
            }
            var raw = JsonSerializer.Serialize(request);
            if (mismatch == "context") Assert.Throws<FormatException>(() => this.InvokeExecutor(database).Execute(raw, "Default", identity));
            else
            {
                var response = Allors.Protocol.Json.SystemTextJson.InvokeReadJson.DeserializeResponse(this.InvokeExecutor(database).Execute(raw, "Default", identity), request);
                Assert.Equal("rejected", response.invoke.k);
                Assert.Null(response.sc);
                Assert.Null(response.changed);
            }
            Assert.Equal(0, calls);
            Assert.Equal(0, services.Derivations);
            this.Transaction.Rollback();
            Assert.Equal("Before Invoke", target.Name);
        }

        [Theory]
        [InlineData("null")]
        [InlineData("uncommitted")]
        [InlineData("foreign-transaction")]
        [InlineData("deleted")]
        public void InvokeReadRejectsUncommittedOrUntrustedIdentity(string identityKind)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var calls = 0;
            var services = new InvokeServices(this.M);
            var database = this.InvokeDatabase(services, _ => calls++);
            var executor = this.InvokeExecutor(database);
            var foreign = (User)this.Transaction.Instantiate(identity.UserId);
            if (identityKind == "deleted")
            {
                foreign.Strategy.Delete();
                this.Transaction.Commit();
            }
            var raw = executor.Execute(JsonSerializer.Serialize(request), "Default", transaction => identityKind switch
            {
                "null" => null,
                "foreign-transaction" => foreign,
                "uncommitted" => new PersonBuilder(transaction).Build(),
                _ => transaction.Instantiate(identity.UserId) as User,
            });
            var response = Allors.Protocol.Json.SystemTextJson.InvokeReadJson.DeserializeResponse(raw, request);
            Assert.Equal("rejected", response.invoke.k);
            Assert.Null(response.changed);
            Assert.Equal(0, calls);
            Assert.Equal(0, services.Derivations);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InvokeReadPreservesEmptyAtomicBatch(bool refresh)
        {
            var request = this.InvokeRequest(out var identity);
            request.refresh = refresh;
            var services = new InvokeServices(this.M);
            var commits = 0;
            var response = this.ExecuteInvoke(request, identity, this.InvokeDatabase(services, commit: connection => { commits++; connection.Commit(); }, observeCommit: true));
            Assert.Equal(1, commits);
            Assert.Equal("committed", response.invoke.k);
            Assert.Equal(0, services.Derivations);
            Assert.Equal(refresh ? 2 : 1, services.Transactions);
            if (refresh)
            {
                var changed = Assert.IsType<PushReadChanged>(response.changed);
                Assert.Empty(changed.p);
                Assert.Empty(changed.s);
                Assert.Empty(changed.pm);
                Assert.Empty(changed.u);
            }
            else Assert.Null(response.changed);
        }

        [Theory]
        [InlineData("same-method-twice")]
        [InlineData("different-methods")]
        public void InvokeReadExecutesRepeatedTargetsInOrder(string kind)
        {
            var target = this.InvokeTarget();
            var originalDerived = target.JustDidItDerived;
            var one = this.Invocation(target);
            var two = this.Invocation(target);
            if (kind == "different-methods") two.m = this.M.Organisation.ToggleCanWrite.Tag;
            var request = this.InvokeRequest(out var identity, "Default", one, two);
            var observations = new List<bool?>();
            var order = new List<string>();
            var services = new InvokeServices(this.M);
            var database = this.InvokeDatabase(services, value =>
            {
                order.Add("invoke");
                observations.Add(value.JustDidItDerived);
                value.JustDidIt = true;
            }, toggle: value =>
            {
                order.Add("toggle");
                observations.Add(value.JustDidItDerived);
                Assert.True(value.JustDidItDerived);
            });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("committed", response.invoke.k);
            Assert.Equal(2, services.Derivations);
            Assert.Equal(new[] { originalDerived, (bool?)true }, observations);
            Assert.Equal(kind == "same-method-twice" ? new[] { "invoke", "invoke" } : new[] { "invoke", "toggle" }, order);
            this.Transaction.Rollback();
            Assert.True(target.JustDidIt);
            Assert.True(target.JustDidItDerived);
        }

        [Theory]
        [InlineData("method-exception")]
        [InlineData("later-target-deleted")]
        [InlineData("derivation-rejection")]
        [InlineData("later-execute-revoked")]
        public void InvokeReadRollsBackTheWholeBatchAfterLaterFailure(string failure)
        {
            var messages = failure == "method-exception" ? new[] { "The later method failed.", null, string.Empty, " ", "\t" } : failure == "derivation-rejection" ? new[] { "Reject the derived batch.", null } : new[] { "The later method failed." };
            foreach (var message in messages)
            {
                var first = this.InvokeTarget("First");
                var second = this.InvokeTarget("Second");
                var previous = new TrimToBuilder(this.Transaction).WithName("Previous").Build();
                var next = new TrimToBuilder(this.Transaction).WithName("Next").Build();
                var source = new TrimFromBuilder(this.Transaction).WithName("Source").WithMany2One(previous).Build();
                var removed = new TrimToBuilder(this.Transaction).WithName("Retained").Build();
                this.Transaction.Derive();
                this.Transaction.Commit();
                var request = this.InvokeRequest(out var identity, "Default", this.Invocation(first), this.Invocation(second));
                var calls = 0;
                long created = 0;
                var services = new InvokeServices(this.M, validationError: failure == "derivation-rejection" ? "Reject the derived batch." : null, emptyValidationErrors: failure == "derivation-rejection" && message == null);
                var database = this.InvokeDatabase(services, value =>
                {
                    calls++;
                    var transaction = value.Strategy.Transaction;
                    if (value.Id == first.Id)
                    {
                        value.Name = "Pending first";
                        ((TrimFrom)transaction.Instantiate(source.Id)).Many2One = (TrimTo)transaction.Instantiate(next.Id);
                        created = new TrimToBuilder(transaction).WithName("Rolled back creation").Build().Id;
                        transaction.Instantiate(removed.Id).Strategy.Delete();
                        if (failure == "later-target-deleted") transaction.Instantiate(second.Id).Strategy.Delete();
                        if (failure == "later-execute-revoked")
                        {
                            var permission = new Permissions(transaction).Get(this.M.Organisation, this.M.Organisation.JustDoIt);
                            ((Organisation)transaction.Instantiate(second.Id)).AddRevocation(new RevocationBuilder(transaction).WithDeniedPermission(permission).Build());
                        }
                    }
                    else throw new InvokeFailure(message);
                });
                var response = this.ExecuteInvoke(request, identity, database);
                Assert.Equal("rejected", response.invoke.k);
                Assert.Null(response.changed);
                Assert.Equal(failure == "method-exception" ? 2 : 1, calls);
                if (failure == "later-execute-revoked") Assert.Equal(new[] { second.Id }, response.invoke._a);
                if (failure == "later-target-deleted") Assert.Equal(new[] { second.Id }, response.invoke._m);
                if (failure == "derivation-rejection")
                {
                    if (message == null) Assert.Equal("The invocation derivation failed. Inspect the server diagnostics and correct the validation failure before trying a new batch.", response.invoke._e);
                    else Assert.NotEmpty(response.invoke._d);
                }
                if (failure == "method-exception") Assert.Equal(string.IsNullOrWhiteSpace(message) ? "The invoked method failed. Inspect the server diagnostics before trying a new batch." : message, response.invoke._e);
                this.Transaction.Rollback();
                Assert.Equal("First", first.Name);
                Assert.NotNull(this.Transaction.Instantiate(second.Id));
                Assert.Equal(previous.Id, source.Many2One.Id);
                Assert.NotNull(this.Transaction.Instantiate(removed.Id));
                Assert.Null(this.Transaction.Instantiate(created));
            }
        }

        [Theory]
        [InlineData("commit")]
        [InlineData("rollback")]
        public void InvokeReadForbidsMethodTransactionBoundaries(string boundary)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var attempts = 0;
            var services = new InvokeServices(this.M);
            var database = this.InvokeDatabase(services, value =>
            {
                value.Name = "Must roll back";
                attempts++;
                Assert.Throws<InvalidOperationException>(() =>
                {
                    if (boundary == "commit") value.Strategy.Transaction.Commit();
                    else value.Strategy.Transaction.Rollback();
                });
            });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("rejected", response.invoke.k);
            Assert.Null(response.changed);
            Assert.Equal(1, attempts);
            this.Transaction.Rollback();
            Assert.Equal("Before Invoke", target.Name);
        }

        [Theory]
        [InlineData("switch-user")]
        [InlineData("switch-and-restore")]
        public void InvokeReadRejectsAnyAdmittedUserChange(string change)
        {
            var first = this.InvokeTarget();
            var second = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(first), this.Invocation(second));
            var calls = 0;
            var services = new InvokeServices(this.M);
            var database = this.InvokeDatabase(services, value =>
            {
                calls++;
                value.Name = "Must roll back";
                var userService = value.Strategy.Transaction.Services.Get<Allors.Database.Services.IUserService>();
                var original = userService.User;
                userService.User = null;
                if (change == "switch-and-restore") userService.User = original;
            });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("rejected", response.invoke.k);
            Assert.Equal(1, calls);
            Assert.Equal(0, services.Derivations);
            Assert.Null(response.changed);
            this.Transaction.Rollback();
            Assert.Equal("Before Invoke", first.Name);
        }

        [Fact]
        public void InvokeReadRefreshesTargetsAndCumulativeAffectedObjects()
        {
            var target = this.InvokeTarget();
            var previous = new TrimToBuilder(this.Transaction).WithName("Previous").Build();
            var next = new TrimToBuilder(this.Transaction).WithName("Next").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").WithMany2One(previous).Build();
            var removed = new TrimToBuilder(this.Transaction).WithName("Removed").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            long created = 0, hidden = 0;
            var services = new InvokeServices(this.M, transaction =>
            {
                created = new TrimToBuilder(transaction).WithName("Derived creation").Build().Id;
                hidden = new WorkspaceNoneObject1Builder(transaction).Build().Id;
                transaction.Instantiate(removed.Id).Strategy.Delete();
                transaction.Checkpoint();
            });
            var database = this.InvokeDatabase(services, value =>
            {
                ((TrimFrom)value.Strategy.Transaction.Instantiate(source.Id)).Many2One = (TrimTo)value.Strategy.Transaction.Instantiate(next.Id);
                value.Strategy.Transaction.Checkpoint();
            });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("committed", response.invoke.k);
            var changed = Assert.IsType<PushReadChanged>(response.changed);
            var ids = changed.p.Select(value => value.i).ToArray();
            foreach (var id in new[] { target.Id, previous.Id, next.Id, source.Id, created }) Assert.Contains(id, ids);
            Assert.DoesNotContain(hidden, ids);
            Assert.Contains(removed.Id, changed.u);
            this.Transaction.Rollback();
            Assert.Equal(next.Id, source.Many2One.Id);
            Assert.Null(this.Transaction.Instantiate(removed.Id));
            Assert.NotNull(this.Transaction.Instantiate(created));
            Assert.Equal("Before Invoke", target.Name);
        }

        [Fact]
        public void InvokeReadRefreshPreservesCurrentSecurityAndRevocations()
        {
            var target = this.InvokeTarget("Private name");
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            long revocationId = 0, readPermissionId = 0, executePermissionId = 0;
            var services = new InvokeServices(this.M);
            var database = this.InvokeDatabase(services, value =>
            {
                var transaction = value.Strategy.Transaction;
                var read = new Permissions(transaction).Get(this.M.Organisation, this.M.Organisation.Name, Allors.Operations.Read);
                var execute = new Permissions(transaction).Get(this.M.Organisation, this.M.Organisation.JustDoIt);
                var revocation = new RevocationBuilder(transaction).WithDeniedPermission(read).WithDeniedPermission(execute).Build();
                value.AddRevocation(revocation);
                revocationId = revocation.Id; readPermissionId = read.Id; executePermissionId = execute.Id;
            });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("committed", response.invoke.k);
            using var document = JsonDocument.Parse(JsonSerializer.Serialize(response.changed));
            var changed = document.RootElement;
            var record = changed.GetProperty("p").EnumerateArray().Single(value => value.GetProperty("i").GetInt64() == target.Id);
            Assert.DoesNotContain(record.GetProperty("p").GetProperty("ro").EnumerateArray(), value => value.GetProperty("t").GetString() == this.M.Organisation.Name.RelationType.Tag);
            Assert.Contains(changed.GetProperty("s").EnumerateArray(), value => value.GetProperty("k").GetString() == "revocation" && value.GetProperty("i").GetInt64() == revocationId && value.GetProperty("p").ValueKind == JsonValueKind.Object);
            Assert.Contains(changed.GetProperty("pm").EnumerateArray(), value => value.GetProperty("i").GetInt64() == readPermissionId);
            Assert.Contains(changed.GetProperty("pm").EnumerateArray(), value => value.GetProperty("i").GetInt64() == executePermissionId);
        }

        [Fact]
        public void InvokeReadSkipRefreshOpensNoPostCommitSnapshot()
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            request.refresh = false;
            var services = new InvokeServices(this.M);
            var response = this.ExecuteInvoke(request, identity, this.InvokeDatabase(services, value => value.Name = "Committed"));
            Assert.Equal("committed", response.invoke.k);
            Assert.Null(response.changed);
            Assert.Equal(1, services.Transactions);
            Assert.Equal(1, services.Derivations);
            this.Transaction.Rollback();
            Assert.Equal("Committed", target.Name);
        }

        [Theory]
        [InlineData("read-failure")]
        [InlineData("read-cleanup")]
        [InlineData("affected-count-limit")]
        [InlineData("response-byte-limit")]
        public void InvokeReadKeepsCommitWhenRefreshFails(string failure)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var services = new InvokeServices(this.M,
                read: (_, _) => { if (failure == "read-failure") throw new InvalidOperationException("Private refresh failure."); },
                dispose: (attempt, _) => { if (failure == "read-cleanup" && attempt == 2) throw new InvalidOperationException("Private refresh cleanup failure."); });
            var database = this.InvokeDatabase(services, value =>
            {
                value.Name = "Committed despite refresh failure";
                var transaction = value.Strategy.Transaction;
                if (failure == "affected-count-limit") transaction.Create(this.M.TrimTo, 10001);
                if (failure == "response-byte-limit") new C1Builder(transaction).WithAllorsStringMax(new string('x', InvokeReadExecutor.MaxResponseBytes)).Build();
            });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("committed", response.invoke.k);
            Assert.IsType<ReadFailure>(response.changed);
            var raw = JsonSerializer.Serialize(response);
            Assert.True(System.Text.Encoding.UTF8.GetByteCount(raw) < 4096);
            Assert.DoesNotContain("Private", raw);
            Assert.Equal(1, services.Derivations);
            this.Transaction.Rollback();
            Assert.Equal("Committed despite refresh failure", target.Name);
        }

        [Theory]
        [InlineData("before-commit")]
        [InlineData("unknown-after-actual-commit")]
        [InlineData("after-trace")]
        [InlineData("write-cleanup")]
        public void InvokeReadPreservesCommitClassificationAcrossFailures(string failure)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var calls = 0;
            var commits = 0;
            var services = new InvokeServices(this.M, dispose: (attempt, _) =>
            {
                if (failure == "write-cleanup" && attempt == 1) throw new InvalidOperationException("Private write cleanup failure.");
            });
            Action<Allors.Database.Adapters.Sql.IConnection> commit = failure == "unknown-after-actual-commit" ? connection =>
            {
                commits++;
                connection.Commit();
                throw new InvalidOperationException("Private lost commit acknowledgement.");
            } : null;
            var database = (Allors.Database.Adapters.Sql.Database)this.InvokeDatabase(services, value =>
            {
                calls++;
                value.Name = "Written once";
                if (failure == "before-commit") throw new InvalidOperationException("Method failed before commit.");
            }, commit);
            if (failure == "after-trace") database.Sink = new InvokeSink(after: _ => throw new InvalidOperationException("Private after hook failure."));
            var response = this.ExecuteInvoke(request, identity, database);
            var expected = failure == "before-commit" ? "rejected" : failure == "unknown-after-actual-commit" ? "unknown" : "committed";
            Assert.Equal(expected, response.invoke.k);
            Assert.Equal(1, calls);
            Assert.Equal(failure == "unknown-after-actual-commit" ? 1 : 0, commits);
            if (expected == "committed") { Assert.NotEmpty(response.invoke.e); Assert.IsType<ReadFailure>(response.changed); }
            else Assert.Null(response.changed);
            this.Transaction.Rollback();
            Assert.Equal(failure == "before-commit" ? "Before Invoke" : "Written once", target.Name);
        }

        [Fact]
        public void InvokeReadTracingCannotRewriteCapturedRequestOrReceipt()
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var calls = 0;
            var services = new InvokeServices(this.M);
            var database = (Allors.Database.Adapters.Sql.Database)this.InvokeDatabase(services, value => { calls++; value.Name = "Original batch"; });
            database.Sink = new InvokeSink(
                before: trace => { trace.InvokeRequest.l[0].i = 9007199254740991; trace.InvokeRequest.l[0].v = 1; trace.InvokeRequest.l[0].m = "unknown"; trace.InvokeRequest.l = Array.Empty<Invocation>(); },
                after: trace => { trace.InvokeResponse._e = "Forged rejection"; trace.InvokeResponse._v = new[] { target.Id }; });
            var response = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("committed", response.invoke.k);
            Assert.Null(response.invoke._e);
            Assert.Null(response.invoke._v);
            Assert.IsType<PushReadChanged>(response.changed);
            Assert.Equal(target.Id, Assert.Single(request.l).i);
            Assert.Equal(1, calls);
            Assert.Equal(1, services.Derivations);
            this.Transaction.Rollback();
            Assert.Equal("Original batch", target.Name);
        }

        [Theory]
        [InlineData("before-admission")]
        [InlineData("during-method")]
        [InlineData("after-commit")]
        public void InvokeReadCancellationPreservesDispatchAndCommitEvidence(string timing)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            using var cancellation = new CancellationTokenSource();
            var calls = 0;
            var services = new InvokeServices(this.M);
            var database = (Allors.Database.Adapters.Sql.Database)this.InvokeDatabase(services, value =>
            {
                calls++; value.Name = "Cancellation boundary";
                if (timing == "during-method") cancellation.Cancel();
            });
            if (timing == "after-commit") database.Sink = new InvokeSink(after: _ => cancellation.Cancel());
            if (timing == "before-admission")
            {
                cancellation.Cancel();
                Assert.Throws<OperationCanceledException>(() => this.ExecuteInvoke(request, identity, database, cancellation.Token));
                Assert.Equal(0, calls);
                Assert.Equal(0, services.Transactions);
            }
            else
            {
                var response = this.ExecuteInvoke(request, identity, database, cancellation.Token);
                Assert.Equal(timing == "after-commit" ? "committed" : "rejected", response.invoke.k);
                Assert.Equal(1, calls);
                if (timing == "after-commit") Assert.IsType<ReadFailure>(response.changed);
                else Assert.Null(response.changed);
            }
            this.Transaction.Rollback();
            Assert.Equal(timing == "after-commit" ? "Cancellation boundary" : "Before Invoke", target.Name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async System.Threading.Tasks.Task InvokeReadLocalTransportRunsOneProtectedAttempt(bool typed)
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            var calls = 0;
            var services = new InvokeServices(this.M);
            var transport = new Allors.Workspace.Adapters.Local.InvokeReadTransport(this.InvokeExecutor(this.InvokeDatabase(services, value => { calls++; value.Name = "Local protected Invoke"; })), "Default", identity);
            var response = typed ? await transport.InvokeRead(request) : await transport.InvokeRead(JsonSerializer.Serialize(request));
            Assert.Equal("committed", response.invoke.k);
            Assert.IsType<PushReadChanged>(response.changed);
            Assert.Equal(1, calls);
            Assert.Equal(1, services.Derivations);
            Assert.Equal(2, services.Transactions);
            this.Transaction.Rollback();
            Assert.Equal("Local protected Invoke", target.Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task InvokeReadControllerRequiresConfigurationBeforeBodyRead()
        {
            var workspace = new InvokeReadWorkspace();
            var resolver = new InvokeReadResolver(1);
            var controller = new InvokeReadController(workspace, resolver)
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() },
            };
            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(await controller.Post(CancellationToken.None));
            Assert.Equal(503, result.StatusCode);
            Assert.Equal(0, workspace.Reads);
            Assert.Equal(0, resolver.Resolutions);
        }

        [Theory]
        [InlineData("duplicate-json")]
        [InlineData("oversized")]
        [InlineData("invalid-utf8")]
        public async System.Threading.Tasks.Task InvokeReadControllerRejectsInvalidBodyBeforeUserResolution(string invalid)
        {
            this.InvokeRequest(out var identity, "X");
            var bytes = invalid == "invalid-utf8" ? new byte[] { 0xc3, 0x28 } : System.Text.Encoding.UTF8.GetBytes(invalid == "oversized" ? new string(' ', InvokeReadExecutor.MaxRequestBytes + 1) : "{\"v\":1,\"v\":1}");
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "original") }, "Tests"));
            context.Request.Body = new System.IO.MemoryStream(bytes);
            var resolver = new InvokeReadResolver(identity.UserId);
            var controller = new InvokeReadController(new InvokeReadWorkspace(), resolver, this.InvokeExecutor(workspace: "X"))
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = context },
            };
            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(await controller.Post(CancellationToken.None), exactMatch: false);
            Assert.Equal(invalid == "oversized" ? 413 : 400, result.StatusCode);
            Assert.Equal(0, resolver.Resolutions);
        }

        [Fact]
        public async System.Threading.Tasks.Task InvokeReadControllerCapturesTrustedInputsBeforeBodyRead()
        {
            var request = this.InvokeRequest(out var identity, "X", new Invocation { i = this.x1.Id, v = this.x1.Strategy.ObjectVersion, m = this.M.WorkspaceXObject1.DoX.Tag });
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "original") }, "Tests"));
            context.Request.Body = new InvokeReadIdentityMutatingStream(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)), () =>
                context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "changed") }, "Changed")));
            var resolver = new InvokeReadResolver(identity.UserId);
            var workspace = new InvokeReadWorkspace();
            var controller = new InvokeReadController(workspace, resolver, this.InvokeExecutor(workspace: "X"))
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = context },
            };
            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ContentResult>(await controller.Post(CancellationToken.None));
            var response = Allors.Protocol.Json.SystemTextJson.InvokeReadJson.DeserializeResponse(result.Content, request);
            Assert.Equal("committed", response.invoke.k);
            Assert.IsType<PushReadChanged>(response.changed);
            Assert.Equal(1, workspace.Reads);
            Assert.Equal(2, resolver.Resolutions);
        }

        [Fact]
        public async System.Threading.Tasks.Task InvokeReadPopulationFenceCoversAdmissionThroughCommit()
        {
            var target = this.InvokeTarget();
            var request = this.InvokeRequest(out var identity, "Default", this.Invocation(target));
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            var services = new InvokeServices(this.M);
            var database = (Allors.Database.Adapters.Sql.Database)this.InvokeDatabase(services, value =>
            {
                value.Name = "Fenced write";
                entered.Set();
                Assert.True(release.Wait(TimeSpan.FromSeconds(10)));
            });
            var invoking = System.Threading.Tasks.Task.Run(() => this.ExecuteInvoke(request, identity, database));
            Assert.True(entered.Wait(TimeSpan.FromSeconds(10)));
            using var renewing = new ManualResetEventSlim();
            var renewal = System.Threading.Tasks.Task.Run(() => { renewing.Set(); return database.PopulationLifecycle.Renew(); });
            try
            {
                Assert.True(renewing.Wait(TimeSpan.FromSeconds(10)));
                Assert.NotSame(renewal, await System.Threading.Tasks.Task.WhenAny(renewal, System.Threading.Tasks.Task.Delay(100)));
            }
            finally { release.Set(); }
            var response = await invoking;
            Assert.Equal("committed", response.invoke.k);
            var next = await renewal;
            Assert.NotEqual(identity.Population, next.Incarnation);
            var rejected = this.ExecuteInvoke(request, identity, database);
            Assert.Equal("rejected", rejected.invoke.k);
            Assert.Equal(1, services.Derivations);
            this.Transaction.Rollback();
            Assert.Equal("Fenced write", target.Name);
        }

        [Fact]
        public void InvokeReadRequiresProtectedSqlScopes()
        {
            var services = new Allors.Database.Configuration.DefaultDatabaseServices(new Allors.Database.Configuration.Derivations.Default.Engine(Rules.Create(this.M)));
            var memory = new Allors.Database.Adapters.Memory.Database(services, new Allors.Database.Adapters.Memory.Configuration
            {
                ObjectFactory = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).ObjectFactory,
            });
            var exception = Assert.Throws<NotSupportedException>(() => this.InvokeExecutor(memory));
            Assert.Contains("SQL", exception.Message);
        }

        private Allors.Database.IDatabase InvokeDatabase(InvokeServices services, Action<Organisation> invoke = null, Action<Allors.Database.Adapters.Sql.IConnection> commit = null, bool observeCommit = false, Action<Organisation> toggle = null)
        {
            var existing = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            var configuration = new Allors.Database.Adapters.Sql.Configuration
            {
                ObjectFactory = invoke == null ? existing.ObjectFactory : new InvokeObjectFactory(existing.ObjectFactory, invoke, toggle),
                ConnectionString = existing.ConnectionString, SchemaName = existing.SchemaName,
            };
            if (commit != null)
            {
                return existing is Allors.Database.Adapters.Sql.Npgsql.Database
                    ? new InvokeCommitNpgsqlDatabase(services, configuration, commit, observeCommit)
                    : new InvokeCommitSqlClientDatabase(services, configuration, commit, observeCommit);
            }
            return existing is Allors.Database.Adapters.Sql.Npgsql.Database
                ? new Allors.Database.Adapters.Sql.Npgsql.Database(services, configuration)
                : new Allors.Database.Adapters.Sql.SqlClient.Database(services, configuration);
        }

        private Organisation InvokeTarget(string name = "Before Invoke")
        {
            var target = new OrganisationBuilder(this.Transaction).WithName(name).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            return target;
        }

        private Invocation Invocation(Organisation target) => new Invocation { i = target.Id, v = target.Strategy.ObjectVersion, m = this.M.Organisation.JustDoIt.Tag };

        private InvokeReadRequest InvokeRequest(out LocalReadIdentity identity, string workspace = "Default", params Invocation[] invocations)
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            identity = new LocalReadIdentity(population.Incarnation, user.Id);
            return new InvokeReadRequest
            {
                v = 1, i = "invoke-read-server-test", refresh = true, l = invocations,
                sc = new ReadScope { p = population.Incarnation.ToString("D"), u = user.Id, w = workspace, r = "revision-1" },
            };
        }

        private InvokeReadExecutor InvokeExecutor(Allors.Database.IDatabase database = null, string workspace = "Default") =>
            new InvokeReadExecutor(database ?? this.Transaction.Database, new ReadConfiguration(new Dictionary<string, string> { [workspace] = "revision-1" }));

        private InvokeReadResponse ExecuteInvoke(InvokeReadRequest request, LocalReadIdentity identity, Allors.Database.IDatabase database = null, CancellationToken cancellationToken = default)
        {
            var raw = this.InvokeExecutor(database, request.sc.w).Execute(JsonSerializer.Serialize(request), request.sc.w, identity, cancellationToken);
            var response = Allors.Protocol.Json.SystemTextJson.InvokeReadJson.DeserializeResponse(raw, request);
            var other = Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeResponse(raw, request);
            Assert.Equal(response.invoke.k, other.invoke.k);
            return response;
        }

        private sealed class InvokeEmptyDerivation : Allors.Database.Derivations.IDerivation
        {
            private readonly Allors.Database.Derivations.IDerivation inner;
            internal InvokeEmptyDerivation(Allors.Database.Derivations.IDerivation inner) => this.inner = inner;
            public Guid Id => this.inner.Id;
            public DateTime TimeStamp => this.inner.TimeStamp;
            public Allors.Database.ITransaction Transaction => this.inner.Transaction;
            public Allors.Database.Derivations.IAccumulatedChangeSet ChangeSet => this.inner.ChangeSet;
            public Allors.Database.Derivations.IValidation Validation => this.inner.Validation;
            public object this[string key] { get => this.inner[key]; set => this.inner[key] = value; }
            public Allors.Database.Derivations.IValidation Derive() => new InvokeEmptyValidation(this.inner.Derive());
        }

        // A custom derivation reports failure without exposing detailed errors. Its failure bit
        // remains authoritative; missing diagnostics must never turn it into a successful commit.
        private sealed class InvokeEmptyValidation : Allors.Database.Derivations.IValidation
        {
            private readonly Allors.Database.Derivations.IValidation inner;
            internal InvokeEmptyValidation(Allors.Database.Derivations.IValidation inner) => this.inner = inner;
            public bool HasErrors => true;
            public Allors.Database.Derivations.IDerivationError[] Errors => Array.Empty<Allors.Database.Derivations.IDerivationError>();
            public void AddError(Allors.Database.Derivations.IDerivationError derivationError) => this.inner.AddError(derivationError);
            public void AddError(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType, string errorMessage, params object[] messageParam) => this.inner.AddError(association, roleType, errorMessage, messageParam);
            public void AddError(Allors.Database.IObject role, Allors.Database.Meta.IAssociationType associationType, string errorMessage, params object[] messageParam) => this.inner.AddError(role, associationType, errorMessage, messageParam);
            public void AddError(string error) => this.inner.AddError(error);
            public void AssertExists(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType) => this.inner.AssertExists(association, roleType);
            public void AssertNotExists(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType) => this.inner.AssertNotExists(association, roleType);
            public void AssertNonEmptyString(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType) => this.inner.AssertNonEmptyString(association, roleType);
            public void AssertNonWhiteSpaceString(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType) => this.inner.AssertNonWhiteSpaceString(association, roleType);
            public void AssertExistsNonEmptyString(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType) => this.inner.AssertExistsNonEmptyString(association, roleType);
            public void AssertIsUnique(Allors.Database.IChangeSet changeSet, Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType) => this.inner.AssertIsUnique(changeSet, association, roleType);
            public void AssertIsUnique(Allors.Database.IChangeSet changeSet, Allors.Database.IObject association, Allors.Database.Meta.IComposite objectType, params Allors.Database.Meta.IRoleType[] roleTypes) => this.inner.AssertIsUnique(changeSet, association, objectType, roleTypes);
            public void AssertAtLeastOne(Allors.Database.IObject association, params Allors.Database.Meta.IRoleType[] roleTypes) => this.inner.AssertAtLeastOne(association, roleTypes);
            public void AssertExistsAtMostOne(Allors.Database.IObject association, params Allors.Database.Meta.IRoleType[] roleTypes) => this.inner.AssertExistsAtMostOne(association, roleTypes);
            public void AssertAreEqual(Allors.Database.IObject association, Allors.Database.Meta.IRoleType roleType, Allors.Database.Meta.IRoleType otherRoleType) => this.inner.AssertAreEqual(association, roleType, otherRoleType);
            public void AssertExists(Allors.Database.IObject role, Allors.Database.Meta.IAssociationType associationType) => this.inner.AssertExists(role, associationType);
        }

        private sealed class InvokeFailure : Exception
        {
            private readonly string message;
            internal InvokeFailure(string message) => this.message = message;
            public override string Message => this.message;
        }

        private sealed class InvokeSink : ISink
        {
            private readonly Action<InvokeEvent> before;
            private readonly Action<InvokeEvent> after;
            internal InvokeSink(Action<InvokeEvent> before = null, Action<InvokeEvent> after = null) { this.before = before; this.after = after; }
            public void OnBefore(IEvent value) { if (value is InvokeEvent invocation) this.before?.Invoke(invocation); }
            public void OnAfter(IEvent value) { if (value is InvokeEvent invocation) this.after?.Invoke(invocation); }
        }

        private sealed class InvokeServices : Allors.Database.IDatabaseServices, Allors.Database.Derivations.IDerivationService
        {
            private readonly Allors.Database.Configuration.DefaultDatabaseServices inner;
            private readonly Action<Allors.Database.ITransaction> derive;
            private readonly Action<long, Allors.Database.ITransaction> read;
            private readonly Action<int, Allors.Database.ITransaction> dispose;
            private readonly string validationError;
            private readonly bool emptyValidationErrors;
            internal Func<long, Allors.Database.Meta.IMethodType, bool?> ExecutePermission { get; set; }

            internal InvokeServices(Allors.Database.Meta.MetaPopulation meta, Action<Allors.Database.ITransaction> derive = null,
                Action<long, Allors.Database.ITransaction> read = null, Action<int, Allors.Database.ITransaction> dispose = null, string validationError = null, bool emptyValidationErrors = false)
            {
                this.inner = new Allors.Database.Configuration.DefaultDatabaseServices(new Allors.Database.Configuration.Derivations.Default.Engine(Rules.Create(meta)));
                this.derive = derive;
                this.read = read;
                this.dispose = dispose;
                this.validationError = validationError;
                this.emptyValidationErrors = emptyValidationErrors;
            }

            internal int Transactions { get; private set; }
            internal int Derivations { get; private set; }
            public void OnInit(Allors.Database.IDatabase database) => this.inner.OnInit(database);
            public T Get<T>() => typeof(T) == typeof(Allors.Database.Derivations.IDerivationService) ? (T)(object)this : this.inner.Get<T>();
            public void Dispose() => this.inner.Dispose();
            public Allors.Database.ITransactionServices CreateTransactionServices() => new TransactionServices(this, this.inner.CreateTransactionServices());
            public Allors.Database.Derivations.IDerivation CreateDerivation(Allors.Database.ITransaction transaction, bool continueOnError = false)
            {
                this.Derivations++;
                this.derive?.Invoke(transaction);
                var derivation = this.inner.Get<Allors.Database.Derivations.IDerivationService>().CreateDerivation(transaction, continueOnError);
                if (this.validationError != null) { derivation.Validation.AddError(this.validationError); }
                return this.emptyValidationErrors ? new InvokeEmptyDerivation(derivation) : derivation;
            }

            private sealed class TransactionServices : Allors.Database.ITransactionServices, Allors.Database.Services.IWorkspaceAclsService
            {
                private readonly InvokeServices owner;
                private readonly Allors.Database.ITransactionServices inner;
                private Allors.Database.ITransaction transaction;
                private int attempt;
                internal TransactionServices(InvokeServices owner, Allors.Database.ITransactionServices inner) { this.owner = owner; this.inner = inner; }
                public void OnInit(Allors.Database.ITransaction value)
                {
                    this.attempt = ++this.owner.Transactions;
                    this.transaction = value;
                    this.inner.OnInit(value);
                }
                public T Get<T>() => typeof(T) == typeof(Allors.Database.Services.IWorkspaceAclsService) ? (T)(object)this : this.inner.Get<T>();
                public Allors.Database.Security.IAccessControl Create(string workspaceName) => new AccessControl(this,
                    this.inner.Get<Allors.Database.Services.IWorkspaceAclsService>().Create(workspaceName));
                public void Dispose()
                {
                    this.inner.Dispose();
                    this.owner.dispose?.Invoke(this.attempt, this.transaction);
                }

                private sealed class AccessControl : Allors.Database.Security.IAccessControl
                {
                    private readonly TransactionServices owner;
                    private readonly Allors.Database.Security.IAccessControl inner;
                    internal AccessControl(TransactionServices owner, Allors.Database.Security.IAccessControl inner) { this.owner = owner; this.inner = inner; }
                    public Allors.Database.Security.IAccessControlList this[Allors.Database.IObject value] => new Acl(this.owner, value.Id, this.inner[value]);
                }

                private sealed class Acl : Allors.Database.Security.IAccessControlList
                {
                    private readonly TransactionServices owner;
                    private readonly long id;
                    private readonly Allors.Database.Security.IAccessControlList inner;
                    internal Acl(TransactionServices owner, long id, Allors.Database.Security.IAccessControlList inner) { this.owner = owner; this.id = id; this.inner = inner; }
                    public Allors.Database.Domain.IVersionedGrant[] Grants
                    {
                        get { this.owner.owner.read?.Invoke(this.id, this.owner.transaction); return this.inner.Grants; }
                    }
                    public Allors.Database.Domain.IVersionedRevocation[] Revocations => this.inner.Revocations;
                    public bool CanRead(Allors.Database.Meta.IRoleType role) => this.inner.CanRead(role);
                    public bool CanWrite(Allors.Database.Meta.IRoleType role) => this.inner.CanWrite(role);
                    public bool CanExecute(Allors.Database.Meta.IMethodType method) => this.owner.owner.ExecutePermission?.Invoke(this.id, method) ?? this.inner.CanExecute(method);
                }
            }
        }

        private sealed class InvokeCommitNpgsqlDatabase : Allors.Database.Adapters.Sql.Npgsql.Database
        {
            private readonly Action<Allors.Database.Adapters.Sql.IConnection> commit;
            private readonly bool observeCommit;
            internal InvokeCommitNpgsqlDatabase(Allors.Database.IDatabaseServices services, Allors.Database.Adapters.Sql.Configuration configuration,
                Action<Allors.Database.Adapters.Sql.IConnection> commit, bool observeCommit) : base(services, configuration)
            {
                this.commit = commit; this.observeCommit = observeCommit;
            }
            public override Allors.Database.Adapters.Sql.WriteScope OpenWrite(Guid expectedIncarnation)
            {
                var connection = this.ConnectionFactory.Create();
                return this.CreateWriteScope(connection, this.PopulationLifecycle.RequireReady(), expectedIncarnation, observed =>
                {
                    this.commit(connection);
                    if (this.observeCommit) observed();
                });
            }
        }

        private sealed class InvokeCommitSqlClientDatabase : Allors.Database.Adapters.Sql.SqlClient.Database
        {
            private readonly Action<Allors.Database.Adapters.Sql.IConnection> commit;
            private readonly bool observeCommit;
            internal InvokeCommitSqlClientDatabase(Allors.Database.IDatabaseServices services, Allors.Database.Adapters.Sql.Configuration configuration,
                Action<Allors.Database.Adapters.Sql.IConnection> commit, bool observeCommit) : base(services, configuration)
            {
                this.commit = commit; this.observeCommit = observeCommit;
            }
            public override Allors.Database.Adapters.Sql.WriteScope OpenWrite(Guid expectedIncarnation)
            {
                var connection = this.ConnectionFactory.Create();
                return this.CreateWriteScope(connection, this.PopulationLifecycle.RequireReady(), expectedIncarnation, observed =>
                {
                    this.commit(connection);
                    if (this.observeCommit) observed();
                });
            }
        }

        private sealed class InvokeReadWorkspace : Allors.Services.IWorkspaceService
        {
            internal int Reads { get; private set; }
            public string Name => ++this.Reads == 1 ? "X" : "changed-workspace";
        }

        private sealed class InvokeReadResolver : Allors.Services.IUserResolver
        {
            private readonly long userId;
            internal InvokeReadResolver(long userId) => this.userId = userId;
            internal int Resolutions { get; private set; }
            public User Resolve(System.Security.Claims.ClaimsPrincipal principal, Allors.Database.ITransaction transaction)
            {
                this.Resolutions++;
                Assert.Equal("original", principal.FindFirst("user").Value);
                return (User)transaction.Instantiate(this.userId);
            }
        }

        private sealed class InvokeReadIdentityMutatingStream : System.IO.MemoryStream
        {
            private readonly Action mutate;
            internal InvokeReadIdentityMutatingStream(byte[] bytes, Action mutate) : base(bytes) => this.mutate = mutate;
            public override System.Threading.Tasks.ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                this.mutate();
                return base.ReadAsync(buffer, cancellationToken);
            }
        }

        private Allors.Database.IDatabase InvokeDatabase(Allors.Database.IObjectFactory factory)
        {
            var existing = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            var services = new Allors.Database.Configuration.DefaultDatabaseServices(new Allors.Database.Configuration.Derivations.Default.Engine(Rules.Create(this.M)));
            var configuration = new Allors.Database.Adapters.Sql.Configuration
            {
                ObjectFactory = factory, ConnectionString = existing.ConnectionString, SchemaName = existing.SchemaName,
            };
            return existing is Allors.Database.Adapters.Sql.Npgsql.Database
                ? new Allors.Database.Adapters.Sql.Npgsql.Database(services, configuration)
                : new Allors.Database.Adapters.Sql.SqlClient.Database(services, configuration);
        }

        private sealed class InvokeObjectFactory : Allors.Database.IObjectFactory
        {
            private readonly Allors.Database.IObjectFactory inner;
            private readonly System.Action<Organisation> invoke;
            private readonly System.Action<Organisation> toggle;
            internal InvokeObjectFactory(Allors.Database.IObjectFactory inner, System.Action<Organisation> invoke, System.Action<Organisation> toggle = null) { this.inner = inner; this.invoke = invoke; this.toggle = toggle; }
            public string Namespace => this.inner.Namespace;
            public System.Reflection.Assembly Assembly => this.inner.Assembly;
            public Allors.Database.Meta.IMetaPopulation MetaPopulation => this.inner.MetaPopulation;
            public Allors.Database.IObject Create(Allors.Database.IStrategy strategy) => strategy.Class.Name == "Organisation"
                ? new InvokeOrganisation(strategy, this.invoke, this.toggle) : this.inner.Create(strategy);
            public Allors.Database.Meta.IObjectType GetObjectType<T>() => this.inner.GetObjectType<T>();
            public Allors.Database.Meta.IObjectType GetObjectType(System.Type type) => this.inner.GetObjectType(type);
            public Allors.Database.Meta.IObjectType GetObjectType(System.Guid id) => this.inner.GetObjectType(id);
            public System.Type GetType(Allors.Database.Meta.IObjectType type) => this.inner.GetType(type);
        }

        private sealed class InvokeOrganisation : Organisation
        {
            private readonly System.Action<Organisation> invoke;
            private readonly System.Action<Organisation> toggle;
            internal InvokeOrganisation(Allors.Database.IStrategy strategy, System.Action<Organisation> invoke, System.Action<Organisation> toggle) : base(strategy) { this.invoke = invoke; this.toggle = toggle; }
            public new OrganisationJustDoIt JustDoIt() { this.invoke(this); return null; }
            public new OrganisationToggleCanWrite ToggleCanWrite()
            {
                if (this.toggle == null) return base.ToggleCanWrite();
                this.toggle(this);
                return base.ToggleCanWrite();
            }
        }

        [Fact]
        public void SameWorkspace()
        {
            this.SetUser("jane@example.com");

            var invokeRequest = new InvokeRequest
            {
                l = new[]
                {
                    new Invocation
                    {
                        i = this.x1.Id,
                        v = this.x1.Strategy.ObjectVersion,
                        m = this.M.WorkspaceXObject1.DoX.Tag
                    },
                },
            };

            var api = new Api(this.Transaction, "X", CancellationToken.None);
            var invokeResponse = api.Invoke(invokeRequest);

            Assert.False(invokeResponse.HasErrors);
        }

        [Fact]
        public void OtherWorkspace()
        {
            this.SetUser("jane@example.com");

            var invokeRequest = new InvokeRequest
            {
                l = new[]
                {
                    new Invocation
                    {
                        i = this.x1.Id,
                        v = this.x1.Strategy.ObjectVersion,
                        m = this.M.WorkspaceXObject1.DoX.Tag
                    },
                },
            };

            var api = new Api(this.Transaction, "Y", CancellationToken.None);
            var invokeResponse = api.Invoke(invokeRequest);

            Assert.True(invokeResponse.HasErrors);

            Assert.Single(invokeResponse._a);

            var accessError = invokeResponse._a[0];

            Assert.Equal(this.x1.Id, accessError);
        }

        [Fact]
        public void NoneWorkspace()
        {
            this.SetUser("jane@example.com");

            var invokeRequest = new InvokeRequest
            {
                l = new[]
                {
                    new Invocation
                    {
                        i = this.x1.Id,
                        v = this.x1.Strategy.ObjectVersion,
                        m = this.M.WorkspaceXObject1.DoX.Tag
                    },
                },
            };

            var api = new Api(this.Transaction, "None", CancellationToken.None);
            var invokeResponse = api.Invoke(invokeRequest);

            Assert.True(invokeResponse.HasErrors);

            Assert.Single(invokeResponse._a);

            var accessError = invokeResponse._a[0];

            Assert.Equal(this.x1.Id, accessError);
        }
    }
}
