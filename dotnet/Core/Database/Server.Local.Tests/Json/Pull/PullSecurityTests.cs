// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using Allors;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;
    using Permission = Allors.Database.Domain.Permission;
    using TrimFrom = Allors.Database.Domain.TrimFrom;
    using User = Allors.Database.Domain.User;

    public class PullSecurityTests : ApiTest, IClassFixture<Fixture>
    {
        public PullSecurityTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Theory]
        [InlineData("selection", false)]
        [InlineData("security", false)]
        [InlineData("permissions", false)]
        [InlineData("validation", false)]
        [InlineData("cleanup", false)]
        [InlineData("cleanup-taint", false)]
        [InlineData("selection", true)]
        [InlineData("security", true)]
        [InlineData("permissions", true)]
        [InlineData("validation", true)]
        [InlineData("cleanup", true)]
        public void CompleteReadSuppressesSuccessForFaultsAndCancellation(string phase, bool cancel)
        {
            this.CompleteReadExecutor(out var identity);
            var target = new TrimToBuilder(this.Transaction).WithName("Target").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").WithMany2One(target).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            using var cancellation = new CancellationTokenSource();
            var fired = 0;
            var services = new CompleteReadServices(this.M, (current, transaction) =>
            {
                if (current != phase) { return; }
                fired++;
                if (cancel) { cancellation.Cancel(); }
                else if (phase == "cleanup-taint") { try { transaction.Commit(); } catch (InvalidOperationException) { } }
                else if (phase != "permissions" && phase != "validation") { throw new FormatException("private fault detail"); }
            }, phase == "permissions" && !cancel, phase == "validation");
            var executor = this.CompleteReadExecutor(services);
            var request = this.CompleteReadRequest(new Pull { Object = source, Results = new[] { new Result
            {
                Name = "source", Include = new[] { new Node(this.M.TrimFrom.Many2One) },
            } } });
            var raw = System.Text.Json.JsonSerializer.Serialize(request);
            if (cancel)
            {
                Assert.Throws<OperationCanceledException>(() => executor.Execute(raw, "Default", identity, cancellation.Token));
            }
            else
            {
                var json = executor.Execute(raw, "Default", identity);
                Assert.IsType<Allors.Protocol.Json.Api.Read.ReadFailure>(ReadJson.DeserializeResponse(json, request));
                Assert.DoesNotContain("private fault detail", json);
            }

            Assert.True(fired > 0, phase + " fault was not exercised");
            Assert.Equal(1, services.Attempts);
            Assert.Equal(1, services.Disposals);
            Assert.ThrowsAny<InvalidOperationException>(() => services.Transaction.Instantiate(source.Id));
        }

        [Fact]
        public void CompleteReadRetainsItsSnapshotAcrossSelectionSecurityAndRoleMaterialization()
        {
            this.CompleteReadExecutor(out var identity);
            var source = new TrimFromBuilder(this.Transaction).WithName("Snapshot name").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var version = source.Strategy.ObjectVersion;
            var writes = 0;
            var services = new CompleteReadServices(this.M, (phase, _) =>
            {
                if (phase == "security" && writes++ == 0)
                {
                    source.Name = "Later committed name";
                    this.Transaction.Derive();
                    this.Transaction.Commit();
                }
            });
            var executor = this.CompleteReadExecutor(services);
            var request = this.CompleteReadRequest(new Pull { Object = source });
            var response = this.ExecuteCompleteRead(executor, identity, request);
            var record = Assert.Single(response.p);
            Assert.Equal(version, record.v);
            Assert.Equal("Snapshot name", record.p.ro.Single(v => v.t == this.M.TrimFrom.Name.RelationType.Tag).v.ToString());
            Assert.True(writes > 0);
            var next = this.ExecuteCompleteRead(executor, identity, request);
            Assert.NotEqual(version, Assert.Single(next.p).v);
        }

        private ReadExecutor CompleteReadExecutor(CompleteReadServices services)
        {
            var existing = (Allors.Database.Adapters.Sql.Database)this.Transaction.Database;
            Allors.Database.IDatabase database = existing is Allors.Database.Adapters.Sql.Npgsql.Database
                ? new Allors.Database.Adapters.Sql.Npgsql.Database(services, new Allors.Database.Adapters.Sql.Configuration
                {
                    ObjectFactory = existing.ObjectFactory, ConnectionString = existing.ConnectionString, SchemaName = existing.SchemaName,
                })
                : new Allors.Database.Adapters.Sql.SqlClient.Database(services, new Allors.Database.Adapters.Sql.Configuration
                {
                    ObjectFactory = existing.ObjectFactory, ConnectionString = existing.ConnectionString, SchemaName = existing.SchemaName,
                });
            return new ReadExecutor(database, new ReadConfiguration(new System.Collections.Generic.Dictionary<string, string> { ["Default"] = "revision-1" }));
        }

        private sealed class CompleteReadServices : Allors.Database.IDatabaseServices
        {
            private readonly Allors.Database.IDatabaseServices inner;
            private readonly Action<string, Allors.Database.ITransaction> visit;
            private readonly bool invalidPermission;
            private readonly bool invalidVersion;

            internal CompleteReadServices(MetaPopulation meta, Action<string, Allors.Database.ITransaction> visit, bool invalidPermission = false, bool invalidVersion = false)
            {
                this.inner = new Allors.Database.Configuration.DefaultDatabaseServices(
                    new Allors.Database.Configuration.Derivations.Default.Engine(Rules.Create(meta)));
                this.visit = visit;
                this.invalidPermission = invalidPermission;
                this.invalidVersion = invalidVersion;
            }

            internal int Attempts { get; private set; }
            internal int Disposals { get; private set; }
            internal Allors.Database.ITransaction Transaction { get; private set; }
            public void OnInit(Allors.Database.IDatabase database) => this.inner.OnInit(database);
            public T Get<T>() => this.inner.Get<T>();
            public void Dispose() => this.inner.Dispose();
            public Allors.Database.ITransactionServices CreateTransactionServices() => new TransactionServices(this, this.inner.CreateTransactionServices());

            private sealed class TransactionServices : Allors.Database.ITransactionServices, Allors.Database.Services.IWorkspaceAclsService
            {
                private readonly CompleteReadServices owner;
                private readonly Allors.Database.ITransactionServices inner;
                internal TransactionServices(CompleteReadServices owner, Allors.Database.ITransactionServices inner) { this.owner = owner; this.inner = inner; }
                public void OnInit(Allors.Database.ITransaction transaction)
                {
                    this.owner.Attempts++;
                    this.owner.Transaction = transaction;
                    this.inner.OnInit(transaction);
                }
                public T Get<T>() => typeof(T) == typeof(Allors.Database.Services.IWorkspaceAclsService) ? (T)(object)this : this.inner.Get<T>();
                public Allors.Database.Security.IAccessControl Create(string workspaceName) => new AccessControl(this.owner,
                    this.inner.Get<Allors.Database.Services.IWorkspaceAclsService>().Create(workspaceName));
                public void Dispose()
                {
                    this.owner.Disposals++;
                    this.inner.Dispose();
                    this.owner.visit("cleanup", this.owner.Transaction);
                    this.owner.visit("cleanup-taint", this.owner.Transaction);
                }
            }

            private sealed class AccessControl : Allors.Database.Security.IAccessControl
            {
                private readonly CompleteReadServices owner;
                private readonly Allors.Database.Security.IAccessControl inner;
                internal AccessControl(CompleteReadServices owner, Allors.Database.Security.IAccessControl inner) { this.owner = owner; this.inner = inner; }
                public Allors.Database.Security.IAccessControlList this[Allors.Database.IObject value] => new Acl(this.owner, this.inner[value]);
            }

            private sealed class Acl : Allors.Database.Security.IAccessControlList
            {
                private readonly CompleteReadServices owner;
                private readonly Allors.Database.Security.IAccessControlList inner;
                internal Acl(CompleteReadServices owner, Allors.Database.Security.IAccessControlList inner) { this.owner = owner; this.inner = inner; }
                public Allors.Database.Domain.IVersionedGrant[] Grants
                {
                    get
                    {
                        this.owner.visit("security", this.owner.Transaction);
                        return this.inner.Grants.Select(v => (Allors.Database.Domain.IVersionedGrant)new Grant(this.owner, v)).ToArray();
                    }
                }
                public Allors.Database.Domain.IVersionedRevocation[] Revocations => this.inner.Revocations;
                public bool CanRead(Allors.Database.Meta.IRoleType role)
                {
                    this.owner.visit("selection", this.owner.Transaction);
                    return this.inner.CanRead(role);
                }
                public bool CanWrite(Allors.Database.Meta.IRoleType role) => this.inner.CanWrite(role);
                public bool CanExecute(Allors.Database.Meta.IMethodType method) => this.inner.CanExecute(method);
            }

            private sealed class Grant : Allors.Database.Domain.IVersionedGrant
            {
                private readonly CompleteReadServices owner;
                private readonly Allors.Database.Domain.IVersionedGrant inner;
                internal Grant(CompleteReadServices owner, Allors.Database.Domain.IVersionedGrant inner) { this.owner = owner; this.inner = inner; }
                public long Id => this.inner.Id;
                public long Version
                {
                    get
                    {
                        this.owner.visit("validation", this.owner.Transaction);
                        return this.owner.invalidVersion ? long.MaxValue : this.inner.Version;
                    }
                }
                public System.Collections.Generic.ISet<long> UserSet => this.inner.UserSet;
                public System.Collections.Generic.ISet<long> PermissionSet
                {
                    get
                    {
                        this.owner.visit("permissions", this.owner.Transaction);
                        return this.owner.invalidPermission ? new System.Collections.Generic.HashSet<long> { 9007199254740991 } : this.inner.PermissionSet;
                    }
                }
                public Allors.Ranges.IRange<long> PermissionRange => this.inner.PermissionRange;
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CompleteReadDoesNotDistinguishMissingAndWorkspaceIneligibleTargets(bool withSelect)
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var hidden = new AccessClassBuilder(this.Transaction).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.DoesNotContain("Default", hidden.Strategy.Class.WorkspaceNames);
            var request = this.CompleteReadRequest(new Pull
            {
                Object = hidden,
                Results = withSelect ? new[] { new Result { Select = new Select(this.M.TrimFrom.Many2One) } } : null,
            });
            request.p.l[0].t = null;
            var unavailable = this.ExecuteCompleteRead(executor, identity, request);
            request.p.l[0].o = 9007199254740991;
            var missing = this.ExecuteCompleteRead(executor, identity, request);
            Assert.Equal("unavailable", Assert.Single(unavailable.l).s);
            Assert.Equal("unavailable", Assert.Single(missing.l).s);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(missing.o), System.Text.Json.JsonSerializer.Serialize(unavailable.o));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(missing.c), System.Text.Json.JsonSerializer.Serialize(unavailable.c));
            Assert.Empty(unavailable.p);
        }

        [Fact]
        public void CompleteReadDoesNotExposeResolverExceptionDetails()
        {
            var executor = this.CompleteReadExecutor(out _);
            var request = this.CompleteReadRequest();
            var json = executor.Execute(System.Text.Json.JsonSerializer.Serialize(request), "Default",
                transaction => throw new FormatException("private resolver credential detail"));
            Assert.IsType<Allors.Protocol.Json.Api.Read.ReadFailure>(ReadJson.DeserializeResponse(json, request));
            Assert.DoesNotContain("private resolver credential detail", json);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CompleteReadKeepsIncludeAndDependencyPermissionRulesDistinct(bool dependency)
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var target = new TrimToBuilder(this.Transaction).WithName("Target").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").WithMany2One(target).Build();
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Many2One, Operations.Read);
            source.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.CompleteReadRequest(new Pull { Object = source, Results = new[] { new Result
            {
                Name = "source", Include = new[] { new Node(this.M.TrimFrom.Many2One) },
            } } });
            if (dependency) { request.p.d = new[] { new PullDependency { o = this.M.TrimFrom.Tag, r = this.M.TrimFrom.Many2One.RelationType.Tag } }; }
            var response = this.ExecuteCompleteRead(executor, identity, request);
            Assert.Equal(dependency, response.p.Any(v => v.i == target.Id));
            Assert.DoesNotContain(response.p.Single(v => v.i == source.Id).p.ro, v => v.t == this.M.TrimFrom.Many2One.RelationType.Tag);
            Assert.Equal(source.Id, response.o["source"]);
        }

        [Fact]
        public void CompleteReadClosesCyclicDependenciesAndSharesSecurityEntries()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var target = new TrimToBuilder(this.Transaction).WithName("Shared target").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").WithMany2One(target).Build();
            var sibling = new TrimFromBuilder(this.Transaction).WithName("Sibling").WithMany2One(target).Build();
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Shared read").WithPermission(permission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject((User)this.Transaction.Instantiate(identity.UserId)).WithRole(role).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            source.AddSecurityToken(token);
            sibling.AddSecurityToken(token);
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.CompleteReadRequest(new Pull { Object = source });
            request.p.d = new[]
            {
                new PullDependency { o = this.M.TrimFrom.Tag, r = this.M.TrimFrom.Many2One.RelationType.Tag },
                new PullDependency { o = this.M.TrimTo.Tag, a = this.M.TrimFrom.Many2One.RelationType.Tag },
            };

            var response = this.ExecuteCompleteRead(executor, identity, request);

            Assert.Equal(new[] { source.Id, target.Id, sibling.Id }.OrderBy(v => v), response.p.Select(v => v.i).OrderBy(v => v));
            Assert.Single(response.s.Where(v => v.k == "grant" && v.i == grant.Id));
            var index = Array.FindIndex(response.s, v => v.k == "grant" && v.i == grant.Id);
            Assert.Contains(index, response.p.Single(v => v.i == source.Id).g);
            Assert.Contains(index, response.p.Single(v => v.i == sibling.Id).g);
        }

        [Fact]
        public void CompleteReadReevaluatesInverseIncludesAfterDiscoveryAndWhenTargetRecordIsReused()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var target = new TrimToBuilder(this.Transaction).WithName("Target").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.CompleteReadRequest(new Pull { Object = target, Results = new[] { new Result
            {
                Name = "target", Include = new[] { new Node(this.M.TrimFrom.Many2One.AssociationType) },
            } } });
            var first = this.ExecuteCompleteRead(executor, identity, request);
            request.k = "discover";
            var discovery = Assert.IsType<Allors.Protocol.Json.Api.Read.DiscoverySuccess>(ReadJson.DeserializeResponse(
                executor.Execute(System.Text.Json.JsonSerializer.Serialize(request), "Default", identity), request));
            Assert.Single(discovery.p);
            var source = new TrimFromBuilder(this.Transaction).WithName("New inverse member").WithMany2One(target).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            request.k = "read";
            request.sc = first.sc;
            request.h = new Allors.Protocol.Json.Api.Read.ReadCache
            {
                s = first.s.Select(v => new Allors.Protocol.Json.Api.Read.ReadSecurityHint { k = v.k, i = v.i, v = v.v, h = true }).ToArray(),
                r = first.p.Select(v => new Allors.Protocol.Json.Api.Read.ReadRecordHeader { i = v.i, c = v.c, v = v.v, g = v.g, r = v.r }).ToArray(),
                p = first.pm.Select(v => v.i).ToArray(),
            };
            var after = this.ExecuteCompleteRead(executor, identity, request);
            Assert.Equal(2, after.p.Length);
            Assert.Equal(0, after.p.Single(v => v.i == target.Id).p.r);
            Assert.NotNull(after.p.Single(v => v.i == source.Id).p.ro);
        }

        [Fact]
        public void CompleteReadReturnsDetachedRecordsAndPermissionClosure()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var source = new TrimFromBuilder(this.Transaction).WithName("Read body").Build();
            var create = (Allors.Database.Domain.CreatePermission)this.Transaction.Instantiate(this.M.TrimFrom.CreatePermissionId);
            var name = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Read and create").WithPermission(name).WithPermission(create).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject((User)this.Transaction.Instantiate(identity.UserId)).WithRole(role).Build();
            source.AddSecurityToken(new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.CompleteReadRequest(new Pull { Object = source, Results = new[] { new Result { Name = "source" } } });
            var response = this.ExecuteCompleteRead(executor, identity, request);
            var record = Assert.Single(response.p);
            Assert.Equal(source.Id, response.o["source"]);
            Assert.Equal("Read body", record.p.ro.Single(v => v.t == this.M.TrimFrom.Name.RelationType.Tag).v.ToString());
            Assert.Equal("resolved", Assert.Single(response.l).s);
            Assert.NotEmpty(response.s);
            var required = response.s.SelectMany(v => v.p.p).Distinct().OrderBy(v => v).ToArray();
            Assert.Equal(required, response.pm.Select(v => v.i).OrderBy(v => v));
            Assert.Contains(response.pm, v => v.p.o == (long)Operations.Create && v.p.t == null);
            source.Name = "Ambient change";
            Assert.Equal("Read body", record.p.ro.Single(v => v.t == this.M.TrimFrom.Name.RelationType.Tag).v.ToString());
        }

        [Fact]
        public void CompleteReadReusesExactCandidatesAndKeepsPermissionClosureForReusedSecurity()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var source = new TrimFromBuilder(this.Transaction).WithName("Reusable").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.CompleteReadRequest(new Pull { Object = source });
            var full = this.ExecuteCompleteRead(executor, identity, request);
            request.sc = full.sc;
            request.h = new Allors.Protocol.Json.Api.Read.ReadCache
            {
                s = full.s.Select(v => new Allors.Protocol.Json.Api.Read.ReadSecurityHint { k = v.k, i = v.i, v = v.v, h = true }).ToArray(),
                r = full.p.Select(v => new Allors.Protocol.Json.Api.Read.ReadRecordHeader { i = v.i, c = v.c, v = v.v, g = v.g, r = v.r }).ToArray(),
                p = Array.Empty<long>(),
            };
            var reused = this.ExecuteCompleteRead(executor, identity, request);
            Assert.Equal(0, Assert.Single(reused.p).p.r);
            Assert.All(reused.s, v => Assert.NotNull(v.p.r));
            Assert.Equal(full.pm.Select(v => v.i).OrderBy(v => v), reused.pm.Select(v => v.i).OrderBy(v => v));
            Assert.All(reused.pm, v => Assert.Null(v.p.r));
            request.h.p = full.pm.Select(v => v.i).Reverse().ToArray();
            var allReused = this.ExecuteCompleteRead(executor, identity, request);
            Assert.All(allReused.pm, v => Assert.Equal(v.i, request.h.p[v.p.r.Value]));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(full.o), System.Text.Json.JsonSerializer.Serialize(allReused.o));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(full.c), System.Text.Json.JsonSerializer.Serialize(allReused.c));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(full.p.Select(v => v.p.ro)),
                System.Text.Json.JsonSerializer.Serialize(allReused.p.Select(v => full.p[v.p.r.Value].p.ro)));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(full.s.Select(v => v.p.p)),
                System.Text.Json.JsonSerializer.Serialize(allReused.s.Select(v => full.s[v.p.r.Value].p.p)));
            var fullPermissions = full.pm.ToDictionary(v => v.i, v => v.p);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(full.pm.Select(v => v.p)),
                System.Text.Json.JsonSerializer.Serialize(allReused.pm.Select(v => fullPermissions[request.h.p[v.p.r.Value]])));
            request.h.s[0].h = false;
            var provenanceOnly = this.ExecuteCompleteRead(executor, identity, request);
            Assert.NotNull(provenanceOnly.s.Single(v => v.i == request.h.s[0].i).p.p);
        }

        [Fact]
        public void CompleteReadRejectsOldProvenanceAfterPermissionChangeWithoutDataVersionChange()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var user = (User)this.Transaction.Instantiate(identity.UserId);
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Read role").WithPermission(permission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Previously readable").Build();
            source.AddSecurityToken(new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();
            var request = this.CompleteReadRequest(new Pull { Object = source });
            var before = this.ExecuteCompleteRead(executor, identity, request);
            request.sc = before.sc;
            request.h = new Allors.Protocol.Json.Api.Read.ReadCache
            {
                s = before.s.Select(v => new Allors.Protocol.Json.Api.Read.ReadSecurityHint { k = v.k, i = v.i, v = v.v, h = true }).ToArray(),
                r = before.p.Select(v => new Allors.Protocol.Json.Api.Read.ReadRecordHeader { i = v.i, c = v.c, v = v.v, g = v.g, r = v.r }).ToArray(),
                p = before.pm.Select(v => v.i).ToArray(),
            };
            role.Permissions = Array.Empty<Permission>();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var after = this.ExecuteCompleteRead(executor, identity, request);
            Assert.Equal(before.p[0].v, after.p[0].v);
            Assert.Null(after.p[0].p.r);
            Assert.DoesNotContain(after.p[0].p.ro, v => v.t == this.M.TrimFrom.Name.RelationType.Tag);
            Assert.Empty(Assert.Single(after.s).p.p);
            Assert.Empty(after.pm);
        }

        [Fact]
        public void CompleteReadRetainsEmptyOutputsAndReportsUnavailableTargets()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var request = this.CompleteReadRequest(new Pull
            {
                Extent = new Extent(this.M.TrimFrom) { Predicate = new Equals { PropertyType = this.M.TrimFrom.Name, Value = "Never present" } },
                Results = new[] { new Result { Name = "empty" } },
            });
            var original = request.p.l[0];
            request.p.l = new[] { original, new Allors.Protocol.Json.Data.Pull { o = 9007199254740991, r = new[] { new Allors.Protocol.Json.Data.Result { n = "missing" } } } };
            var response = this.ExecuteCompleteRead(executor, identity, request);
            Assert.Empty(response.c["empty"]);
            Assert.Null(response.o["missing"]);
            Assert.Equal(new[] { "resolved", "unavailable" }, response.l.Select(v => v.s));
            Assert.Empty(response.p);
        }

        [Fact]
        public void CompleteReadFailureContainsNoSuccessfulPayloadAndDoesNotRetryResolution()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var request = this.CompleteReadRequest();
            var attempts = 0;
            var json = executor.Execute(System.Text.Json.JsonSerializer.Serialize(request), "Default", transaction =>
            {
                attempts++;
                try { transaction.Commit(); } catch (InvalidOperationException) { }
                return (User)transaction.Instantiate(identity.UserId);
            });
            var failure = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadFailure>(ReadJson.DeserializeResponse(json, request));
            Assert.Equal(1, attempts);
            Assert.NotEmpty(failure.e);
            Assert.DoesNotContain("\"p\":", json);
        }

        [Fact]
        public void CompleteReadRejectsMalformedInputBeforeResolvingUserAndHonorsCancellation()
        {
            var executor = this.CompleteReadExecutor(out var identity);
            var attempts = 0;
            Assert.Throws<FormatException>(() => executor.Execute("{\"v\":1,\"v\":1}", "Default", transaction =>
            {
                attempts++;
                return (User)transaction.Instantiate(identity.UserId);
            }));
            Assert.Equal(0, attempts);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(() => executor.Execute(System.Text.Json.JsonSerializer.Serialize(this.CompleteReadRequest()), "Default", identity, cancellation.Token));
        }

        private ReadExecutor CompleteReadExecutor(out LocalReadIdentity identity)
        {
            this.ReadFactory(out identity);
            return new ReadExecutor(this.Transaction.Database, new ReadConfiguration(
                new System.Collections.Generic.Dictionary<string, string> { ["Default"] = "revision-1" }));
        }

        private Allors.Protocol.Json.Api.Read.ReadRequest CompleteReadRequest(params Pull[] pulls) => new Allors.Protocol.Json.Api.Read.ReadRequest
        {
            v = 1, i = "complete-read-test", k = "read",
            p = new Allors.Protocol.Json.Api.Read.ReadPull { l = pulls.Select(v => v.ToJson(this.UnitConvert)).ToArray(), d = Array.Empty<PullDependency>() },
        };

        private Allors.Protocol.Json.Api.Read.ReadSuccess ExecuteCompleteRead(ReadExecutor executor, LocalReadIdentity identity, Allors.Protocol.Json.Api.Read.ReadRequest request)
        {
            var json = executor.Execute(System.Text.Json.JsonSerializer.Serialize(request), "Default", identity);
            var response = ReadJson.DeserializeResponse(json, request);
            Assert.True(response is Allors.Protocol.Json.Api.Read.ReadSuccess, json);
            return (Allors.Protocol.Json.Api.Read.ReadSuccess)response;
        }

        [Fact]
        public void ProtectedReadCapturesConfigurationAndReturnsAnOwnedScope()
        {
            this.ReadFactory(out var identity);
            var revisions = new System.Collections.Generic.Dictionary<string, string> { ["Default"] = "revision-1" };
            var factory = new ReadScopeFactory(this.Transaction.Database, new ReadConfiguration(revisions));
            revisions["Default"] = "revision-2";

            using var read = factory.Open("Default", identity, " exact context ");
            var scope = read.Scope;
            Assert.Equal(identity.Population.ToString("D"), scope.p);
            Assert.Equal(identity.UserId, scope.u);
            Assert.Equal("Default", scope.w);
            Assert.Equal("revision-1", scope.r);
            Assert.Equal(" exact context ", scope.x);
            scope.u = 999;
            scope.r = "changed";
            Assert.Equal(identity.UserId, read.Scope.u);
            Assert.Equal("revision-1", read.Scope.r);
            Assert.Equal(identity.UserId, read.Transaction.Services.Get<Allors.Database.Services.IUserService>().User.Id);
            read.Complete();
            Assert.Throws<InvalidOperationException>(() => read.EnsureValid());
        }

        [Theory]
        [InlineData("population")]
        [InlineData("user")]
        [InlineData("workspace")]
        [InlineData("revision")]
        [InlineData("context")]
        public void ProtectedReadRejectsEveryExpectedScopeMismatch(string field)
        {
            var factory = this.ReadFactory(out var identity);
            Allors.Protocol.Json.Api.Read.ReadScope expected;
            using (var first = factory.Open("Default", identity, null))
            {
                expected = first.Scope;
            }

            switch (field)
            {
                case "population": expected.p = Guid.NewGuid().ToString("D"); break;
                case "user": expected.u++; break;
                case "workspace": expected.w = "X"; break;
                case "revision": expected.r = "another-revision"; break;
                case "context": expected.x = string.Empty; break;
            }

            var resolutions = 0;
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", transaction =>
            {
                resolutions++;
                return (User)transaction.Instantiate(identity.UserId);
            }, null, expected));
            Assert.Equal(1, resolutions);
            using var next = factory.Open("Default", identity, null);
            next.Complete();
        }

        [Fact]
        public void ProtectedReadCapturesExpectedScopeBeforeUserResolution()
        {
            var factory = this.ReadFactory(out var identity);
            Allors.Protocol.Json.Api.Read.ReadScope expected;
            using (var first = factory.Open("Default", identity, null))
            {
                expected = first.Scope;
            }

            using var read = factory.Open("Default", transaction =>
            {
                expected.u++;
                expected.r = "changed during resolution";
                return (User)transaction.Instantiate(identity.UserId);
            }, null, expected);
            Assert.Equal(identity.UserId, read.Scope.u);
            read.Complete();
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("X")]
        public void ProtectedReadRejectsUnknownOrUnconfiguredWorkspaceBeforeResolution(string workspace)
        {
            var factory = this.ReadFactory(out var identity);
            var called = false;
            Assert.Throws<InvalidOperationException>(() => factory.Open(workspace, transaction =>
            {
                called = true;
                return (User)transaction.Instantiate(identity.UserId);
            }, null));
            Assert.False(called);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProtectedReadConfigurationRequiresARevision(string revision)
        {
            Assert.Throws<ArgumentException>(() => new ReadConfiguration(
                new System.Collections.Generic.Dictionary<string, string> { ["Default"] = revision }));
        }

        [Fact]
        public void ProtectedReadRejectsRetiredLocalIdentity()
        {
            var factory = this.ReadFactory(out var identity);
            ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", identity, null));
        }

        [Fact]
        public void ProtectedReadRejectsMissingAndWrongClassLocalUsers()
        {
            var factory = this.ReadFactory(out var identity);
            var source = new TrimFromBuilder(this.Transaction).WithName("Not a user").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", new LocalReadIdentity(identity.Population, source.Id), null));
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", new LocalReadIdentity(identity.Population, long.MaxValue), null));
        }

        [Fact]
        public void ProtectedReadRejectsAnUnresolvedOrForeignTransactionUser()
        {
            var factory = this.ReadFactory(out var identity);
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", transaction => null, null));
            var ambientUser = (User)this.Transaction.Instantiate(identity.UserId);
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", transaction => ambientUser, null));
        }

        [Fact]
        public void ProtectedReadLeavesAmbientEditsUntouchedAndReadsCommittedValues()
        {
            var factory = this.ReadFactory(out var identity);
            var source = new TrimFromBuilder(this.Transaction).WithName("Committed").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var version = source.Strategy.ObjectVersion;
            source.Name = "Ambient edit";

            using var read = factory.Open("Default", identity, null);
            var readSource = (TrimFrom)read.Transaction.Instantiate(source.Id);
            Assert.NotSame(this.Transaction, read.Transaction);
            Assert.Equal("Committed", readSource.Name);
            Assert.Equal(version, readSource.Strategy.ObjectVersion);
            read.Complete();
            Assert.Equal("Ambient edit", source.Name);
            Assert.Equal(version, source.Strategy.ObjectVersion);
        }

        [Fact]
        public void ProtectedReadRetainsDataAndSecuritySnapshotAndCreatesAFreshAcl()
        {
            var factory = this.ReadFactory(out var identity);
            var user = (User)this.Transaction.Instantiate(identity.UserId);
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Read source").WithPermission(permission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Before").Build();
            source.AddSecurityToken(token);
            this.Transaction.Derive();
            this.Transaction.Commit();

            using var read = factory.Open("Default", identity, null);
            var readSource = (TrimFrom)read.Transaction.Instantiate(source.Id);
            var version = readSource.Strategy.ObjectVersion;
            Assert.True(read.AccessControl[readSource].CanRead(this.M.TrimFrom.Name));
            var grantVersion = Assert.Single(read.AccessControl[readSource].Grants).Version;

            source.Name = "After";
            role.Permissions = Array.Empty<Permission>();
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.Equal("Before", readSource.Name);
            Assert.Equal(version, readSource.Strategy.ObjectVersion);
            Assert.True(read.AccessControl[readSource].CanRead(this.M.TrimFrom.Name));
            Assert.Equal(grantVersion, Assert.Single(read.AccessControl[readSource].Grants).Version);
            read.EnsureValid();

            using var next = factory.Open("Default", identity, null);
            var nextSource = (TrimFrom)next.Transaction.Instantiate(source.Id);
            Assert.NotSame(read.AccessControl, next.AccessControl);
            Assert.Equal("After", nextSource.Name);
            Assert.False(next.AccessControl[nextSource].CanRead(this.M.TrimFrom.Name));
            next.Complete();
            read.Complete();
        }

        [Fact]
        public void ProtectedReadCannotRecoverFromACaughtResolverMutation()
        {
            var factory = this.ReadFactory(out var identity);
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", transaction =>
            {
                var user = (User)transaction.Instantiate(identity.UserId);
                Assert.Throws<InvalidOperationException>(() => user.UserName = "forbidden");
                return user;
            }, null));
            using var next = factory.Open("Default", identity, null);
            Assert.NotEqual("forbidden", ((User)next.Transaction.Instantiate(identity.UserId)).UserName);
            next.Complete();
        }

        [Fact]
        public void ProtectedReadIdentityCannotChangeAndBeRestoredDuringAnAttempt()
        {
            var factory = this.ReadFactory(out var identity);
            using var read = factory.Open("Default", identity, null);
            var service = read.Transaction.Services.Get<Allors.Database.Services.IUserService>();
            var original = service.User;
            service.User = null;
            service.User = original;
            Assert.Throws<InvalidOperationException>(() => read.EnsureValid());
            Assert.Throws<InvalidOperationException>(() => read.Complete());
        }

        [Fact]
        public void ProtectedReadObservesCancellationBeforeAdmissionAndCompletion()
        {
            var factory = this.ReadFactory(out var identity);
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            var called = false;
            Assert.Throws<OperationCanceledException>(() => factory.Open("Default", transaction =>
            {
                called = true;
                return (User)transaction.Instantiate(identity.UserId);
            }, null, cancellationToken: cancelled.Token));
            Assert.False(called);

            using var during = new CancellationTokenSource();
            using var read = factory.Open("Default", identity, null, cancellationToken: during.Token);
            during.Cancel();
            Assert.Throws<OperationCanceledException>(() => read.Complete());
            using var next = factory.Open("Default", identity, null);
            next.Complete();
        }

        [Fact]
        public void ProtectedReadUsesCapturedHttpIdentityInsideItsOwnSqlTransaction()
        {
            var factory = this.ReadFactory(out var identity);
            var claims = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, identity.UserId.ToString()),
            }, "Tests");
            var principal = new System.Security.Claims.ClaimsPrincipal(claims);
            Allors.Database.ITransaction resolvedTransaction = null;
            var resolver = new ProtectedReadUserResolver((captured, transaction) =>
            {
                resolvedTransaction = transaction;
                var id = long.Parse(captured.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                return (User)transaction.Instantiate(id);
            });
            var capturedIdentity = Allors.Services.ReadUserIdentity.Capture(principal, resolver);
            claims.RemoveClaim(claims.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier));
            claims.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, long.MaxValue.ToString()));

            using var read = factory.Open("Default", capturedIdentity.Resolve, null);
            Assert.Equal(identity.UserId, read.Scope.u);
            Assert.Same(read.Transaction, resolvedTransaction);
            Assert.NotSame(this.Transaction, resolvedTransaction);
            read.Complete();
        }

        [Fact]
        public void ProtectedReadUserNeedNotBeExposedInTheSelectedWorkspace()
        {
            this.ReadFactory(out var identity);
            Assert.DoesNotContain("X", this.Transaction.Instantiate(identity.UserId).Strategy.Class.WorkspaceNames);
            var factory = new ReadScopeFactory(this.Transaction.Database, new ReadConfiguration(
                new System.Collections.Generic.Dictionary<string, string> { ["X"] = "revision-X" }));
            using var read = factory.Open("X", identity, null);
            Assert.Equal(identity.UserId, read.Scope.u);
            Assert.Equal("X", read.Scope.w);
            read.Complete();
        }

        [Fact]
        public void ProtectedReadRejectsADeletedLocalUser()
        {
            var factory = this.ReadFactory(out var identity);
            var user = new PersonBuilder(this.Transaction).WithUserName("deleted read user").Build();
            this.Transaction.Derive();
            this.Transaction.Commit();
            var deletedIdentity = new LocalReadIdentity(identity.Population, user.Id);
            user.Delete();
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.Throws<InvalidOperationException>(() => factory.Open("Default", deletedIdentity, null));
        }

        [Fact]
        public void ProtectedReadAbandonsAResolverFailureWithoutRetrying()
        {
            var factory = this.ReadFactory(out var identity);
            var failure = new InvalidOperationException("Resolver failed");
            var calls = 0;
            Allors.Database.ITransaction failedTransaction = null;
            Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => factory.Open("Default", transaction =>
            {
                calls++;
                failedTransaction = transaction;
                throw failure;
            }, null)));
            Assert.Equal(1, calls);
            Assert.ThrowsAny<InvalidOperationException>(() => failedTransaction.Instantiate(identity.UserId));
            using var next = factory.Open("Default", identity, null);
            next.Complete();
        }

        [Fact]
        public void ProtectedReadRejectsMemoryBeforeResolvingIdentity()
        {
            var memory = new Allors.Database.Adapters.Memory.Database(
                new Allors.Database.Configuration.DefaultDatabaseServices(
                    new Allors.Database.Configuration.Derivations.Default.Engine(Allors.Database.Domain.Rules.Create(this.M))),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(this.M, typeof(User)),
                });
            var configuration = new ReadConfiguration(new System.Collections.Generic.Dictionary<string, string> { ["Default"] = "revision-1" });
            Assert.Throws<NotSupportedException>(() => new ReadScopeFactory(memory, configuration));
        }

        [Theory]
        [InlineData("Api")]
        [InlineData("Pull")]
        [InlineData("Sync")]
        [InlineData("Access")]
        [InlineData("Permission")]
        [InlineData("Push")]
        [InlineData("Invoke")]
        public void WorkspaceProtocolConstructorsRejectMemory(string subject)
        {
            var memory = new Allors.Database.Adapters.Memory.Database(
                new Allors.Database.Configuration.DefaultDatabaseServices(
                    new Allors.Database.Configuration.Derivations.Default.Engine(Rules.Create(this.M))),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(this.M, typeof(User)),
                });
            memory.Init();
            new Setup(memory, new Config()).Apply();

            using var transaction = memory.CreateTransaction();
            var user = new PersonBuilder(transaction).WithUserName("Memory workspace rejection").Build();
            transaction.Derive();
            transaction.Commit();

            var transactionServices = transaction.Services;
            var databaseServices = memory.Services;
            transactionServices.Get<Allors.Database.Services.IUserService>().User = user;
            var accessControl = transactionServices.Get<Allors.Database.Services.IWorkspaceAclsService>().Create("Default");
            var metaCache = databaseServices.Get<Allors.Database.Services.IMetaCache>();
            var allowedClasses = metaCache.GetWorkspaceClasses("Default");
            var roleTypesByClass = metaCache.GetWorkspaceRoleTypesByClass("Default");
            var security = databaseServices.Get<Allors.Database.Domain.ISecurity>();
            var ranges = databaseServices.Get<Allors.Ranges.IRanges<long>>();
            var prefetch = databaseServices.Get<Allors.Database.Domain.IPrefetchPolicyCache>();
            var prefetchPolicyByClass = prefetch.WorkspacePrefetchPolicyByClass("Default");
            var unitConvert = new UnitConvert();
            var dependencies = new System.Collections.Generic.Dictionary<IClass, System.Collections.Generic.ISet<IPropertyType>>();
            var derivationService = databaseServices.Get<Allors.Database.Derivations.IDerivationService>();
            Func<Allors.Database.Derivations.IValidation> derive = () => derivationService.CreateDerivation(transaction).Derive();
            var objectBuilder = transactionServices.Get<Allors.Database.Services.IObjectBuilderService>();
            Func<IClass, Allors.Database.IObject> build = @class => objectBuilder.Build(@class);
            Func<object> construct = subject switch
            {
                "Api" => () => new Api(transaction, "Default", CancellationToken.None),
                "Pull" => () => new PullResponseBuilder(transaction, accessControl, allowedClasses, unitConvert, ranges, dependencies, prefetch, CancellationToken.None),
                "Sync" => () => new SyncResponseBuilder(transaction, accessControl, allowedClasses, roleTypesByClass, prefetchPolicyByClass, unitConvert, ranges),
                "Access" => () => new AccessResponseBuilder(transaction, security, user, "Default"),
                "Permission" => () => new PermissionResponseBuilder(transaction, allowedClasses),
                "Push" => () => new PushResponseBuilder(transaction, derive, this.M, accessControl, allowedClasses, build, unitConvert),
                "Invoke" => () => new InvokeResponseBuilder(transaction, derive, accessControl, allowedClasses),
                _ => throw new ArgumentOutOfRangeException(nameof(subject)),
            };

            var exception = Assert.Throws<NotSupportedException>(() => construct());
            Assert.Contains("PostgreSQL", exception.Message);
            Assert.Contains("SQL Server", exception.Message);
        }

        private sealed class ProtectedReadUserResolver : Allors.Services.IUserResolver
        {
            private readonly Func<System.Security.Claims.ClaimsPrincipal, Allors.Database.ITransaction, User> resolve;

            internal ProtectedReadUserResolver(Func<System.Security.Claims.ClaimsPrincipal, Allors.Database.ITransaction, User> resolve) => this.resolve = resolve;

            public User Resolve(System.Security.Claims.ClaimsPrincipal principal, Allors.Database.ITransaction transaction) => this.resolve(principal, transaction);
        }

        private ReadScopeFactory ReadFactory(out LocalReadIdentity identity)
        {
            this.Transaction.Rollback();
            var population = ((Allors.Database.Adapters.Sql.Database)this.Transaction.Database).PopulationLifecycle.Renew();
            var user = (User)this.SetUser("jane@example.com");
            identity = new LocalReadIdentity(population.Incarnation, user.Id);
            return new ReadScopeFactory(this.Transaction.Database, new ReadConfiguration(
                new System.Collections.Generic.Dictionary<string, string> { ["Default"] = "revision-1" }));
        }

        [Fact]
        public void CommittedRolePermissionChangesAdvanceGrantProvenanceWithoutChangingSourceVersion()
        {
            var user = (User)this.SetUser("jane@example.com");
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Read source").WithPermission(permission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").Build();
            source.AddSecurityToken(token);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new PullRequest { l = new[] { new Pull { Object = source }.ToJson(this.UnitConvert) } };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var before = api.Pull(request);
            var sourceVersion = Assert.Single(before.p).v;
            var grantVersion = Assert.Single(before.g)[1];
            var tokenVersion = token.Strategy.ObjectVersion;
            var tokenStamp = token.SecurityStamp;
            Assert.True(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
            Assert.Contains(permission.Id, Assert.Single(api.AccessControl[source].Grants).PermissionSet);

            role.Permissions = Array.Empty<Permission>();
            this.Transaction.Derive();
            this.Transaction.Commit();

            api = new Api(this.Transaction, "Default", CancellationToken.None);
            var after = api.Pull(request);
            Assert.False(after.HasErrors);
            Assert.Equal(sourceVersion, Assert.Single(after.p).v);
            Assert.NotEqual(grantVersion, grant.Strategy.ObjectVersion);
            Assert.NotEqual(tokenVersion, token.Strategy.ObjectVersion);
            Assert.NotEqual(tokenStamp, token.SecurityStamp);
            Assert.Equal(grant.Strategy.ObjectVersion, Assert.Single(after.g)[1]);
            Assert.Empty(Assert.Single(api.AccessControl[source].Grants).PermissionSet);
            Assert.False(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
        }

        [Fact]
        public void CommittedGroupMembershipChangesRemoveApplicableGrantWithoutChangingSourceVersion()
        {
            var user = (User)this.SetUser("jane@example.com");
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Read source").WithPermission(permission).Build();
            var group = new UserGroupBuilder(this.Transaction).WithName("Readers").WithMember(user).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubjectGroup(group).WithRole(role).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").Build();
            source.AddSecurityToken(token);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new PullRequest { l = new[] { new Pull { Object = source }.ToJson(this.UnitConvert) } };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var before = api.Pull(request);
            var sourceVersion = Assert.Single(before.p).v;
            var grantVersion = Assert.Single(before.g)[1];
            var tokenVersion = token.Strategy.ObjectVersion;
            var tokenStamp = token.SecurityStamp;
            Assert.True(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));

            group.RemoveMember(user);
            this.Transaction.Derive();
            this.Transaction.Commit();

            api = new Api(this.Transaction, "Default", CancellationToken.None);
            var after = api.Pull(request);
            Assert.False(after.HasErrors);
            Assert.Equal(sourceVersion, Assert.Single(after.p).v);
            Assert.NotEqual(grantVersion, grant.Strategy.ObjectVersion);
            Assert.NotEqual(tokenVersion, token.Strategy.ObjectVersion);
            Assert.NotEqual(tokenStamp, token.SecurityStamp);
            Assert.Empty(grant.EffectiveUsers);
            Assert.Null(after.g);
            Assert.Empty(api.AccessControl[source].Grants);
            Assert.False(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
        }

        [Fact]
        public void CommittedEquivalentRoleReplacementRefreshesGrantVersion()
        {
            var user = (User)this.SetUser("jane@example.com");
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var firstRole = new RoleBuilder(this.Transaction).WithName("First reader role").WithPermission(permission).Build();
            var secondRole = new RoleBuilder(this.Transaction).WithName("Second reader role").WithPermission(permission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(firstRole).Build();
            var token = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").Build();
            source.AddSecurityToken(token);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new PullRequest { l = new[] { new Pull { Object = source }.ToJson(this.UnitConvert) } };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var before = api.Pull(request);
            var sourceVersion = Assert.Single(before.p).v;
            var grantVersion = Assert.Single(before.g)[1];
            var tokenVersion = token.Strategy.ObjectVersion;
            var tokenStamp = token.SecurityStamp;

            grant.Role = secondRole;
            this.Transaction.Derive();
            this.Transaction.Commit();

            api = new Api(this.Transaction, "Default", CancellationToken.None);
            var after = api.Pull(request);
            Assert.False(after.HasErrors);
            Assert.Equal(sourceVersion, Assert.Single(after.p).v);
            Assert.NotEqual(grantVersion, grant.Strategy.ObjectVersion);
            Assert.Equal(tokenVersion, token.Strategy.ObjectVersion);
            Assert.Equal(tokenStamp, token.SecurityStamp);
            Assert.True(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
            Assert.Equal(new[] { permission.Id }, Assert.Single(api.AccessControl[source].Grants).PermissionSet);

            // The unchanged token stamp must not hide the current grant version, even
            // when the permission body is equivalent to the previous committed body.
            Assert.NotEqual(grantVersion, Assert.Single(after.g)[1]);
            Assert.Equal(grant.Strategy.ObjectVersion, Assert.Single(after.g)[1]);

            var freshSecurity = new Allors.Database.Configuration.Security(
                (Allors.Database.Configuration.DatabaseServices)this.Transaction.Database.Services);
            var freshAcl = new WorkspaceAccessControl(freshSecurity, user, "Default")[source];
            Assert.Equal(grant.Strategy.ObjectVersion, Assert.Single(freshAcl.Grants).Version);
            Assert.Equal(new[] { permission.Id }, Assert.Single(freshAcl.Grants).PermissionSet);
            Assert.True(freshAcl.CanRead(this.M.TrimFrom.Name));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CommittedDelegatedMembershipChangesRefreshAclWithoutChangingSourceVersion(bool replaceRevocation)
        {
            var user = (User)this.SetUser("jane@example.com");
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Workspace permission marker").WithPermission(permission).Build();
            var firstGrant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var secondGrant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            var firstToken = new SecurityTokenBuilder(this.Transaction).WithGrant(firstGrant).Build();
            var secondToken = new SecurityTokenBuilder(this.Transaction).WithGrant(secondGrant).Build();
            var firstRevocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build();
            var secondRevocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build();
            var delegated = new AccessClassBuilder(this.Transaction).Build();
            delegated.AddSecurityToken(firstToken);
            delegated.AddRevocation(firstRevocation);
            var source = new AccessClassBuilder(this.Transaction).WithDelegatedAccess(delegated).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            // AccessClass is a database-only test type. Exercise the same workspace ACL used
            // by Pull directly, with a workspace permission marker; this is not a Pull pool test.
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var before = api.AccessControl[source];
            var sourceVersion = source.Strategy.ObjectVersion;
            var delegatedVersion = delegated.Strategy.ObjectVersion;
            var firstGrantVersion = firstGrant.Strategy.ObjectVersion;
            var secondGrantVersion = secondGrant.Strategy.ObjectVersion;
            var firstRevocationVersion = firstRevocation.Strategy.ObjectVersion;
            var secondRevocationVersion = secondRevocation.Strategy.ObjectVersion;
            Assert.Equal(firstGrant.Id, Assert.Single(before.Grants).Id);
            Assert.Equal(firstRevocation.Id, Assert.Single(before.Revocations).Id);

            if (replaceRevocation)
            {
                delegated.Revocations = new[] { secondRevocation };
            }
            else
            {
                delegated.SecurityTokens = new[] { secondToken };
            }

            this.Transaction.Derive();
            this.Transaction.Commit();

            api = new Api(this.Transaction, "Default", CancellationToken.None);
            var after = api.AccessControl[source];
            Assert.Equal(sourceVersion, source.Strategy.ObjectVersion);
            Assert.NotEqual(delegatedVersion, delegated.Strategy.ObjectVersion);
            Assert.Equal(firstGrantVersion, firstGrant.Strategy.ObjectVersion);
            Assert.Equal(secondGrantVersion, secondGrant.Strategy.ObjectVersion);
            Assert.Equal(firstRevocationVersion, firstRevocation.Strategy.ObjectVersion);
            Assert.Equal(secondRevocationVersion, secondRevocation.Strategy.ObjectVersion);
            Assert.Equal(replaceRevocation ? firstGrant.Id : secondGrant.Id, Assert.Single(after.Grants).Id);
            Assert.Equal(replaceRevocation ? secondRevocation.Id : firstRevocation.Id, Assert.Single(after.Revocations).Id);
            Assert.Contains(permission.Id, Assert.Single(after.Grants).PermissionSet);
            Assert.Contains(permission.Id, Assert.Single(after.Revocations).PermissionSet);
        }

        [Fact]
        public void CommittedRevocationBodyChangesRefreshProvenanceWithoutChangingSourceVersion()
        {
            this.SetUser("jane@example.com");
            var permissions = new Permissions(this.Transaction);
            var namePermission = permissions.Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            var relationPermission = permissions.Get(this.M.TrimFrom, this.M.TrimFrom.Many2One, Operations.Read);
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(relationPermission).Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Source").Build();
            source.AddRevocation(revocation);
            this.Transaction.Derive();
            this.Transaction.Commit();

            var request = new PullRequest { l = new[] { new Pull { Object = source }.ToJson(this.UnitConvert) } };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var before = api.Pull(request);
            var sourceVersion = Assert.Single(before.p).v;
            var revocationVersion = Assert.Single(before.r)[1];
            Assert.True(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
            Assert.False(api.AccessControl[source].CanRead(this.M.TrimFrom.Many2One));

            revocation.DeniedPermissions = new[] { namePermission };
            this.Transaction.Derive();
            this.Transaction.Commit();

            api = new Api(this.Transaction, "Default", CancellationToken.None);
            var after = api.Pull(request);
            Assert.False(after.HasErrors);
            Assert.Equal(sourceVersion, Assert.Single(after.p).v);
            Assert.NotEqual(revocationVersion, revocation.Strategy.ObjectVersion);
            Assert.Equal(revocation.Strategy.ObjectVersion, Assert.Single(after.r)[1]);
            Assert.Equal(new[] { namePermission.Id }, Assert.Single(api.AccessControl[source].Revocations).PermissionSet);
            Assert.False(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
            Assert.True(api.AccessControl[source].CanRead(this.M.TrimFrom.Many2One));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PullRetainsObjectsWithUnreadableNames(bool useExtent)
        {
            this.SetUser("jane@example.com");
            var source = new TrimFromBuilder(this.Transaction).WithName("Unreadable root").Build();
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, this.M.TrimFrom.Name, Operations.Read);
            source.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = useExtent
                ? new Pull { Extent = new Extent(this.M.TrimFrom) }
                : new Pull { Object = source };
            pull.Results = new[] { new Result { Name = "roots" } };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);

            var response = api.Pull(new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) } });

            Assert.False(response.HasErrors);
            Assert.Contains(response.p, v => v.i == source.Id);
            if (useExtent)
            {
                Assert.Contains(source.Id, response.c["roots"]);
            }
            else
            {
                Assert.Equal(source.Id, response.o["roots"]);
            }

            Assert.False(api.AccessControl[source].CanRead(this.M.TrimFrom.Name));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void PullRetainsRelatedObjectsWithUnreadableNames(bool useDependencies, bool toMany)
        {
            this.SetUser("jane@example.com");
            var target = new TrimToBuilder(this.Transaction).WithName("Unreadable target").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Readable source").Build();
            source.Many2One = target;
            source.AddMany2Many(target);
            var permission = new Permissions(this.Transaction).Get(this.M.TrimTo, this.M.TrimTo.Name, Operations.Read);
            target.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();

            IRoleType relation = toMany ? this.M.TrimFrom.Many2Manies : this.M.TrimFrom.Many2One;
            var pull = new Pull
            {
                Object = source,
                Results = new[]
                {
                    new Result { Name = "source", Include = useDependencies ? null : new[] { new Node(relation) } },
                },
            };
            var request = new PullRequest
            {
                l = new[] { pull.ToJson(this.UnitConvert) },
                d = useDependencies ? new[] { new PullDependency { o = this.M.TrimFrom.Tag, r = relation.RelationType.Tag } } : null,
            };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);

            var response = api.Pull(request);

            Assert.False(response.HasErrors);
            Assert.Equal(source.Id, response.o["source"]);
            Assert.Equal(new[] { source.Id, target.Id }.OrderBy(v => v), response.p.Select(v => v.i).OrderBy(v => v));
            Assert.True(api.AccessControl[source].CanRead(relation));
            Assert.False(api.AccessControl[target].CanRead(this.M.TrimTo.Name));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PullDoesNotTraverseUnreadableRoleIncludes(bool toMany)
        {
            this.SetUser("jane@example.com");
            var target = new TrimToBuilder(this.Transaction).WithName("Readable target").Build();
            var source = new TrimFromBuilder(this.Transaction).WithName("Readable source").Build();
            source.Many2One = target;
            source.AddMany2Many(target);
            IRoleType relation = toMany ? this.M.TrimFrom.Many2Manies : this.M.TrimFrom.Many2One;
            var permission = new Permissions(this.Transaction).Get(this.M.TrimFrom, relation, Operations.Read);
            source.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull
            {
                Object = source,
                Results = new[] { new Result { Name = "source", Include = new[] { new Node(relation) } } },
            };
            var api = new Api(this.Transaction, "Default", CancellationToken.None);

            var response = api.Pull(new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) } });

            Assert.False(response.HasErrors);
            Assert.Equal(source.Id, Assert.Single(response.p).i);
            Assert.False(api.AccessControl[source].CanRead(relation));
        }

        [Fact]
        public void SameWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Extent(m.WorkspaceXObject1) };
                var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

                var api = new Api(this.Transaction, "X", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                var wx1s = pullResponse.c["WorkspaceXObject1s"];

                Assert.Single(wx1s);

                var wx1 = wx1s.First();

                Assert.Equal(x1.Id, wx1);
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l = new[]
                                {
                    new Allors.Protocol.Json.Data.Pull
                    {
                        o = x1.Id,
                    },
                },
                };

                var api = new Api(this.Transaction, "X", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                var wx1 = pullResponse.o["WorkspaceXObject1"];

                Assert.Equal(x1.Id, wx1);
            }
        }

        [Fact]
        public void DifferentWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Extent(m.WorkspaceXObject1) };

                var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) } };

                var api = new Api(this.Transaction, "Y", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.False(pullResponse.c.ContainsKey("WorkspaceXObject1s"));
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l = new[]
                    {
                        new Allors.Protocol.Json.Data.Pull
                        {
                            o = x1.Id,
                        },
                    },
                };

                var api = new Api(this.Transaction, "Y", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                Assert.Empty(pullResponse.o);
            }
        }

        [Fact]
        public void NoneWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Extent(m.WorkspaceXObject1) };
                var pullRequest = new PullRequest { l = new[] { pull.ToJson(this.UnitConvert) }, };

                var api = new Api(this.Transaction, "None", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.False(pullResponse.c.ContainsKey("WorkspaceXObject1s"));
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l = new[]
                    {
                        new Allors.Protocol.Json.Data.Pull
                        {
                            o = x1.Id,
                        },
                    },
                };

                var api = new Api(this.Transaction, "None", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.Empty(pullResponse.o);
            }
        }

        [Fact]
        public void WithDeniedPermissions()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();
            var permissions = new Permissions(this.Transaction).Extent();
            var permission = permissions.First(v => Equals(v.Class, this.M.Data) && v.InWorkspace("Default"));
            var revocation = new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build();
            data.AddRevocation(revocation);

            this.Transaction.Commit();

            var pull = new Pull { Extent = new Extent(m.Data) };
            var pullRequest = new PullRequest
            {
                l = new[]
                {
                    pull.ToJson(this.UnitConvert)
                },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
            Assert.Equal(acl.Revocations.Select(v => v.Id), @object.r);
        }

        [Fact]
        public async void WithoutDeniedPermissions()
        {
            var user = this.SetUser("jane@example.com");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var uri = new Uri(@"allors/pull", UriKind.Relative);

            var pull = new Pull { Extent = new Extent(this.M.Data) };

            var pullRequest = new PullRequest
            {
                l = new[]
                      {
                          pull.ToJson(this.UnitConvert)
                      },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
        }

        [Fact]
        public void WithoutDeniedPermissionsAsAgent()
        {
            var user = this.SetUser("agent");

            var data = new DataBuilder(this.Transaction).WithString("First").Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Pull { Extent = new Extent(this.M.Data) };

            var pullRequest = new PullRequest
            {
                l = new[]
                      {
                          pull.ToJson(this.UnitConvert)
                      },
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.NotEmpty(@object.g);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
        }
    }
}
