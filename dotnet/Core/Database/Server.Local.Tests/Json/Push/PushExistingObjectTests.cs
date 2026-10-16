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

    public class PushExistingObjectTests : ApiTest, IClassFixture<Fixture>
    {
        private WorkspaceXObject1 x1;
        private long x1Version;

        private WorkspaceYObject1 y1;
        private long y1Version;

        private WorkspaceNoneObject1 none1;
        private long none1Version;

        public PushExistingObjectTests(Fixture fixture) : base(fixture)
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
        [InlineData(false)]
        [InlineData(true)]
        public void PushReadReturnsFullBaselineForSubmittedNoOp(bool omitRoles)
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = omitRoles
                ? new { o = new object[] { new { d = this.x1.Id, v = this.x1.Strategy.ObjectVersion } } }
                : new { o = new object[] { new PushRequestObject { d = this.x1.Id, v = this.x1.Strategy.ObjectVersion, r = Array.Empty<PushRequestRole>() } } };
            request["returnChanged"] = true;

            var response = this.ExecutePushRead(request, identity);

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            var changed = response.GetProperty("changed");
            Assert.Equal("changed", changed.GetProperty("k").GetString());
            var record = Assert.Single(changed.GetProperty("p").EnumerateArray());
            Assert.Equal(this.x1.Id, record.GetProperty("i").GetInt64());
            Assert.Equal(this.x1.Strategy.ObjectVersion, record.GetProperty("v").GetInt64());
            Assert.Equal(JsonValueKind.Object, record.GetProperty("p").ValueKind);
            Assert.All(changed.GetProperty("s").EnumerateArray(), value => Assert.Equal(JsonValueKind.Object, value.GetProperty("p").ValueKind));
            Assert.All(changed.GetProperty("pm").EnumerateArray(), value => Assert.Equal(JsonValueKind.Object, value.GetProperty("p").ValueKind));
            Assert.Empty(changed.GetProperty("u").EnumerateArray());
        }

        [Fact]
        public void PushReadCommitsWithoutReadsWhenNeitherRefreshOptionIsRequested()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Saved without refresh");

            var response = this.ExecutePushRead(request, identity);

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.False(response.TryGetProperty("changed", out var changed) && changed.ValueKind != JsonValueKind.Null);
            Assert.False(response.TryGetProperty("discovery", out var discovery) && discovery.ValueKind != JsonValueKind.Null);
            this.Transaction.Rollback();
            Assert.Equal("Saved without refresh", this.x1.WorkspaceXString);
        }

        [Theory]
        [InlineData("population")]
        [InlineData("user")]
        [InlineData("workspace")]
        [InlineData("revision")]
        [InlineData("context")]
        public void PushReadRejectsScopeMismatchBeforeWriting(string mismatch)
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Must not be written");
            request["returnChanged"] = true;
            var scope = (Allors.Protocol.Json.Api.Read.ReadScope)request["sc"];
            switch (mismatch)
            {
                case "population": scope.p = Guid.NewGuid().ToString("D"); break;
                case "user": scope.u++; break;
                case "workspace": scope.w = "Y"; break;
                case "revision": scope.r = "retired-revision"; break;
                case "context": scope.x = "different-context"; break;
            }

            if (mismatch == "context")
            {
                Assert.Throws<FormatException>(() => this.ExecutePushRead(request, identity));
            }
            else
            {
                var response = this.ExecutePushRead(request, identity);
                Assert.Equal("rejected", response.GetProperty("push").GetProperty("k").GetString());
                Assert.False(response.TryGetProperty("changed", out var changed) && changed.ValueKind != JsonValueKind.Null);
            }
            this.Transaction.Rollback();
            Assert.Null(this.x1.WorkspaceXString);
        }

        [Fact]
        public void PushReadRejectsInvalidDiscoveryBeforeWriting()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Must not be written");
            request["discover"] = new Allors.Protocol.Json.Api.Read.ReadPull
            {
                l = new[] { new Allors.Protocol.Json.Data.Pull
                {
                    e = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = "unknown-class" },
                } },
                d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
            };

            var response = this.ExecutePushRead(request, identity);

            Assert.Equal("rejected", response.GetProperty("push").GetProperty("k").GetString());
            this.Transaction.Rollback();
            Assert.Null(this.x1.WorkspaceXString);
        }

        [Fact]
        public void PushReadRejectsVersionConflictWithoutPublishingReadParts()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = new { o = new[] { new PushRequestObject
            {
                d = this.x1.Id, v = this.x1.Strategy.ObjectVersion + 1,
                r = new[] { new PushRequestRole { t = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag, u = "Must not be written" } },
            } } };
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);

            var response = this.ExecutePushRead(request, identity);

            Assert.Equal("rejected", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Contains(this.x1.Id, response.GetProperty("push").GetProperty("_v").EnumerateArray().Select(value => value.GetInt64()));
            Assert.False(response.TryGetProperty("changed", out var changed) && changed.ValueKind != JsonValueKind.Null);
            Assert.False(response.TryGetProperty("discovery", out var discovery) && discovery.ValueKind != JsonValueKind.Null);
            this.Transaction.Rollback();
            Assert.Null(this.x1.WorkspaceXString);
        }

        [Fact]
        public void PushReadRefreshesBothPreviousAndNewInverseEndpoints()
        {
            var request = this.PushReadRequest(out var identity, "Default");
            var previous = new TrimToBuilder(this.Transaction).WithName("Previous").Build();
            var next = new TrimToBuilder(this.Transaction).WithName("Next").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").WithMany2One(previous).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            request["push"] = new { o = new[] { new PushRequestObject
            {
                d = source.Id, v = source.Strategy.ObjectVersion,
                r = new[] { new PushRequestRole { t = this.M.TrimFrom.Many2One.RelationType.Tag, c = next.Id } },
            } } };
            request["returnChanged"] = true;
            request["discover"] = Discover(source.Id);

            var response = this.ExecutePushRead(request, identity, "Default");

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            var records = response.GetProperty("changed").GetProperty("p").EnumerateArray().ToArray();
            Assert.Equal(new[] { previous.Id, next.Id, source.Id }.OrderBy(value => value), records.Select(value => value.GetProperty("i").GetInt64()).OrderBy(value => value));
            var refreshedSource = records.Single(value => value.GetProperty("i").GetInt64() == source.Id);
            var discoveredSource = Assert.Single(response.GetProperty("discovery").GetProperty("p").EnumerateArray());
            Assert.Equal(refreshedSource.GetProperty("v").GetInt64(), discoveredSource.GetProperty("v").GetInt64());
            this.Transaction.Rollback();
            Assert.Equal(next.Id, source.Many2One.Id);
        }

        [Fact]
        public void PushReadFullBaselineKeepsRoleRevocationsAndCompleteSecurityBodies()
        {
            var request = this.PushReadRequest(out var identity, "Default");
            var source = new TrimFromBuilder(this.Transaction).WithName("Private name").Build();
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Allors.Operations.Read);
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build();
            source.AddRevocation(revocation);
            this.Transaction.Derive();
            this.Transaction.Commit();
            request["push"] = new { o = new[] { new PushRequestObject { d = source.Id, v = source.Strategy.ObjectVersion, r = Array.Empty<PushRequestRole>() } } };
            request["returnChanged"] = true;

            var response = this.ExecutePushRead(request, identity, "Default");

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            var changed = response.GetProperty("changed");
            var record = Assert.Single(changed.GetProperty("p").EnumerateArray());
            Assert.DoesNotContain(record.GetProperty("p").GetProperty("ro").EnumerateArray(), value => value.GetProperty("t").GetString() == this.M.TrimFrom.Name.RelationType.Tag);
            var security = changed.GetProperty("s").EnumerateArray().ToArray();
            var revoked = Assert.Single(security, value => value.GetProperty("k").GetString() == "revocation" && value.GetProperty("i").GetInt64() == revocation.Id);
            Assert.Equal(JsonValueKind.Object, revoked.GetProperty("p").ValueKind);
            Assert.NotEmpty(record.GetProperty("r").EnumerateArray());
            Assert.Contains(changed.GetProperty("pm").EnumerateArray(), value => value.GetProperty("i").GetInt64() == permission.Id && value.GetProperty("p").ValueKind == JsonValueKind.Object);
        }

        [Fact]
        public void PushReadAllowsNegativeUnitValuesInDiscoveryPredicates()
        {
            var request = this.PushReadRequest(out var identity, "Default");
            request["discover"] = new Allors.Protocol.Json.Api.Read.ReadPull
            {
                l = new[] { new Allors.Protocol.Json.Data.Pull { e = new Allors.Protocol.Json.Data.Extent
                {
                    k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag,
                    p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.C1.C1AllorsInteger.RelationType.Tag, v = -1 },
                } } },
                d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
            };

            var response = this.ExecutePushRead(request, identity, "Default");

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal("discovery", response.GetProperty("discovery").GetProperty("k").GetString());
        }

        [Theory]
        [InlineData("target")]
        [InlineData("predicate")]
        [InlineData("parameter")]
        public void PushReadRejectsNewObjectReferencesInDiscoveryBeforeWriting(string reference)
        {
            var request = this.PushReadRequest(out var identity, "Default");
            var discovery = Discover(-1);
            if (reference != "target")
            {
                discovery.l[0] = new Allors.Protocol.Json.Data.Pull
                {
                    e = new Allors.Protocol.Json.Data.Extent
                    {
                        k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag,
                        p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, ob = reference == "predicate" ? -1 : null, p = reference == "parameter" ? "target" : null },
                    },
                    a = reference == "parameter" ? new Dictionary<string, object> { ["target"] = -1 } : null,
                };
            }
            request["discover"] = discovery;
            var services = new PushReadServices(this.M);
            var database = this.PushReadDatabase(services);

            // Malformed wire references may be rejected during decoding; semantic references are
            // rejected at admission. Neither path may enter the mutation's derivation.
            try
            {
                var response = this.ExecutePushRead(request, identity, "Default", database);
                Assert.Equal("rejected", response.GetProperty("push").GetProperty("k").GetString());
            }
            catch (FormatException)
            {
            }
            Assert.Equal(0, services.Derivations);
        }

        private object XChange(string value) => new { o = new[] { new PushRequestObject
        {
            d = this.x1.Id, v = this.x1.Strategy.ObjectVersion,
            r = new[] { new PushRequestRole { t = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag, u = value } },
        } } };

        [Fact]
        public void PushReadCapturesDerivedCreationsDeletionsAndChangesAcrossCheckpoints()
        {
            var request = this.PushReadRequest(out var identity);
            var removed = new WorkspaceXObject1Builder(this.Transaction).Build();
            var hiddenRemoved = new WorkspaceNoneObject1Builder(this.Transaction).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            long createdId = 0;
            long hiddenCreatedId = 0;
            var services = new PushReadServices(this.M, transaction =>
            {
                ((WorkspaceXObject1)transaction.Instantiate(this.x1.Id)).WorkspaceXString = "Derived value";
                createdId = new WorkspaceXObject1Builder(transaction).WithWorkspaceXString("Derived creation").Build().Id;
                hiddenCreatedId = new WorkspaceNoneObject1Builder(transaction).Build().Id;
                transaction.Instantiate(removed.Id).Strategy.Delete();
                transaction.Instantiate(hiddenRemoved.Id).Strategy.Delete();
                transaction.Checkpoint();
            });
            request["push"] = new { o = new[] { new PushRequestObject { d = this.x1.Id, v = this.x1.Strategy.ObjectVersion, r = Array.Empty<PushRequestRole>() } } };
            request["returnChanged"] = true;

            var response = this.ExecutePushRead(request, identity, database: this.PushReadDatabase(services));

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            var changed = response.GetProperty("changed");
            var ids = changed.GetProperty("p").EnumerateArray().Select(value => value.GetProperty("i").GetInt64()).ToArray();
            Assert.Contains(this.x1.Id, ids);
            Assert.Contains(createdId, ids);
            Assert.DoesNotContain(hiddenCreatedId, ids);
            Assert.Equal(new[] { removed.Id }, changed.GetProperty("u").EnumerateArray().Select(value => value.GetInt64()));
            var record = changed.GetProperty("p").EnumerateArray().Single(value => value.GetProperty("i").GetInt64() == this.x1.Id);
            Assert.Equal("Derived value", record.GetProperty("p").GetProperty("ro").EnumerateArray().Single(value => value.GetProperty("t").GetString() == this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag).GetProperty("v").GetString());
            Assert.Equal(1, services.Derivations);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PushReadPreservesIndependentReadSuccess(bool failChanged)
        {
            var request = this.PushReadRequest(out var identity, "Default");
            var changed = new TrimFromBuilder(this.Transaction).WithName("Changed").Build();
            var discovered = new TrimFromBuilder(this.Transaction).WithName("Discovered").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var failedId = failChanged ? changed.Id : discovered.Id;
            var visits = 0;
            var services = new PushReadServices(this.M, read: (id, transaction) =>
            {
                if (id == failedId) { visits++; throw new FormatException("private security failure"); }
            });
            request["push"] = new { o = new[] { new PushRequestObject { d = changed.Id, v = changed.Strategy.ObjectVersion, r = Array.Empty<PushRequestRole>() } } };
            request["returnChanged"] = true;
            request["discover"] = Discover(discovered.Id);

            var response = this.ExecutePushRead(request, identity, "Default", this.PushReadDatabase(services));

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal(failChanged ? "error" : "changed", response.GetProperty("changed").GetProperty("k").GetString());
            Assert.Equal(failChanged ? "discovery" : "error", response.GetProperty("discovery").GetProperty("k").GetString());
            Assert.DoesNotContain("private security failure", response.GetRawText());
            Assert.True(visits > 0);
            Assert.Equal(2, services.Transactions);
        }

        [Fact]
        public void PushReadDiscardsBothReadPartsAfterSnapshotCleanupFailureButKeepsCommit()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Committed before cleanup failure");
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);
            var services = new PushReadServices(this.M, dispose: (attempt, transaction) =>
            {
                if (attempt == 2) { throw new FormatException("private cleanup failure"); }
            });

            var response = this.ExecutePushRead(request, identity, database: this.PushReadDatabase(services));

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal("error", response.GetProperty("changed").GetProperty("k").GetString());
            Assert.Equal("error", response.GetProperty("discovery").GetProperty("k").GetString());
            Assert.DoesNotContain("private cleanup failure", response.GetRawText());
            Assert.Equal(2, services.Transactions);
            this.Transaction.Rollback();
            Assert.Equal("Committed before cleanup failure", this.x1.WorkspaceXString);
        }

        [Fact]
        public void PushReadReportsCommittedSaveAndReadFailureWhenAffectedPoolExceedsLimit()
        {
            var request = this.PushReadRequest(out var identity);
            request["returnChanged"] = true;
            long lastCreated = 0;
            var services = new PushReadServices(this.M, transaction =>
            {
                lastCreated = transaction.Create(this.M.WorkspaceXObject1, 10001).Last().Id;
                transaction.Checkpoint();
            });

            var response = this.ExecutePushRead(request, identity, database: this.PushReadDatabase(services));

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal("error", response.GetProperty("changed").GetProperty("k").GetString());
            Assert.False(response.GetProperty("changed").TryGetProperty("p", out _));
            this.Transaction.Rollback();
            Assert.NotNull(this.Transaction.Instantiate(lastCreated));
            Assert.Equal(1, services.Derivations);
        }

        [Fact]
        public void PushReadPreservesNewIdReceiptWhenWriteCleanupFailsAfterCommit()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = new { n = new[] { new PushRequestNewObject { t = this.M.WorkspaceXObject1.Tag, w = -1 } } };
            request["returnChanged"] = true;
            var services = new PushReadServices(this.M, dispose: (attempt, transaction) =>
            {
                if (attempt == 1) { throw new FormatException("private write cleanup failure"); }
            });

            var response = this.ExecutePushRead(request, identity, database: this.PushReadDatabase(services));

            var receipt = response.GetProperty("push");
            Assert.Equal("committed", receipt.GetProperty("k").GetString());
            var mapping = Assert.Single(receipt.GetProperty("n").EnumerateArray());
            Assert.Equal(-1, mapping.GetProperty("w").GetInt64());
            Assert.Equal("error", response.GetProperty("changed").GetProperty("k").GetString());
            Assert.DoesNotContain("private write cleanup failure", response.GetRawText());
            this.Transaction.Rollback();
            Assert.NotNull(this.Transaction.Instantiate(mapping.GetProperty("d").GetInt64()));
            Assert.Equal(1, services.Transactions);
            Assert.Equal(1, services.Derivations);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PushReadPreservesCommitAfterTracingFailureOrCancellation(bool cancel)
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Committed before tracing");
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);
            using var cancellation = new CancellationTokenSource();
            var services = new PushReadServices(this.M);
            var database = (Allors.Database.Adapters.Sql.Database)this.PushReadDatabase(services);
            var visits = 0;
            database.Sink = new PushReadSink(() =>
            {
                visits++;
                if (cancel) { cancellation.Cancel(); }
                else { throw new FormatException("private tracing failure"); }
            });
            var executor = new PushReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" }));

            using var document = JsonDocument.Parse(executor.Execute(JsonSerializer.Serialize(request), "X", identity, cancellation.Token));

            var response = document.RootElement;
            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal("error", response.GetProperty("changed").GetProperty("k").GetString());
            Assert.Equal("error", response.GetProperty("discovery").GetProperty("k").GetString());
            Assert.DoesNotContain("private tracing failure", response.GetRawText());
            Assert.Equal(1, visits);
            Assert.Equal(1, services.Derivations);
            this.Transaction.Rollback();
            Assert.Equal("Committed before tracing", this.x1.WorkspaceXString);
        }

        [Fact]
        public void PushReadReportsUnknownCommitWithoutRetryingOrReturningReads()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Potentially committed");
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);
            var services = new PushReadServices(this.M);
            var existing = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            var configuration = new Allors.Database.Adapters.Sql.Configuration
            {
                ObjectFactory = existing.ObjectFactory, ConnectionString = existing.ConnectionString, SchemaName = existing.SchemaName,
            };
            var commits = 0;
            Action<Allors.Database.Adapters.Sql.IConnection> commit = connection =>
            {
                commits++;
                connection.Commit();
                throw new InvalidOperationException("private ambiguous commit failure");
            };
            Allors.Database.IDatabase database = existing is Allors.Database.Adapters.Sql.Npgsql.Database
                ? new PushReadUnknownNpgsqlDatabase(services, configuration, commit)
                : new PushReadUnknownSqlClientDatabase(services, configuration, commit);

            var response = this.ExecutePushRead(request, identity, database: database);

            Assert.Equal("unknown", response.GetProperty("push").GetProperty("k").GetString());
            Assert.False(response.TryGetProperty("changed", out var changed) && changed.ValueKind != JsonValueKind.Null);
            Assert.False(response.TryGetProperty("discovery", out var discovery) && discovery.ValueKind != JsonValueKind.Null);
            Assert.DoesNotContain("private ambiguous commit failure", response.GetRawText());
            Assert.Equal(1, commits);
            Assert.Equal(1, services.Derivations);
            Assert.Equal(1, services.Transactions);
        }

        [Fact]
        public void PushReadReturnsDerivationDiagnosticsAndRollsBackRejectedChanges()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Invalid derived change");
            request["returnChanged"] = true;
            var services = new PushReadServices(this.M, validationError: "Correct the invalid test value.");

            var response = this.ExecutePushRead(request, identity, database: this.PushReadDatabase(services));

            Assert.Equal("rejected", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Contains("Correct the invalid test value.", response.GetProperty("push").GetRawText());
            Assert.False(response.TryGetProperty("changed", out var changed) && changed.ValueKind != JsonValueKind.Null);
            this.Transaction.Rollback();
            Assert.Null(this.x1.WorkspaceXString);
            Assert.Equal(1, services.Derivations);
            Assert.Equal(1, services.Transactions);
        }

        [Fact]
        public void PushReadChangedAndDiscoveryKeepOneSnapshotAcrossAConcurrentCommit()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Saved value");
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);
            var concurrentWrites = 0;
            var services = new PushReadServices(this.M, read: (id, transaction) =>
            {
                if (id == this.x1.Id && concurrentWrites++ == 0)
                {
                    this.Transaction.Rollback();
                    this.x1.WorkspaceXString = "Later concurrent value";
                    this.Transaction.Derive();
                    this.Transaction.Commit();
                }
            });

            var response = this.ExecutePushRead(request, identity, database: this.PushReadDatabase(services));

            var changed = Assert.Single(response.GetProperty("changed").GetProperty("p").EnumerateArray());
            var discovered = Assert.Single(response.GetProperty("discovery").GetProperty("p").EnumerateArray());
            Assert.Equal(changed.GetProperty("v").GetInt64(), discovered.GetProperty("v").GetInt64());
            Assert.Equal("Saved value", changed.GetProperty("p").GetProperty("ro").EnumerateArray().Single(value => value.GetProperty("t").GetString() == this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag).GetProperty("v").GetString());
            Assert.True(concurrentWrites > 0);
            Assert.Equal(2, services.Transactions);
            this.Transaction.Rollback();
            Assert.Equal("Later concurrent value", this.x1.WorkspaceXString);
            Assert.NotEqual(changed.GetProperty("v").GetInt64(), this.x1.Strategy.ObjectVersion);
        }

        [Fact]
        public void PushReadKeepsCommitAndDiscoveryWhenFullRefreshExceedsByteLimit()
        {
            var request = this.PushReadRequest(out var identity, "Default");
            var source = new C1Builder(this.Transaction).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            request["push"] = new { o = new[] { new PushRequestObject { d = source.Id, v = source.Strategy.ObjectVersion, r = Array.Empty<PushRequestRole>() } } };
            request["returnChanged"] = true;
            request["discover"] = Discover(source.Id);
            var services = new PushReadServices(this.M, transaction =>
            {
                ((C1)transaction.Instantiate(source.Id)).C1AllorsBinary = new byte[13 * 1024 * 1024];
            });

            var response = this.ExecutePushRead(request, identity, "Default", this.PushReadDatabase(services));

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal("error", response.GetProperty("changed").GetProperty("k").GetString());
            Assert.Equal("discovery", response.GetProperty("discovery").GetProperty("k").GetString());
            Assert.Equal(source.Id, Assert.Single(response.GetProperty("discovery").GetProperty("p").EnumerateArray()).GetProperty("i").GetInt64());
            this.Transaction.Rollback();
            Assert.Equal(13 * 1024 * 1024, source.C1AllorsBinary.Length);
            Assert.Equal(1, services.Derivations);
        }

        [Fact]
        public void PushReadDiscardsTheSharedSnapshotWhenAProviderRejectsTheDiscoveryQuery()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Saved before discovery query");
            request["returnChanged"] = true;
            request["discover"] = new Allors.Protocol.Json.Api.Read.ReadPull
            {
                l = new[] { new Allors.Protocol.Json.Data.Pull { e = new Allors.Protocol.Json.Data.Extent
                {
                    k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.WorkspaceXObject1.Tag,
                    p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Like, r = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag, v = "%\\" },
                } } },
                d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
            };

            var response = this.ExecutePushRead(request, identity);

            Assert.Equal("committed", response.GetProperty("push").GetProperty("k").GetString());
            // A trailing LIKE escape is a native PostgreSQL query error (22025). SQL Server
            // treats the same pattern as a literal, so its shared snapshot remains usable.
            var nativeQueryFails = this.Transaction.Database is Allors.Database.Adapters.Sql.Npgsql.Database;
            Assert.Equal(nativeQueryFails ? "error" : "discovery", response.GetProperty("discovery").GetProperty("k").GetString());
            Assert.Equal(nativeQueryFails ? "error" : "changed", response.GetProperty("changed").GetProperty("k").GetString());
            this.Transaction.Rollback();
            Assert.Equal("Saved before discovery query", this.x1.WorkspaceXString);
        }

        private sealed class PushReadSink : Allors.Database.Tracing.ISink
        {
            private readonly Action after;
            internal PushReadSink(Action after) => this.after = after;
            public void OnBefore(Allors.Database.Tracing.IEvent value) { }
            public void OnAfter(Allors.Database.Tracing.IEvent value)
            {
                if (value is PushEvent) { this.after(); }
            }
        }

        // These databases use a real provider commit while withholding its success acknowledgement,
        // so the executor must preserve uncertainty even when the mutation actually reached SQL.
        private sealed class PushReadUnknownNpgsqlDatabase : Allors.Database.Adapters.Sql.Npgsql.Database
        {
            private readonly Action<Allors.Database.Adapters.Sql.IConnection> commit;
            internal PushReadUnknownNpgsqlDatabase(Allors.Database.IDatabaseServices services, Allors.Database.Adapters.Sql.Configuration configuration,
                Action<Allors.Database.Adapters.Sql.IConnection> commit) : base(services, configuration) => this.commit = commit;
            public override Allors.Database.Adapters.Sql.WriteScope OpenWrite(Guid expectedIncarnation)
            {
                var connection = this.ConnectionFactory.Create();
                return this.CreateWriteScope(connection, this.PopulationLifecycle.RequireReady(), expectedIncarnation, _ => this.commit(connection));
            }
        }

        private sealed class PushReadUnknownSqlClientDatabase : Allors.Database.Adapters.Sql.SqlClient.Database
        {
            private readonly Action<Allors.Database.Adapters.Sql.IConnection> commit;
            internal PushReadUnknownSqlClientDatabase(Allors.Database.IDatabaseServices services, Allors.Database.Adapters.Sql.Configuration configuration,
                Action<Allors.Database.Adapters.Sql.IConnection> commit) : base(services, configuration) => this.commit = commit;
            public override Allors.Database.Adapters.Sql.WriteScope OpenWrite(Guid expectedIncarnation)
            {
                var connection = this.ConnectionFactory.Create();
                return this.CreateWriteScope(connection, this.PopulationLifecycle.RequireReady(), expectedIncarnation, _ => this.commit(connection));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task PushReadLocalTransportRunsOneCommittedRefresh(bool typed)
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Saved through Local transport");
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);
            var services = new PushReadServices(this.M);
            var executor = new PushReadExecutor(this.PushReadDatabase(services), new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" }));
            var transport = new Allors.Workspace.Adapters.Local.PushReadTransport(executor, "X", identity);
            var raw = JsonSerializer.Serialize(request);

            var response = typed
                ? await transport.PushRead(Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(raw))
                : await transport.PushRead(raw);

            Assert.Equal("committed", response.push.k);
            var changed = Assert.IsType<Allors.Protocol.Json.Api.PushRead.PushReadChanged>(response.changed);
            var discovery = Assert.IsType<Allors.Protocol.Json.Api.Read.DiscoverySuccess>(response.discovery);
            Assert.Equal(Assert.Single(changed.p).v, Assert.Single(discovery.p).v);
            Assert.Equal(1, services.Derivations);
            Assert.Equal(2, services.Transactions);
            this.Transaction.Rollback();
            Assert.Equal("Saved through Local transport", this.x1.WorkspaceXString);
        }

        [Fact]
        public async System.Threading.Tasks.Task PushReadControllerRequiresConfigurationBeforeReadingTheRequest()
        {
            var workspace = new PushReadWorkspace();
            var resolver = new PushReadResolver(1);
            var controller = new PushReadController(workspace, resolver)
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() },
            };

            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(await controller.Post(CancellationToken.None));

            Assert.Equal(503, result.StatusCode);
            Assert.Equal(0, workspace.Reads);
            Assert.Equal(0, resolver.Resolutions);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task PushReadControllerRejectsMalformedAndOversizedRequestsBeforeResolvingUser(bool oversized)
        {
            this.PushReadRequest(out var identity);
            var raw = oversized ? new string(' ', PushReadExecutor.MaxRequestBytes + 1) : "{\"v\":1,\"v\":1}";
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "original") }, "Tests"));
            context.Request.Body = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(raw));
            var resolver = new PushReadResolver(identity.UserId);
            var controller = new PushReadController(new PushReadWorkspace(), resolver,
                new PushReadExecutor(this.Transaction.Database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" })))
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = context },
            };

            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(await controller.Post(CancellationToken.None), exactMatch: false);

            Assert.Equal(oversized ? 413 : 400, result.StatusCode);
            Assert.Equal(0, resolver.Resolutions);
        }

        [Fact]
        public async System.Threading.Tasks.Task PushReadControllerCapturesIdentityAndWorkspaceBeforeReadingTheBody()
        {
            var request = this.PushReadRequest(out var identity);
            request["push"] = this.XChange("Trusted HTTP identity");
            request["returnChanged"] = true;
            request["discover"] = Discover(this.x1.Id);
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "original") }, "Tests"));
            context.Request.Body = new PushReadIdentityMutatingStream(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)), () =>
                context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "changed") }, "Changed")));
            var resolver = new PushReadResolver(identity.UserId);
            var workspace = new PushReadWorkspace();
            var controller = new PushReadController(workspace, resolver,
                new PushReadExecutor(this.Transaction.Database, new ReadConfiguration(new Dictionary<string, string> { ["X"] = "revision-1" })))
            {
                ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = context },
            };

            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ContentResult>(await controller.Post(CancellationToken.None));

            using var response = JsonDocument.Parse(result.Content);
            Assert.Equal("committed", response.RootElement.GetProperty("push").GetProperty("k").GetString());
            Assert.Equal("changed", response.RootElement.GetProperty("changed").GetProperty("k").GetString());
            Assert.Equal("discovery", response.RootElement.GetProperty("discovery").GetProperty("k").GetString());
            Assert.Equal(1, workspace.Reads);
            Assert.Equal(2, resolver.Resolutions);
            this.Transaction.Rollback();
            Assert.Equal("Trusted HTTP identity", this.x1.WorkspaceXString);
        }

        private sealed class PushReadWorkspace : Allors.Services.IWorkspaceService
        {
            internal int Reads { get; private set; }
            public string Name => ++this.Reads == 1 ? "X" : "changed-workspace";
        }

        private sealed class PushReadResolver : Allors.Services.IUserResolver
        {
            private readonly long userId;
            internal PushReadResolver(long userId) => this.userId = userId;
            internal int Resolutions { get; private set; }
            public User Resolve(System.Security.Claims.ClaimsPrincipal principal, Allors.Database.ITransaction transaction)
            {
                this.Resolutions++;
                Assert.Equal("original", principal.FindFirst("user").Value);
                return (User)transaction.Instantiate(this.userId);
            }
        }

        private sealed class PushReadIdentityMutatingStream : System.IO.MemoryStream
        {
            private readonly Action mutate;
            internal PushReadIdentityMutatingStream(byte[] bytes, Action mutate) : base(bytes) => this.mutate = mutate;
            public override System.Threading.Tasks.ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                this.mutate();
                return base.ReadAsync(buffer, cancellationToken);
            }
        }

        private Allors.Database.IDatabase PushReadDatabase(PushReadServices services)
        {
            var existing = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            var configuration = new Allors.Database.Adapters.Sql.Configuration
            {
                ObjectFactory = existing.ObjectFactory, ConnectionString = existing.ConnectionString, SchemaName = existing.SchemaName,
            };
            return existing is Allors.Database.Adapters.Sql.Npgsql.Database
                ? new Allors.Database.Adapters.Sql.Npgsql.Database(services, configuration)
                : new Allors.Database.Adapters.Sql.SqlClient.Database(services, configuration);
        }

        private sealed class PushReadServices : Allors.Database.IDatabaseServices, Allors.Database.Derivations.IDerivationService
        {
            private readonly Allors.Database.Configuration.DefaultDatabaseServices inner;
            private readonly Action<Allors.Database.ITransaction> derive;
            private readonly Action<long, Allors.Database.ITransaction> read;
            private readonly Action<int, Allors.Database.ITransaction> dispose;
            private readonly string validationError;

            internal PushReadServices(Allors.Database.Meta.MetaPopulation meta, Action<Allors.Database.ITransaction> derive = null,
                Action<long, Allors.Database.ITransaction> read = null, Action<int, Allors.Database.ITransaction> dispose = null, string validationError = null)
            {
                this.inner = new Allors.Database.Configuration.DefaultDatabaseServices(new Allors.Database.Configuration.Derivations.Default.Engine(Rules.Create(meta)));
                this.derive = derive;
                this.read = read;
                this.dispose = dispose;
                this.validationError = validationError;
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
                return derivation;
            }

            private sealed class TransactionServices : Allors.Database.ITransactionServices, Allors.Database.Services.IWorkspaceAclsService
            {
                private readonly PushReadServices owner;
                private readonly Allors.Database.ITransactionServices inner;
                private Allors.Database.ITransaction transaction;
                private int attempt;
                internal TransactionServices(PushReadServices owner, Allors.Database.ITransactionServices inner) { this.owner = owner; this.inner = inner; }
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
                    public bool CanExecute(Allors.Database.Meta.IMethodType method) => this.inner.CanExecute(method);
                }
            }
        }

        private static Allors.Protocol.Json.Api.Read.ReadPull Discover(long id) => new Allors.Protocol.Json.Api.Read.ReadPull
        {
            l = new[] { new Allors.Protocol.Json.Data.Pull { o = id } },
            d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
        };

        private Dictionary<string, object> PushReadRequest(out LocalReadIdentity identity, string workspace = "X")
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = this.SetUser("jane@example.com");
            identity = new LocalReadIdentity(population.Incarnation, user.Id);
            return new Dictionary<string, object>
            {
                ["v"] = 1,
                ["i"] = "push-read-server-test",
                ["sc"] = new Allors.Protocol.Json.Api.Read.ReadScope { p = population.Incarnation.ToString("D"), u = user.Id, w = workspace, r = "revision-1" },
                ["push"] = new { },
            };
        }

        private JsonElement ExecutePushRead(Dictionary<string, object> request, LocalReadIdentity identity, string workspace = "X", Allors.Database.IDatabase database = null)
        {
            var configuration = new ReadConfiguration(new Dictionary<string, string> { [workspace] = "revision-1" });
            var executor = new PushReadExecutor(database ?? this.Transaction.Database, configuration);
            var rawRequest = JsonSerializer.Serialize(request);
            var rawResponse = executor.Execute(rawRequest, workspace, identity);
            var systemText = Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeResponse(rawResponse,
                Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(rawRequest));
            var newtonsoft = Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeResponse(rawResponse,
                Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeRequest(rawRequest));
            Assert.Equal(systemText.push.k, newtonsoft.push.k);
            using var document = JsonDocument.Parse(rawResponse);
            return document.RootElement.Clone();
        }

        [Fact]
        public void WorkspaceX1ObjectInWorkspaceX()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                o = new[]
                {
                    new PushRequestObject
                    {
                        d = this.x1.Id,
                        v = this.x1.Strategy.ObjectVersion,
                        r = new []
                        {
                            new PushRequestRole
                            {
                                t = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag,
                                u = "x string"
                            },
                        }
                    },
                }
            };

            var api = new Api(this.Transaction, "X", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            Assert.NotEqual(this.x1.Strategy.ObjectVersion, this.x1Version);
            Assert.Equal("x string", this.x1.WorkspaceXString);
        }


        [Fact]
        public void WorkspaceX1ObjectInWorkspaceY()
        {
            this.SetUser("jane@example.com");

            var pushRequest = new PushRequest
            {
                o = new[]
                {
                    new PushRequestObject
                    {
                        d = this.x1.Id,
                        v = this.x1.Strategy.ObjectVersion,
                        r = new []
                        {
                            new PushRequestRole
                            {
                                t = this.M.WorkspaceXObject1.WorkspaceXString.RelationType.Tag,
                                u = "x string"
                            },
                        }
                    },
                }
            };

            var api = new Api(this.Transaction, "Y", CancellationToken.None);
            var pushResponse = api.Push(pushRequest);

            this.Transaction.Rollback();

            Assert.Equal(this.x1.Strategy.ObjectVersion, this.x1Version);
            Assert.Null(this.x1.WorkspaceXString);
        }
    }
}
