// <copyright file="PullRetryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using Allors.Database;
    using Allors.Database.Adapters.Sql.Tracing;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Server.Controllers;
    using Allors.Services;
    using Microsoft.AspNetCore.Mvc;
    using Polly;
    using Xunit;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using SqlDatabase = Allors.Database.Adapters.Sql.Database;
    using PushOptions = Allors.Workspace.PushOptions;
    using MutationOutcome = Allors.Workspace.MutationOutcome;
    using MutationReadStatus = Allors.Workspace.MutationReadStatus;
    using MutationAcknowledgementStatus = Allors.Workspace.MutationAcknowledgementStatus;
    using MutationRefreshApplicationStatus = Allors.Workspace.MutationRefreshApplicationStatus;

    public class PullRetryTests : ApiTest, IClassFixture<Fixture>
    {
        public PullRetryTests(Fixture fixture) : base(fixture)
        {
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void LegacyPullDoesNotExecuteRemovedProcedurePayload(string serializer)
        {
            var database = this.Transaction.Database;
            this.SetUser("jane@example.com");
            Assert.Empty(new UnitSamples(this.Transaction).Extent());
            const string json = "{\"l\":[],\"p\":{\"n\":\"TestUnitSamples\",\"v\":{\"step\":\"1\"}}}";
            var request = serializer == "SystemText"
                ? System.Text.Json.JsonSerializer.Deserialize<PullRequest>(json)
                : Newtonsoft.Json.JsonConvert.DeserializeObject<PullRequest>(json);
            var controller = new PullController(new StubTransactionService { Transaction = this.Transaction }, new StubWorkspaceService { Name = "Default" }, new PolicyService());

            var response = controller.Post(request, CancellationToken.None).Value;

            using var verification = database.CreateTransaction();
            Assert.Empty(new UnitSamples(verification).Extent());
            Assert.False(response.HasErrors);
            Assert.Empty(response.c);
            Assert.Empty(response.o);
            var serialized = serializer == "SystemText" ? System.Text.Json.JsonSerializer.Serialize(response) : Newtonsoft.Json.JsonConvert.SerializeObject(response);
            using var document = System.Text.Json.JsonDocument.Parse(serialized);
            Assert.False(document.RootElement.TryGetProperty("v", out _));
            Assert.Empty(response.p);
        }

        [Theory]
        [InlineData("remove-c1-string-read-grant", "zero")]
        [InlineData("remove-c1-string-read-grant", "negative")]
        [InlineData("remove-c1-string-read-grant", "missing")]
        [InlineData("remove-c1-string-read-grant", "wrong-type")]
        [InlineData("add-denied-property-read-revocation", "zero")]
        [InlineData("add-denied-property-read-revocation", "negative")]
        [InlineData("add-denied-property-read-revocation", "missing")]
        [InlineData("add-denied-property-read-revocation", "wrong-type")]
        [InlineData("remove-denied-property-read-revocation", "zero")]
        [InlineData("remove-denied-property-read-revocation", "negative")]
        [InlineData("remove-denied-property-read-revocation", "missing")]
        [InlineData("remove-denied-property-read-revocation", "wrong-type")]
        public void PermissionSetupRejectsInvalidTargetsWithoutCommit(string action, string invalidTarget)
        {
            this.SetUser("jane@example.com");
            var baseline = this.CapturePermissionSetupBaseline();
            var targetId = invalidTarget switch
            {
                "zero" => 0,
                "negative" => -1,
                "missing" => 9007199254740991,
                _ => action == "remove-c1-string-read-grant" ? baseline.Denied.Id : baseline.C1.Id,
            };
            baseline.Unrelated.Name = "Unrelated pending edit";
            var controller = new TestPermissionController(new StubTransactionService { Transaction = this.Transaction });

            var result = Assert.IsType<BadRequestObjectResult>(ExecutePermissionSetup(controller, action, targetId));

            var message = Assert.IsType<string>(result.Value);
            Assert.Contains("targetId", message);
            Assert.Contains(invalidTarget == "zero" || invalidTarget == "negative" ? "positive" : "existing", message);
            baseline.AssertSecurityUnchanged(this.Transaction);
            this.AssertPermissionSetupUncommitted(baseline);
        }

        [Theory]
        [InlineData("remove-c1-string-read-grant")]
        [InlineData("add-denied-property-read-revocation")]
        [InlineData("remove-denied-property-read-revocation")]
        public void PermissionSetupRejectsInvalidSecurityPreconditionsWithoutCommit(string action)
        {
            this.SetUser("jane@example.com");
            var c1 = new C1s(this.Transaction).Extent().First(value => value.Name == "c1B");
            var denied = new Denieds(this.Transaction).Extent().First;
            if (action == "remove-c1-string-read-grant")
            {
                var permission = new Allors.Database.Domain.Permissions(this.Transaction).Get(this.M.C1, this.M.C1.C1AllorsString, Allors.Operations.Read);
                var grants = this.Transaction.Services.Get<Allors.Database.Services.IWorkspaceAclsService>().Create("Default")[c1].Grants;
                foreach (var grant in grants)
                {
                    ((Grant)this.Transaction.Instantiate(grant.Id)).Role.RemovePermission(permission);
                }
            }
            else if (action == "add-denied-property-read-revocation")
            {
                denied.RemoveRevocations();
            }
            else
            {
                var permission = new Allors.Database.Domain.Permissions(this.Transaction).Get(this.M.Denied, this.M.Denied.DefaultWorkspaceProperty, Allors.Operations.Read);
                denied.AddRevocation(new RevocationBuilder(this.Transaction).WithDeniedPermission(permission).Build());
            }

            var baseline = this.CapturePermissionSetupBaseline();
            baseline.Unrelated.Name = "Unrelated pending edit";
            var controller = new TestPermissionController(new StubTransactionService { Transaction = this.Transaction });

            var result = Assert.IsType<BadRequestObjectResult>(ExecutePermissionSetup(controller, action, baseline.TargetId(action)));

            var message = Assert.IsType<string>(result.Value);
            Assert.Contains(action == "remove-c1-string-read-grant" ? "effective grant" : "exactly one Revocation", message);
            baseline.AssertSecurityUnchanged(this.Transaction);
            this.AssertPermissionSetupUncommitted(baseline);
        }

        [Theory]
        [InlineData("remove-c1-string-read-grant")]
        [InlineData("add-denied-property-read-revocation")]
        [InlineData("remove-denied-property-read-revocation")]
        public void PermissionSetupRequiresResolvedUser(string action)
        {
            this.SetUser("jane@example.com");
            var baseline = this.CapturePermissionSetupBaseline();
            this.Transaction.Services.Get<Allors.Database.Services.IUserService>().User = null;
            baseline.Unrelated.Name = "Unrelated pending edit";
            var controller = new TestPermissionController(new StubTransactionService { Transaction = this.Transaction });

            Assert.IsType<UnauthorizedResult>(ExecutePermissionSetup(controller, action, baseline.TargetId(action)));

            baseline.AssertSecurityUnchanged(this.Transaction);
            this.AssertPermissionSetupUncommitted(baseline);
        }

        [Theory]
        [InlineData("remove-c1-string-read-grant")]
        [InlineData("add-denied-property-read-revocation")]
        [InlineData("remove-denied-property-read-revocation")]
        public void PermissionSetupLeavesFailedDerivationUncommitted(string action)
        {
            this.SetUser("jane@example.com");
            if (action == "remove-denied-property-read-revocation")
            {
                var denied = new Denieds(this.Transaction).Extent().First;
                var permission = new Allors.Database.Domain.Permissions(this.Transaction).Get(this.M.Denied, this.M.Denied.DefaultWorkspaceProperty, Allors.Operations.Read);
                Assert.Single(denied.Revocations).AddDeniedPermission(permission);
            }

            var baseline = this.CapturePermissionSetupBaseline();
            Assert.Empty(new ValiDatas(this.Transaction).Extent());
            var invalid = new ValiDataBuilder(this.Transaction).Build();
            Assert.False(invalid.ExistRequiredPerson);
            baseline.Unrelated.Name = "Unrelated pending edit";
            var controller = new TestPermissionController(new StubTransactionService { Transaction = this.Transaction });

            var result = Assert.IsType<BadRequestObjectResult>(ExecutePermissionSetup(controller, action, baseline.TargetId(action)));

            Assert.Contains("validation", Assert.IsType<string>(result.Value));
            Assert.Empty(new ValiDatas(this.Transaction).Extent());
            Assert.Equal(baseline.UnrelatedName, baseline.Unrelated.Name);
            baseline.AssertSecurityUnchanged(this.Transaction);
            this.AssertPermissionSetupUncommitted(baseline);
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
                        e = new Allors.Protocol.Json.Data.Extent
                        {
                            k = Allors.Protocol.Json.Data.ExtentKind.Filter,
                            t = this.M.Organisation.Tag,
                            p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.Organisation.Name.RelationType.Tag, p = "name" },
                        },
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                },
            };

            var pullResponse = controller.Post(pullRequest, CancellationToken.None).Value;

            Assert.True(failed);
            Assert.Single(pullResponse.c["Organisations"]);
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ReadEntrypointsReturnTheSameDetachedCompleteResult(string transport)
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var user = this.SetUser("jane@example.com");
            var target = new Allors.Database.Domain.C1s(this.Transaction).Extent().First(value => value.Name == "c1B");
            this.Transaction.Derive();
            this.Transaction.Commit();
            var population = database.PopulationLifecycle.Renew();
            var executor = new ReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "test-read-v1" }));
            var identity = new LocalReadIdentity(population.Incarnation, user.Id);
            var input = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "transport", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[]
                    {
                        new Pull { t = this.M.C1.Tag, o = target.Id, r = new[] { new Allors.Protocol.Json.Data.Result { n = "target" } } },
                        new Pull { t = this.M.C1.Tag, o = 9007199254740991, r = new[] { new Allors.Protocol.Json.Data.Result { n = "missing" } } },
                    },
                    d = System.Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };
            var request = System.Text.Json.JsonSerializer.Serialize(input);
            var local = new Allors.Workspace.Adapters.Local.ReadTransport(executor, "Default", identity);
            var expected = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(await local.Read(request));
            var handler = new ReadControllerHandler(executor, user.Id);
            using var http = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            object actual = transport switch
            {
                "Local" => await local.Read(request),
                "SystemText" => await new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(http).Read(request),
                _ => await new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(http).Read(request),
            };
            var complete = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(actual);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Json.JsonSerializer.Serialize(complete));
            Assert.Equal(identity.Population.ToString("D"), complete.sc.p);
            Assert.Equal(user.Id, complete.sc.u);
            Assert.Equal("test-read-v1", complete.sc.r);
            Assert.Single(complete.p);
            Assert.Equal(target.Id, complete.o["target"]);
            Assert.Null(complete.o["missing"]);
            Assert.Equal("unavailable", complete.l[1].s);
            Assert.NotEmpty(complete.s);
            Assert.NotEmpty(complete.pm);
            var serialized = transport == "Newtonsoft" ? Newtonsoft.Json.JsonConvert.SerializeObject(complete) : System.Text.Json.JsonSerializer.Serialize(complete);
            using var document = System.Text.Json.JsonDocument.Parse(serialized);
            Assert.False(document.RootElement.TryGetProperty("v", out _));
            Assert.Equal(transport == "Local" ? 0 : 1, handler.Attempts);
            Assert.Equal(handler.Attempts, handler.WorkspaceReads);
            Assert.Equal(handler.Attempts, handler.Resolutions);

            input.sc = complete.sc;
            input.h = new Allors.Protocol.Json.Api.Read.ReadCache
            {
                s = complete.s.Select(value => new Allors.Protocol.Json.Api.Read.ReadSecurityHint { i = value.i, k = value.k, v = value.v, h = true }).ToArray(),
                r = complete.p.Select(value => new Allors.Protocol.Json.Api.Read.ReadRecordHeader { i = value.i, c = value.c, v = value.v, g = value.g, r = value.r }).ToArray(),
                p = complete.pm.Select(value => value.i).ToArray(),
            };
            request = System.Text.Json.JsonSerializer.Serialize(input);
            var expectedCached = await local.Read(request);
            var cachedHandler = new ReadControllerHandler(executor, user.Id);
            using var cachedHttp = new System.Net.Http.HttpClient(cachedHandler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            var cached = transport switch
            {
                "Local" => await local.Read(input),
                "SystemText" => await new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(cachedHttp).Read(input),
                _ => await new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(cachedHttp).Read(input),
            };
            var reused = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(cached);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expectedCached), System.Text.Json.JsonSerializer.Serialize(cached));
            Assert.Equal(0, Assert.Single(reused.p).p.r);
            Assert.All(reused.s, value => Assert.NotNull(value.p.r));
            Assert.All(reused.pm, value => Assert.NotNull(value.p.r));
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadControllerRejectsDuplicateFieldsBeforeResolvingIdentity()
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var executor = new ReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "test-read-v1" }));
            var handler = new ReadControllerHandler(executor, 1);
            using var http = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            using var response = await http.PostAsync("read", new System.Net.Http.StringContent("{\"v\":1,\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}"));
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(1, handler.Attempts);
            Assert.Equal(0, handler.Resolutions);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadControllerRequiresExplicitReadConfiguration()
        {
            var controller = new ReadController(new StubWorkspaceService { Name = "Default" }, new ReadResolver(1));
            var result = Assert.IsType<Microsoft.AspNetCore.Mvc.ObjectResult>(await controller.Post(CancellationToken.None));
            Assert.Equal(503, result.StatusCode);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadControllerRejectsAnOversizedBodyBeforeResolvingIdentity()
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var executor = new ReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "test-read-v1" }));
            var handler = new ReadControllerHandler(executor, 1);
            using var http = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            using var response = await http.PostAsync("read", new System.Net.Http.StringContent(new string(' ', ReadExecutor.MaxRequestBytes + 1)));
            Assert.Equal(System.Net.HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
            Assert.Equal(0, handler.Resolutions);
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ReadCoordinatorPreservesCompleteResultsAcrossSqlTransports(string transport)
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var user = this.SetUser("jane@example.com");
            var target = new C1s(this.Transaction).Extent().First(value => value.Name == "c1B");
            this.Transaction.Derive();
            this.Transaction.Commit();
            var population = database.PopulationLifecycle.Renew();
            var executor = new ReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "test-read-v1" }));
            var local = new Allors.Workspace.Adapters.Local.ReadTransport(executor, "Default", new LocalReadIdentity(population.Incarnation, user.Id));
            using var http = new System.Net.Http.HttpClient(new CoordinatorControllerHandler(executor, user.Id)) { BaseAddress = new System.Uri("http://localhost/allors/") };
            Allors.Workspace.Adapters.Remote.IReadTransport inner = transport switch
            {
                "Local" => local,
                "SystemText" => new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(http),
                _ => new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(http),
            };
            var recording = new CoordinatorRecordingTransport(inner);
            var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            await using var coordinator = await ReadTestConnection.CreateAsync(recording, metadata, http, "test-read-v1");
            Assert.Equal(new[] { "discover", "read" }, recording.Requests.Select(value => value.k));
            Assert.Null(recording.Requests[0].sc);
            recording.Requests.Clear();
            recording.Responses.Clear();
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "coordinator-sql", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[]
                    {
                        new Pull { t = this.M.C1.Tag, o = target.Id, r = new[] { new Allors.Protocol.Json.Data.Result { n = "target" } } },
                        new Pull { t = this.M.C1.Tag, o = 9007199254740991, r = new[] { new Allors.Protocol.Json.Data.Result { n = "missing" } } },
                    },
                    d = System.Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };

            var cold = await coordinator.Read(request);
            var baseline = Assert.Single(cold.Records);
            Assert.Equal(target.Id, cold.Objects["target"]);
            Assert.Null(cold.Objects["missing"]);
            Assert.Equal("unavailable", cold.Inputs[1].Status);
            Assert.True(coordinator.Store.IsUnavailable(9007199254740991));
            Assert.Equal(population.Incarnation.ToString("D"), cold.Scope.Population);
            Assert.NotEmpty(cold.Security);
            Assert.NotEmpty(cold.Permissions);
            Assert.Equal("c1B", baseline.Roles[this.M.C1.Name.RelationType.Tag]);
            Assert.Equal(new[] { "discover", "read" }, recording.Requests.Select(value => value.k));
            Assert.Equal(cold.Scope.Population, recording.Requests[0].sc.p);
            Assert.Null(recording.Requests[0].h);
            Assert.Equal(cold.Scope.Population, recording.Requests[1].sc.p);

            var cached = await coordinator.Read(request);
            Assert.Same(baseline, Assert.Single(cached.Records));
            Assert.Equal(cold.Objects["target"], cached.Objects["target"]);
            Assert.Equal(cold.Security, cached.Security);
            Assert.Equal(cold.Permissions, cached.Permissions);
            Assert.Equal(new[] { "discover", "read", "discover", "read" }, recording.Requests.Select(value => value.k));
            var reused = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(recording.Responses.Last());
            Assert.NotNull(Assert.Single(reused.p).p.r);
            Assert.All(reused.s, value => Assert.NotNull(value.p.r));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(request.p), System.Text.Json.JsonSerializer.Serialize(recording.Requests.Last().p));

            target.Name = "coordinator-new-value";
            this.Transaction.Derive();
            this.Transaction.Commit();
            var changed = await coordinator.Read(request);
            Assert.Equal("coordinator-new-value", Assert.Single(changed.Records).Roles[this.M.C1.Name.RelationType.Tag]);
            Assert.Equal("c1B", baseline.Roles[this.M.C1.Name.RelationType.Tag]);
            Assert.Same(Assert.Single(changed.Records), coordinator.Store.GetCurrentRecord(target.Id));
            Assert.Equal(6, recording.Requests.Count);
            await coordinator.DisposeAsync();
            Assert.Null(coordinator.Store.GetRecord(baseline));
            Assert.Equal("c1B", baseline.Roles[this.M.C1.Name.RelationType.Tag]);
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ReadCoordinatorReselectsDataAndSecurityAfterDiscovery(string transport)
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var user = (User)this.SetUser("jane@example.com");
            const string memberName = "coordinator-member";
            var data = new C1Builder(this.Transaction).WithName(memberName).WithC1AllorsString("before").Build();
            var leaving = new C1Builder(this.Transaction).WithName(memberName).Build();
            var secured = new C1Builder(this.Transaction).WithName(memberName).WithC1AllorsString("previously readable").Build();
            var permissions = new Allors.Database.Domain.Permissions(this.Transaction);
            var stringPermission = permissions.Get(this.M.C1, this.M.C1.C1AllorsString, Allors.Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Discovery read role")
                .WithPermission(permissions.Get(this.M.C1, this.M.C1.Name, Allors.Operations.Read))
                .WithPermission(stringPermission).Build();
            var grant = new GrantBuilder(this.Transaction).WithSubject(user).WithRole(role).Build();
            secured.AddSecurityToken(new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build());
            this.Transaction.Derive();
            this.Transaction.Commit();
            var population = database.PopulationLifecycle.Renew();
            var executor = new ReadExecutor(database, new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "test-read-v1" }));
            var local = new Allors.Workspace.Adapters.Local.ReadTransport(executor, "Default", new LocalReadIdentity(population.Incarnation, user.Id));
            using var http = new System.Net.Http.HttpClient(new CoordinatorControllerHandler(executor, user.Id)) { BaseAddress = new System.Uri("http://localhost/allors/") };
            Allors.Workspace.Adapters.Remote.IReadTransport inner = transport switch
            {
                "Local" => local,
                "SystemText" => new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(http),
                _ => new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(http),
            };
            var recording = new CoordinatorRecordingTransport(inner);
            await using var coordinator = await ReadTestConnection.CreateAsync(recording,
                new Allors.Workspace.Meta.Lazy.MetaBuilder().Build(), http, "test-read-v1");
            Assert.Equal(new[] { "discover", "read" }, recording.Requests.Select(value => value.k));
            recording.Requests.Clear();
            recording.Responses.Clear();
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "changing-discovery", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[] { new Pull
                    {
                        e = new Allors.Protocol.Json.Data.Extent
                        {
                            k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = this.M.C1.Tag,
                            p = new Allors.Protocol.Json.Data.Predicate { k = Allors.Protocol.Json.Data.PredicateKind.Equals, r = this.M.C1.Name.RelationType.Tag, v = memberName },
                        },
                        r = new[] { new Allors.Protocol.Json.Data.Result { n = "members" } },
                    } },
                    d = System.Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };
            var baseline = await coordinator.Read(request);
            var previousData = baseline.Records.Single(value => value.Id == data.Id);
            var previousSecurity = baseline.Records.Single(value => value.Id == secured.Id);
            Assert.Equal("previously readable", previousSecurity.Roles[this.M.C1.C1AllorsString.RelationType.Tag]);
            C1 entering = null;
            var discoveries = 0;
            recording.AfterDiscovery = () =>
            {
                ++discoveries;
                data.C1AllorsString = "after";
                leaving.Name = "no longer a member";
                entering = new C1Builder(this.Transaction).WithName(memberName).WithC1AllorsString("entered").Build();
                role.RemovePermission(stringPermission);
                this.Transaction.Derive();
                this.Transaction.Commit();
            };

            var final = await coordinator.Read(request);
            Assert.Equal(1, discoveries);
            Assert.Equal(new[] { "discover", "read", "discover", "read" }, recording.Requests.Select(value => value.k));
            var discovered = Assert.IsType<Allors.Protocol.Json.Api.Read.DiscoverySuccess>(recording.Responses[2]);
            Assert.Equal(new[] { data.Id, leaving.Id, secured.Id }.OrderBy(value => value), discovered.p.Select(value => value.i).OrderBy(value => value));
            Assert.Equal(new[] { data.Id, secured.Id, entering.Id }.OrderBy(value => value), final.Collections["members"].OrderBy(value => value));
            Assert.Equal("after", final.Records.Single(value => value.Id == data.Id).Roles[this.M.C1.C1AllorsString.RelationType.Tag]);
            var currentSecurity = final.Records.Single(value => value.Id == secured.Id);
            Assert.Equal(previousSecurity.DataVersion, currentSecurity.DataVersion);
            Assert.False(currentSecurity.Roles.ContainsKey(this.M.C1.C1AllorsString.RelationType.Tag));
            Assert.False(coordinator.Store.IsPermitted(secured.Id, this.M.C1.Tag, Allors.Operations.Read, this.M.C1.C1AllorsString.RelationType.Tag));
            var complete = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(recording.Responses[3]);
            Assert.All(complete.p, value => Assert.Null(value.p.r));
            Assert.NotEmpty(recording.Requests[3].h.r);
            Assert.All(recording.Requests, value => Assert.Equal(System.Text.Json.JsonSerializer.Serialize(request.p), System.Text.Json.JsonSerializer.Serialize(value.p)));
            Assert.Equal("before", previousData.Roles[this.M.C1.C1AllorsString.RelationType.Tag]);
            Assert.Equal("previously readable", previousSecurity.Roles[this.M.C1.C1AllorsString.RelationType.Tag]);
            await coordinator.DisposeAsync();
            Assert.Null(coordinator.Store.GetRecord(previousData));
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionReadsAndMergesThroughEverySqlTransport(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            var session = connection.Session;
            var pull = connection.TargetPull;
            var first = await session.PullAsync(pull, new Allors.Workspace.Data.Pull
            {
                ObjectType = connection.Meta.C1,
                ObjectId = 9007199254740991,
                Results = new[] { new Allors.Workspace.Data.Result { Name = "missing" } },
            });
            Assert.False(first.HasErrors);
            var target = first.GetObject<Allors.Workspace.Domain.C1>("target");
            Assert.Equal("c1B", target.Name);
            Assert.Null(first.GetObject<Allors.Workspace.Domain.C1>("missing"));
            Assert.True(target.CanReadName);
            Assert.True(target.CanWriteName);
            connection.AssertPaths("read", "read");

            target.C1AllorsString = "local edit";
            connection.Target.C1AllorsInteger = 123;
            this.Transaction.Derive();
            this.Transaction.Commit();
            var disjoint = await session.PullAsync(pull);
            Assert.False(disjoint.HasErrors);
            Assert.Same(target, disjoint.GetObject<Allors.Workspace.Domain.C1>("target"));
            Assert.Equal("local edit", target.C1AllorsString);
            Assert.Equal(123, target.C1AllorsInteger);

            connection.Target.C1AllorsString = "competing edit";
            connection.Target.C1AllorsInteger = 456;
            this.Transaction.Derive();
            this.Transaction.Commit();
            var conflict = await session.PullAsync(pull);
            Assert.True(conflict.HasErrors);
            Assert.Same(target, Assert.Single(conflict.MergeErrors));
            Assert.Equal("local edit", target.C1AllorsString);
            Assert.Equal(123, target.C1AllorsInteger);
            connection.AssertPaths("read", "read", "read", "read", "read", "read");
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionPreservesInlineArgumentsAndEmptyResults(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            var result = await connection.Session.PullAsync(new Allors.Workspace.Data.Pull
            {
                Extent = new Allors.Workspace.Data.Filter(connection.Meta.C1)
                {
                    Predicate = new Allors.Workspace.Data.Equals(connection.Meta.C1.Name) { Parameter = "name" },
                },
                Arguments = new Dictionary<string, object> { ["name"] = "c1B" },
                Results = new[] { new Allors.Workspace.Data.Result { Name = "matches" } },
            });
            Assert.False(result.HasErrors);
            Assert.Equal("c1B", Assert.Single(result.GetCollection<Allors.Workspace.Domain.C1>("matches")).Name);

            var empty = await connection.Session.PullAsync(new Allors.Workspace.Data.Pull
            {
                ObjectType = connection.Meta.C1,
                ObjectId = connection.Target.Id,
                Results = System.Array.Empty<Allors.Workspace.Data.Result>(),
            });
            Assert.False(empty.HasErrors);
            Assert.Empty(empty.Collections);
            Assert.Empty(empty.Objects);
            connection.AssertPaths("read", "read", "read", "read");

            connection.AssertPaths("read", "read", "read", "read");
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionPushPreservesBaselineAndUsesOneMutationAttempt(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => connection.Session.PushAsync());
            connection.AssertPaths();
            var result = await connection.Session.PullAsync(connection.TargetPull);
            var target = result.GetObject<Allors.Workspace.Domain.C1>("target");
            var original = target.C1AllorsString;
            target.C1AllorsString = "pushed";
            Assert.False((await connection.Session.PushAsync(new PushOptions { Refresh = false })).HasErrors);
            Assert.Equal("pushed", target.C1AllorsString);
            target.Strategy.Reset();
            Assert.Equal(original, target.C1AllorsString);
            var refreshed = await connection.Session.PullAsync(connection.TargetPull);
            Assert.False(refreshed.HasErrors);
            Assert.Equal("pushed", target.C1AllorsString);
            connection.AssertPaths("read", "read", "push-read", "read", "read");

            if (transport != "Local")
            {
                connection.Handler.FailMutation = true;
                target.C1AllorsString = "possibly committed";
                var unknown = await connection.Session.PushAsync(new PushOptions { Refresh = false });
                Assert.Equal(MutationOutcome.Unknown, unknown.Outcome);
                Assert.True(unknown.ReconciliationRequired);
                Assert.True(connection.Session.ReconciliationRequired);
                connection.AssertPaths("read", "read", "push-read", "read", "read", "push-read");
                Assert.True(connection.Session.HasChanges);
            }
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionRemapsNewIdsAfterAnEmptyRead(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            var created = connection.Session.Create<Allors.Workspace.Domain.C1>();
            var newId = created.Id;
            Assert.True(newId < 0);
            created.C1AllorsString = "new scoped object";
            Assert.False((await connection.Session.PullAsync()).HasErrors);
            Assert.False((await connection.Session.PushAsync(new PushOptions { Refresh = false })).HasErrors);
            Assert.True(created.Id > 0);
            Assert.Same(created, connection.Session.Instantiate<Allors.Workspace.Domain.C1>(created.Id));
            Assert.Equal("new scoped object", created.C1AllorsString);
            var pulled = await connection.Session.PullAsync(new Allors.Workspace.Data.Pull
            {
                Object = created,
                Results = new[] { new Allors.Workspace.Data.Result { Name = "created" } },
            });
            Assert.False(pulled.HasErrors);
            Assert.Same(created, pulled.GetObject<Allors.Workspace.Domain.C1>("created"));
            Assert.Equal("new scoped object", created.C1AllorsString);
            connection.AssertPaths("read", "read", "push-read", "read", "read");
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionUnavailablePreservesEditsAndCanRecover(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            var first = await connection.Session.PullAsync(connection.TargetPull);
            var target = first.GetObject<Allors.Workspace.Domain.C1>("target");
            target.C1AllorsString = "retained local edit";
            var omitted = await connection.Session.PullAsync(new Allors.Workspace.Data.Pull
            {
                Extent = new Allors.Workspace.Data.Filter(connection.Meta.C1)
                {
                    Predicate = new Allors.Workspace.Data.Equals(connection.Meta.C1.Name) { Value = "not present" },
                },
            });
            Assert.False(omitted.HasErrors);
            Assert.Empty(omitted.GetCollection<Allors.Workspace.Domain.C1>());
            Assert.True(target.CanWriteC1AllorsString);
            var unavailable = Assert.IsType<Allors.Workspace.Adapters.Remote.ScopedPullResult>(await connection.Session.PullAsync(new Allors.Workspace.Data.Pull
            {
                ObjectType = connection.Meta.C2,
                ObjectId = target.Id,
                Results = new[] { new Allors.Workspace.Data.Result { Name = "unavailable" } },
            }));
            Assert.False(unavailable.HasErrors);
            Assert.Equal("unavailable", Assert.Single(unavailable.Inputs).Status);
            Assert.Null(unavailable.GetObject<Allors.Workspace.Domain.C1>("unavailable"));
            Assert.Null(target.C1AllorsString);
            Assert.Equal("retained local edit", ((Allors.Workspace.IUnitDiff)Assert.Single(target.Strategy.Diff())).ChangedRole);
            Assert.False(target.CanReadC1AllorsString);
            Assert.False(target.CanWriteC1AllorsString);
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => connection.Session.PushAsync());
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => connection.Session.InvokeAsync(target.ClassMethod));
            connection.AssertPaths("read", "read", "read", "read", "read", "read");
            var recovered = await connection.Session.PullAsync(connection.TargetPull);
            Assert.False(recovered.HasErrors);
            Assert.Same(target, recovered.GetObject<Allors.Workspace.Domain.C1>("target"));
            Assert.Equal("retained local edit", target.C1AllorsString);
            Assert.True(target.CanReadC1AllorsString);
            Assert.True(target.CanWriteC1AllorsString);
            connection.AssertPaths("read", "read", "read", "read", "read", "read", "read", "read");
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionLiveAuthorityChangesWhileAnEditingBaselineConflicts(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            connection.Target.C1AllorsString = "original";
            this.Transaction.Derive();
            this.Transaction.Commit();
            var result = await connection.Session.PullAsync(connection.TargetPull);
            var target = result.GetObject<Allors.Workspace.Domain.C1>("target");
            Assert.True(target.CanReadC1AllorsString);
            target.C1AllorsString = "local edit";
            var version = connection.Target.Strategy.ObjectVersion;
            var permissionId = connection.Target.Strategy.Class.ReadPermissionIdByRelationTypeId[this.M.C1.C1AllorsString.RelationType.Id];
            var permission = (Permission)this.Transaction.Instantiate(permissionId);
            var grants = this.Transaction.Services.Get<Allors.Database.Services.IWorkspaceAclsService>().Create("Default")[connection.Target].Grants
                .Select(grant => (Grant)this.Transaction.Instantiate(grant.Id)).Where(grant => grant.EffectivePermissions.Contains(permission)).ToArray();
            Assert.NotEmpty(grants);
            foreach (var grant in grants) { grant.Role.RemovePermission(permission); }
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.Equal(version, connection.Target.Strategy.ObjectVersion);
            var conflict = await connection.Session.PullAsync(connection.TargetPull);
            Assert.True(conflict.HasErrors);
            Assert.Same(target, Assert.Single(conflict.MergeErrors));
            Assert.Null(target.C1AllorsString);
            var diff = (Allors.Workspace.IUnitDiff)Assert.Single(target.Strategy.Diff());
            Assert.Equal("original", diff.OriginalRole);
            Assert.Equal("local edit", diff.ChangedRole);
            Assert.False(target.CanReadC1AllorsString);
            foreach (var grant in grants) { grant.Role.AddPermission(permission); }
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.False((await connection.Session.PullAsync(connection.TargetPull)).HasErrors);
            Assert.True(target.CanReadC1AllorsString);
            Assert.Equal("local edit", target.C1AllorsString);
            connection.AssertPaths("read", "read", "read", "read", "read", "read");
        }

        [Theory]
        [InlineData("Local")]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public async System.Threading.Tasks.Task ScopedSessionInvokePreservesRefreshVersionErrorsAndOneAttempt(string transport)
        {
            await using var connection = await this.CreateSessionConnectionAsync(transport);
            var result = await connection.Session.PullAsync(connection.TargetPull);
            var target = result.GetObject<Allors.Workspace.Domain.C1>("target");
            var options = new Allors.Workspace.InvokeOptions { Refresh = false };
            Assert.False((await connection.Session.InvokeAsync(target.ClassMethod, options)).HasErrors);
            if (transport != "Local")
            {
                Assert.False(connection.Handler.LastInvocation.refresh);
            }
            connection.Target.Name = "changed after the Session baseline";
            this.Transaction.Derive();
            this.Transaction.Commit();
            var outdated = await connection.Session.InvokeAsync(target.ClassMethod);
            Assert.True(outdated.HasErrors);
            Assert.Same(target, Assert.Single(outdated.VersionErrors));
            Assert.Equal(Allors.Workspace.MutationOutcome.Rejected, outdated.Outcome);
            Assert.Contains(target.Id, outdated.Diagnostics.VersionErrorIds);
            connection.AssertPaths("read", "read", "invoke-read", "invoke-read");
            if (transport != "Local")
            {
                connection.Handler.FailMutation = true;
                var lost = await connection.Session.InvokeAsync(target.ClassMethod, options);
                Assert.Equal(Allors.Workspace.MutationOutcome.Unknown, lost.Outcome);
                Assert.True(lost.ReconciliationRequired);
                connection.AssertPaths("read", "read", "invoke-read", "invoke-read", "invoke-read");
            }
        }

        [Theory]
        [InlineData("retired-population")]
        [InlineData("uncommitted-user")]
        public async System.Threading.Tasks.Task ScopedLocalConstructionRejectsAnInvalidIdentity(string invalid)
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var user = this.SetUser("jane@example.com");
            this.Transaction.Commit();
            var population = database.PopulationLifecycle.Renew();
            var identity = new LocalReadIdentity(invalid == "retired-population" ? System.Guid.NewGuid() : population.Incarnation,
                invalid == "uncommitted-user" ? 9007199254740991 : user.Id);
            var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var configuration = CreateScopedConfiguration(metadata);
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(
                configuration, () => new Allors.Workspace.WorkspaceServices(), database, identity,
                new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1"));
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedPushReadLocalRefreshesMappedRelationsAndPreservesCheckpoint()
        {
            await using var connection = await this.CreateSessionConnectionAsync("Local");
            var session = (Allors.Workspace.Adapters.Remote.Session)connection.Session;
            var target = (await session.PullAsync(connection.TargetPull)).GetObject<Allors.Workspace.Domain.C1>("target");
            session.Checkpoint();
            var created = session.Create<Allors.Workspace.Domain.C2>();
            created.Name = "created by scoped Local save";
            var temporaryId = created.Id;
            target.C1C2Many2One = created;
            target.C1AllorsString = "saved Local baseline";

            var saved = await session.PushAsync(new PushOptions
            {
                Refresh = true, Discover = new[] { connection.TargetPull },
            });

            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Discovery.Status);
            Assert.Empty(saved.Application.Errors);
            Assert.False(saved.ReconciliationRequired);
            Assert.True(created.Id > 0);
            Assert.Same(created, target.C1C2Many2One);
            Assert.Same(created, session.Instantiate<Allors.Workspace.Domain.C2>(created.Id));
            Assert.Null(session.Instantiate<Allors.Workspace.Domain.C2>(temporaryId));
            Assert.False(session.HasChanges);
            Assert.True(target.CanWriteC1AllorsString);
            Assert.Contains(created.Strategy, session.Checkpoint().Created);
            target.C1AllorsString = "after save edit";
            target.Strategy.Reset();
            Assert.Equal("saved Local baseline", target.C1AllorsString);
            var final = await saved.PendingQuery.CompleteAsync();
            Assert.False(final.HasErrors);
            Assert.Same(target, final.GetObject<Allors.Workspace.Domain.C1>("target"));
            Assert.Same(created, target.C1C2Many2One);
            this.Transaction.Rollback();
            Assert.Equal(created.Id, connection.Target.C1C2Many2One.Id);
            Assert.Equal("saved Local baseline", connection.Target.C1AllorsString);
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedPushReadLocalFinalQueryRefreshesConcurrentSecurityAndMembership()
        {
            await using var connection = await this.CreateSessionConnectionAsync("Local");
            var session = (Allors.Workspace.Adapters.Remote.Session)connection.Session;
            var query = new Allors.Workspace.Data.Pull
            {
                Extent = new Allors.Workspace.Data.Filter(connection.Meta.C1)
                {
                    Predicate = new Allors.Workspace.Data.Equals(connection.Meta.C1.Name) { Value = "c1B" },
                },
                Results = new[] { new Allors.Workspace.Data.Result { Name = "members" } },
            };
            var target = Assert.Single((await session.PullAsync(query)).GetCollection<Allors.Workspace.Domain.C1>("members"));
            target.C1AllorsString = "acknowledged";
            var saved = await session.PushAsync(new PushOptions
            {
                Refresh = true, Discover = new[] { query, connection.TargetPull },
            });
            Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            target.C1AllorsString = "intervening local edit";

            this.Transaction.Rollback();
            connection.Target.C1AllorsString = "concurrent server edit";
            connection.Target.Name = "left the query";
            var entered = new C1Builder(this.Transaction).WithName("c1B").Build();
            var permissionId = connection.Target.Strategy.Class.WritePermissionIdByRelationTypeId[this.M.C1.C1AllorsString.RelationType.Id];
            var permission = (Permission)this.Transaction.Instantiate(permissionId);
            var grants = this.Transaction.Services.Get<Allors.Database.Services.IWorkspaceAclsService>().Create("Default")[connection.Target].Grants
                .Select(grant => (Grant)this.Transaction.Instantiate(grant.Id)).Where(grant => grant.EffectivePermissions.Contains(permission)).ToArray();
            Assert.NotEmpty(grants);
            foreach (var grant in grants) { grant.Role.RemovePermission(permission); }
            this.Transaction.Derive();
            this.Transaction.Commit();

            var final = await saved.PendingQuery.CompleteAsync();
            Assert.Same(target, Assert.Single(final.MergeErrors));
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(entered.Id, Assert.Single(final.GetCollection<Allors.Workspace.Domain.C1>("members")).Id);
            Assert.Same(target, final.GetObject<Allors.Workspace.Domain.C1>("target"));
            Assert.Equal("intervening local edit", target.C1AllorsString);
            Assert.False(target.CanWriteC1AllorsString);
            var diff = Assert.IsAssignableFrom<Allors.Workspace.IUnitDiff>(Assert.Single(target.Strategy.Diff()));
            Assert.Equal("acknowledged", diff.OriginalRole);
            Assert.Equal("intervening local edit", diff.ChangedRole);
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedPushReadLocalFinalQueryRefreshesSecurityAtTheSameObjectVersion()
        {
            await using var connection = await this.CreateSessionConnectionAsync("Local");
            var session = (Allors.Workspace.Adapters.Remote.Session)connection.Session;
            var target = (await session.PullAsync(connection.TargetPull)).GetObject<Allors.Workspace.Domain.C1>("target");
            target.C1AllorsString = "unchanged after acknowledgement";
            var saved = await session.PushAsync(new PushOptions
            {
                Refresh = true, Discover = new[] { connection.TargetPull },
            });
            Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.True(target.CanWriteC1AllorsString);
            var version = target.Strategy.Version;
            this.Transaction.Rollback();
            var permissionId = connection.Target.Strategy.Class.WritePermissionIdByRelationTypeId[this.M.C1.C1AllorsString.RelationType.Id];
            var permission = (Permission)this.Transaction.Instantiate(permissionId);
            var grants = this.Transaction.Services.Get<Allors.Database.Services.IWorkspaceAclsService>().Create("Default")[connection.Target].Grants
                .Select(grant => (Grant)this.Transaction.Instantiate(grant.Id)).Where(grant => grant.EffectivePermissions.Contains(permission)).ToArray();
            Assert.NotEmpty(grants);
            foreach (var grant in grants) { grant.Role.RemovePermission(permission); }
            this.Transaction.Derive();
            this.Transaction.Commit();
            Assert.Equal(version, connection.Target.Strategy.ObjectVersion);

            var final = await saved.PendingQuery.CompleteAsync();
            Assert.False(final.HasErrors);
            Assert.Same(target, final.GetObject<Allors.Workspace.Domain.C1>("target"));
            Assert.Equal(version, target.Strategy.Version);
            Assert.Equal("unchanged after acknowledgement", target.C1AllorsString);
            Assert.True(target.CanReadC1AllorsString);
            Assert.False(target.CanWriteC1AllorsString);
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedPushReadLocalPendingQueryReportsConcurrentDeletion()
        {
            await using var connection = await this.CreateSessionConnectionAsync("Local");
            var session = (Allors.Workspace.Adapters.Remote.Session)connection.Session;
            var target = (await session.PullAsync(connection.TargetPull)).GetObject<Allors.Workspace.Domain.C1>("target");
            target.C1AllorsString = "saved before deletion";
            var saved = await session.PushAsync(new PushOptions
            {
                Refresh = true, Discover = new[] { connection.TargetPull },
            });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            this.Transaction.Rollback();
            connection.Target.Strategy.Delete();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var final = Assert.IsType<Allors.Workspace.Adapters.Remote.ScopedPullResult>(await saved.PendingQuery.CompleteAsync());
            Assert.False(final.HasErrors);
            Assert.Equal("unavailable", Assert.Single(final.Inputs).Status);
            Assert.Null(final.GetObject<Allors.Workspace.Domain.C1>("target"));
            Assert.False(target.CanReadC1AllorsString);
            Assert.False(target.CanWriteC1AllorsString);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ScopedPushReadLocalKeepsCommitWhenCompletionCannotRefresh(bool retire)
        {
            await using var connection = await this.CreateSessionConnectionAsync("Local");
            var session = (Allors.Workspace.Adapters.Remote.Session)connection.Session;
            var target = (await session.PullAsync(connection.TargetPull)).GetObject<Allors.Workspace.Domain.C1>("target");
            target.C1AllorsString = "committed once";
            var database = (SqlDatabase)this.Transaction.Database;
            var commits = 0;
            database.Sink = new Sink { PreOnAfter = value =>
            {
                if (value is PushEvent)
                {
                    ++commits;
                    if (!retire) { throw new System.InvalidOperationException("Injected failure after the physical commit."); }
                }
            } };
            var saved = await session.PushAsync(new PushOptions
            {
                Refresh = true, Discover = new[] { connection.TargetPull },
            });
            database.Sink = null;
            Assert.Equal(1, commits);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.False(saved.ReconciliationRequired);
            this.Transaction.Rollback();
            Assert.Equal("committed once", connection.Target.C1AllorsString);
            if (retire)
            {
                Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
                Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
                await connection.Connection.DisposeAsync();
                await Assert.ThrowsAsync<System.InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
            }
            else
            {
                Assert.Equal(MutationReadStatus.Failed, saved.Refresh.Status);
                Assert.Equal(MutationReadStatus.Failed, saved.Discovery.Status);
                Assert.Null(saved.PendingQuery);
                Assert.False(target.CanWriteC1AllorsString);
                Assert.False((await session.PullAsync(connection.TargetPull)).HasErrors);
                Assert.True(target.CanWriteC1AllorsString);
                Assert.Equal("committed once", target.C1AllorsString);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedLocalOwnerBootstrapsBeforeImmediatePushAndBorrowsPopulation()
        {
            var inputs = this.CreateLocalOwnerInputs();
            var services = new BootstrapServiceProbe();
            var owner = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta),
                services.CreateWorkspaceServices, inputs.Database, inputs.Identity, new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
            try
            {
                Assert.Equal(0, services.Workspaces);
                Assert.Equal(0, services.WorkspaceInitializations);
                Assert.Equal(0, services.Sessions);
                var session = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var created = session.Create<Allors.Workspace.Domain.C1>();
                created.Name = "owned local creation";
                Assert.False((await session.PushAsync()).HasErrors);
                Assert.True(created.Id > 0);

                await owner.DisposeAsync();
                Assert.True(session.IsClosed);
                Assert.Throws<System.ObjectDisposedException>(() => (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession());
                Assert.Throws<System.ObjectDisposedException>(() => owner.CreateWorkspace());
                Assert.Equal(inputs.Identity.Population, inputs.Database.PopulationLifecycle.RequireReady().Incarnation);
                this.Transaction.Rollback();
                Assert.Equal("owned local creation", ((C1)this.Transaction.Instantiate(created.Id)).Name);
            }
            finally { await owner.DisposeAsync(); }
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedLocalOwnerClosesSiblingWorkspacesAndKeepsIndependentOwnerUsable()
        {
            var inputs = this.CreateLocalOwnerInputs();
            var configuration = CreateScopedConfiguration(inputs.Meta);
            var owner = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(configuration,
                () => new Allors.Workspace.WorkspaceServices(), inputs.Database, inputs.Identity, new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
            try
            {
                var independent = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(configuration,
                    () => new Allors.Workspace.WorkspaceServices(), inputs.Database, inputs.Identity, new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
                try
                {
                    var first = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                    var sibling = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                    var other = (Allors.Workspace.Adapters.Remote.Session)independent.CreateWorkspace().CreateSession();
                    var targetId = new C1s(this.Transaction).Extent().First(value => value.Name == "c1B").Id;
                    var pull = new Allors.Workspace.Data.Pull
                    {
                        ObjectType = inputs.Meta.C1, ObjectId = targetId,
                        Results = new[] { new Allors.Workspace.Data.Result { Name = "target" } },
                    };
                    var firstTarget = (await first.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("target");
                    var siblingTarget = (await sibling.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("target");
                    var otherTarget = (await other.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("target");
                    Assert.NotSame(firstTarget, siblingTarget);
                    Assert.NotSame(firstTarget, otherTarget);
                    firstTarget.C1AllorsString = "retained owner edit";
                    Assert.Same(owner, ((Allors.Workspace.Adapters.Remote.Workspace)first.Workspace).DatabaseConnection);
                    Assert.Same(independent, ((Allors.Workspace.Adapters.Remote.Workspace)other.Workspace).DatabaseConnection);

                    await owner.DisposeAsync();
                    await owner.DisposeAsync();
                    Assert.True(first.IsClosed);
                    Assert.True(sibling.IsClosed);
                    Assert.False(other.IsClosed);
                    Assert.Equal("retained owner edit", ((Allors.Workspace.IUnitDiff)Assert.Single(firstTarget.Strategy.Diff())).ChangedRole);
                    Assert.Throws<System.InvalidOperationException>(() => firstTarget.CanWriteC1AllorsString);
                    Assert.False((await other.PullAsync(pull)).HasErrors);
                    Assert.NotEqual("retained owner edit", otherTarget.C1AllorsString);
                    otherTarget.C1AllorsString = "independent owner saved";
                    Assert.False((await other.PushAsync()).HasErrors);
                    await independent.DisposeAsync();
                    Assert.True(other.IsClosed);
                    this.Transaction.Rollback();
                    Assert.Equal("independent owner saved", ((C1)this.Transaction.Instantiate(targetId)).C1AllorsString);
                }
                finally { await independent.DisposeAsync(); }
            }
            finally { await owner.DisposeAsync(); }
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedLocalOwnerControlledBootstrapOccursOncePerConnection()
        {
            var inputs = this.CreateLocalOwnerInputs();
            var connection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
            try
            {
                var owner = connection;
                Assert.Equal(0, connection.ApplicationServicesCreated);
                Assert.Equal(new[] { "discover", "read" }, connection.Reads.Requests.Select(value => value.k));
                Assert.All(connection.Reads.Requests, value => Assert.Empty(value.p.l));
                var first = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var sibling = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                Assert.Equal(2, connection.Reads.Requests.Count);
                Assert.False((await first.PushAsync()).HasErrors);
                Assert.False((await sibling.PushAsync()).HasErrors);
                Assert.Equal(2, connection.Pushes);
                Assert.Equal(2, connection.Reads.Requests.Count);

                var independentConnection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
                try
                {
                    Assert.Equal(new[] { "discover", "read" }, independentConnection.Reads.Requests.Select(value => value.k));
                    Assert.Equal(2, connection.Reads.Requests.Count);
                }
                finally { await independentConnection.DisposeAsync(); }
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false, "discover")]
        [InlineData(true, "discover")]
        [InlineData(false, "read")]
        [InlineData(true, "read")]
        public async System.Threading.Tasks.Task ScopedLocalOwnerControlledFailedSetupRetiresWithoutMutationOrRetry(bool throwResponse, string failureStage)
        {
            var inputs = this.CreateLocalOwnerInputs();
            LocalOwnerControlledConnection connection = null;
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => LocalOwnerControlledConnection.CreateAsync(
                CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity, value =>
                {
                    connection = value;
                    value.Reads.FailStage = failureStage;
                    value.Reads.Throw = throwResponse;
                }));
            Assert.NotNull(connection);
            Assert.Equal(0, connection.ApplicationServicesCreated);
            Assert.Equal(failureStage == "discover" ? 1 : 2, connection.Reads.Requests.Count);
            Assert.Equal(0, connection.Pushes);
            Assert.Throws<System.ObjectDisposedException>(() => connection.CreateWorkspace());
            Assert.Equal(failureStage == "discover" ? 1 : 2, connection.Reads.Requests.Count);
            Assert.Equal(inputs.Identity.Population, inputs.Database.PopulationLifecycle.RequireReady().Incarnation);

            var replacement = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta),
                () => new Allors.Workspace.WorkspaceServices(), inputs.Database, inputs.Identity, new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
            try
            {
                Assert.False((await replacement.CreateWorkspace().CreateSession().PushAsync()).HasErrors);
            }
            finally { await replacement.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ScopedLocalOwnerControlledDisposalDrainsDispatchedPushAndRejectsQueuedPush(bool loseResponse)
        {
            var inputs = this.CreateLocalOwnerInputs();
            var connection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
            try
            {
                var owner = connection;
                var editing = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var queued = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var created = editing.Create<Allors.Workspace.Domain.C1>();
                created.Name = "committed during local owner retirement";
                var queuedObject = queued.Create<Allors.Workspace.Domain.C1>();
                queuedObject.Name = "queued local owner creation";
                var entered = new System.Threading.Tasks.TaskCompletionSource<bool>(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);
                var release = new System.Threading.Tasks.TaskCompletionSource<bool>(System.Threading.Tasks.TaskCreationOptions.RunContinuationsAsynchronously);
                connection.BeforePush = async () => { entered.SetResult(true); await release.Task; };
                connection.LosePushResponse = loseResponse;
                var pushing = editing.PushAsync();
                await entered.Task;
                var waiting = queued.PushAsync();
                var disposal = owner.DisposeAsync();
                try
                {
                    Assert.False(disposal.IsCompleted);
                    Assert.False(editing.IsClosed);
                    Assert.False(queued.IsClosed);
                    Assert.Throws<System.ObjectDisposedException>(() => (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession());
                    Assert.Equal(1, connection.Pushes);
                }
                finally
                {
                    release.TrySetResult(true);
                }

                var saved = await pushing;
                Assert.Equal(loseResponse ? MutationOutcome.Unknown : MutationOutcome.Committed, saved.Outcome);
                Assert.True(saved.ReconciliationRequired);
                Assert.True(editing.ReconciliationRequired);
                Assert.Equal(loseResponse ? MutationAcknowledgementStatus.NotApplicable : MutationAcknowledgementStatus.Failed, saved.Application.Acknowledgement);
                await Assert.ThrowsAsync<System.InvalidOperationException>(() => waiting);
                await disposal;
                Assert.True(editing.IsClosed);
                Assert.True(queued.IsClosed);
                Assert.True(created.Id < 0);
                Assert.True(queuedObject.Id < 0);
                Assert.Equal(1, connection.Pushes);
                this.Transaction.Rollback();
                Assert.Single(new C1s(this.Transaction).Extent(), value => value.Name == "committed during local owner retirement");
                Assert.DoesNotContain(new C1s(this.Transaction).Extent(), value => value.Name == "queued local owner creation");
                Assert.Equal(inputs.Identity.Population, inputs.Database.PopulationLifecycle.RequireReady().Incarnation);
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ProtectedPushRefreshAndSkipUseOneRequest(bool skip)
        {
            var inputs = this.CreateLocalOwnerInputs();
            var connection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
            try
            {
                var owner = connection;
                var session = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var created = session.Create<Allors.Workspace.Domain.C1>();
                created.Name = "protected Local default or skip";
                created.C1AllorsString = "accepted saved value";
                var temporaryId = created.Id;

                var saved = skip ? await session.PushAsync(new PushOptions { Refresh = false }) : await session.PushAsync();

                Assert.Equal(MutationOutcome.Committed, saved.Outcome);
                Assert.False(saved.HasErrors);
                Assert.Equal(MutationAcknowledgementStatus.Applied, saved.Application.Acknowledgement);
                Assert.Equal(skip ? MutationReadStatus.NotRequested : MutationReadStatus.Succeeded, saved.Refresh.Status);
                Assert.Equal(skip ? MutationRefreshApplicationStatus.NotApplicable : MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
                Assert.Equal(1, connection.Pushes);
                Assert.Equal(2, connection.Reads.Requests.Count);
                var mapping = Assert.Single(saved.CreationMappings);
                Assert.Equal(temporaryId, mapping.WorkspaceId);
                Assert.Equal(created.Id, mapping.DatabaseId);
                Assert.True(created.Id > 0);
                Assert.Same(created, session.Instantiate<Allors.Workspace.Domain.C1>(created.Id));
                Assert.Null(session.Instantiate<Allors.Workspace.Domain.C1>(temporaryId));
                Assert.Equal(!skip, created.CanWriteC1AllorsString);
                if (!skip) created.C1AllorsString = "later local edit";
                created.Strategy.Reset();
                Assert.Equal(skip ? null : "accepted saved value", created.C1AllorsString);
                if (skip)
                {
                    Assert.False((await session.PullAsync(new Allors.Workspace.Data.Pull { ObjectType = inputs.Meta.C1, ObjectId = created.Id })).HasErrors);
                    Assert.Equal("accepted saved value", created.C1AllorsString);
                    Assert.True(created.CanWriteC1AllorsString);
                    Assert.Equal(4, connection.Reads.Requests.Count);
                    Assert.Equal(1, connection.Pushes);
                }
                this.Transaction.Rollback();
                Assert.Equal("accepted saved value", ((C1)this.Transaction.Instantiate(created.Id)).C1AllorsString);
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ProtectedInvokeRefreshAndSkipUseOneRequest(bool skip)
        {
            var databaseTarget = new OrganisationBuilder(this.Transaction).WithName("Local Invoke target").Build();
            var inputs = this.CreateLocalOwnerInputs();
            var connection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
            try
            {
                var owner = connection;
                var session = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var pull = new Allors.Workspace.Data.Pull { ObjectType = inputs.Meta.Organisation, ObjectId = databaseTarget.Id };
                var target = (await session.PullAsync(pull)).GetObject<Allors.Workspace.Domain.Organisation>();
                var version = target.Strategy.Version;
                var reads = connection.Reads.Requests.Count;
                Assert.False(target.JustDidIt);
                Assert.False(target.JustDidItDerived);

                var invoked = skip
                    ? await session.InvokeAsync(target.JustDoIt, new Allors.Workspace.InvokeOptions { Refresh = false })
                    : await session.InvokeAsync(target.JustDoIt);

                Assert.Equal(MutationOutcome.Committed, invoked.Outcome);
                Assert.False(invoked.HasErrors);
                Assert.False(invoked.ReconciliationRequired);
                Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
                Assert.Equal(skip ? MutationReadStatus.NotRequested : MutationReadStatus.Succeeded, invoked.Refresh.Status);
                Assert.Equal(skip ? MutationRefreshApplicationStatus.NotApplicable : MutationRefreshApplicationStatus.Applied, invoked.Application.Refresh);
                Assert.Equal(1, connection.Invokes);
                Assert.Equal(0, connection.Pushes);
                Assert.Equal(reads, connection.Reads.Requests.Count);
                Assert.Equal(!skip, target.JustDidIt);
                Assert.Equal(!skip, target.JustDidItDerived);
                Assert.False(session.HasChanges);
                Assert.Equal(!skip, Assert.Single(connection.InvokeRequests).refresh);
                if (skip)
                {
                    Assert.Equal(version, target.Strategy.Version);
                    Assert.False((await session.PullAsync(pull)).HasErrors);
                    Assert.True(target.JustDidIt);
                    Assert.True(target.JustDidItDerived);
                    Assert.Equal(reads + 2, connection.Reads.Requests.Count);
                    Assert.Equal(1, connection.Invokes);
                }
                Assert.True(target.Strategy.Version > version);
                using var verification = inputs.Database.CreateTransaction();
                var persisted = (Organisation)verification.Instantiate(target.Id);
                Assert.True(persisted.JustDidIt);
                Assert.True(persisted.JustDidItDerived);
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ProtectedInvokeUnsavedChangesAndCreationTrackingSurviveRealExecution(bool conflict)
        {
            var databaseTarget = new OrganisationBuilder(this.Transaction).WithName("Committed Local name").Build();
            var inputs = this.CreateLocalOwnerInputs();
            var connection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
            try
            {
                var owner = connection;
                var session = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var target = (await session.PullAsync(new Allors.Workspace.Data.Pull { ObjectType = inputs.Meta.Organisation, ObjectId = databaseTarget.Id }))
                    .GetObject<Allors.Workspace.Domain.Organisation>();
                var version = target.Strategy.Version;
                session.Checkpoint();
                target.Name = "unsaved Local name";
                if (conflict) target.JustDidIt = true;
                var created = session.Create<Allors.Workspace.Domain.C1>();
                created.Name = "unsaved Local Invoke creation";
                var temporaryId = created.Id;
                var reads = connection.Reads.Requests.Count;

                var invoked = await session.InvokeAsync(target.JustDoIt);

                Assert.Equal(MutationOutcome.Committed, invoked.Outcome);
                Assert.Equal(MutationReadStatus.Succeeded, invoked.Refresh.Status);
                Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
                Assert.Equal(conflict ? MutationRefreshApplicationStatus.Conflicted : MutationRefreshApplicationStatus.Applied, invoked.Application.Refresh);
                Assert.Equal(conflict, invoked.HasErrors);
                Assert.False(invoked.ReconciliationRequired);
                Assert.Equal(1, connection.Invokes);
                Assert.Equal(0, connection.Pushes);
                Assert.Equal(reads, connection.Reads.Requests.Count);
                Assert.Equal("unsaved Local name", target.Name);
                Assert.Equal(temporaryId, created.Id);
                Assert.True(temporaryId < 0);
                Assert.True(created.Strategy.IsNew);
                Assert.Same(created, session.Instantiate<Allors.Workspace.Domain.C1>(temporaryId));
                Assert.True(session.HasChanges);
                Assert.Contains(created.Strategy, session.Checkpoint().Created);
                Assert.Contains(target.Strategy.Diff(), value => value.RelationType == inputs.Meta.Organisation.Name.RelationType);
                Assert.True(target.CanWriteName);
                if (conflict)
                {
                    Assert.Same(target, Assert.Single(invoked.Refresh.Result.MergeErrors));
                    Assert.Equal(version, target.Strategy.Version);
                    Assert.False(target.JustDidItDerived);
                }
                else
                {
                    Assert.Empty(invoked.Refresh.Result.MergeErrors);
                    Assert.True(target.Strategy.Version > version);
                    Assert.True(target.JustDidItDerived);
                }
                using var verification = inputs.Database.CreateTransaction();
                var persisted = (Organisation)verification.Instantiate(target.Id);
                Assert.Equal("Committed Local name", persisted.Name);
                Assert.True(persisted.JustDidIt);
                Assert.True(persisted.JustDidItDerived);
                Assert.DoesNotContain(new C1s(verification).Extent(), value => value.Name == created.Name);
                target.Strategy.Reset();
                Assert.Equal("Committed Local name", target.Name);
                Assert.Equal(!conflict, target.JustDidIt);
                Assert.True(session.HasChanges);
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ProtectedInvokeRealAtomicBatchReturnsOneOutcome(bool reject)
        {
            var first = new OrganisationBuilder(this.Transaction).WithName("First atomic target").Build();
            var second = new OrganisationBuilder(this.Transaction).WithName("Second atomic target").Build();
            var inputs = this.CreateLocalOwnerInputs();
            var connection = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
            try
            {
                var owner = connection;
                var session = (Allors.Workspace.Adapters.Remote.Session)owner.CreateWorkspace().CreateSession();
                var target = (await session.PullAsync(new Allors.Workspace.Data.Pull { ObjectType = inputs.Meta.Organisation, ObjectId = first.Id }))
                    .GetObject<Allors.Workspace.Domain.Organisation>();
                var later = (await session.PullAsync(new Allors.Workspace.Data.Pull { ObjectType = inputs.Meta.Organisation, ObjectId = second.Id }))
                    .GetObject<Allors.Workspace.Domain.Organisation>();
                var reads = connection.Reads.Requests.Count;
                var methods = reject ? new[] { target.JustDoIt, later.Delete, later.JustDoIt } : new[] { target.JustDoIt, later.JustDoIt };

                var invoked = await session.InvokeAsync(methods);

                Assert.Equal(reject ? MutationOutcome.Rejected : MutationOutcome.Committed, invoked.Outcome);
                Assert.Equal(reject, invoked.HasErrors);
                Assert.Equal(reject ? MutationReadStatus.Unavailable : MutationReadStatus.Succeeded, invoked.Refresh.Status);
                Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
                Assert.False(invoked.ReconciliationRequired);
                Assert.Equal(1, connection.Invokes);
                Assert.Equal(0, connection.Pushes);
                Assert.Equal(reads, connection.Reads.Requests.Count);
                Assert.Equal(!reject, target.JustDidIt);
                Assert.Equal(!reject, target.JustDidItDerived);
                Assert.False(session.HasChanges);
                if (reject) Assert.Contains(second.Id, invoked.Diagnostics.MissingErrorIds);
                using var verification = inputs.Database.CreateTransaction();
                var persistedFirst = (Organisation)verification.Instantiate(first.Id);
                var persistedSecond = (Organisation)verification.Instantiate(second.Id);
                Assert.NotNull(persistedSecond);
                Assert.Equal(!reject, persistedFirst.JustDidIt);
                Assert.Equal(!reject, persistedFirst.JustDidItDerived);
                Assert.Equal(!reject, persistedSecond.JustDidIt);
            }
            finally { await connection.DisposeAsync(); }
        }

        [Fact]
        public async System.Threading.Tasks.Task NormalLocalFactoryBootstrapsWithoutApplicationServices()
        {
            var inputs = this.CreateLocalOwnerInputs();
            var services = new BootstrapServiceProbe();
            var connection = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(
                CreateScopedConfiguration(inputs.Meta), services.CreateWorkspaceServices, inputs.Database, inputs.Identity,
                new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
            try
            {
                Assert.Equal(0, services.Workspaces);
                Assert.Equal(0, services.WorkspaceInitializations);
                Assert.Equal(0, services.Sessions);
                Assert.Equal(0, services.SessionInitializations);
                var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
                var created = session.Create<Allors.Workspace.Domain.C1>();
                created.Name = "ready Local native factory";
                Assert.Equal(MutationOutcome.Committed, (await session.PushAsync()).Outcome);
                Assert.True(created.Id > 0);
                Assert.Equal(1, services.WorkspaceInitializations);
                Assert.Equal(1, services.SessionInitializations);
                await connection.DisposeAsync();
                Assert.True(session.IsClosed);
                Assert.Equal(1, services.SessionDisposals);
                Assert.Equal(1, services.WorkspaceDisposals);
                Assert.Equal(inputs.Identity.Population, inputs.Database.PopulationLifecycle.RequireReady().Incarnation);
                using var verifier = inputs.Database.CreateTransaction();
                Assert.Equal(created.Name, ((C1)verifier.Instantiate(created.Id)).Name);

                // The protected adapter harness exposes the actual Local protocol while preserving
                // the concrete factory's eager identity admission and normal initialization path.
                var recorded = await LocalOwnerControlledConnection.CreateAsync(CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity);
                try
                {
                    Assert.Equal(new[] { "discover", "read" }, recorded.Reads.Requests.Select(value => value.k));
                    Assert.All(recorded.Reads.Requests, request => Assert.Empty(request.p.l));
                    Assert.Equal(0, recorded.ApplicationServicesCreated);
                }
                finally { await recorded.DisposeAsync(); }
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData("discover")]
        [InlineData("read")]
        public async System.Threading.Tasks.Task NormalLocalFactoryRejectsChangingPopulationDuringBootstrap(string stage)
        {
            var inputs = this.CreateLocalOwnerInputs();
            LocalOwnerControlledConnection captured = null;
            await Assert.ThrowsAsync<System.InvalidOperationException>(() => LocalOwnerControlledConnection.CreateAsync(
                CreateScopedConfiguration(inputs.Meta), inputs.Database, inputs.Identity, value =>
                {
                    captured = value;
                    value.Reads.BeforeRead = request =>
                    {
                        if (request.k == stage) inputs.Database.PopulationLifecycle.Renew();
                    };
                }));
            Assert.NotNull(captured);
            Assert.Equal(stage == "discover" ? 1 : 2, captured.Reads.Requests.Count);
            Assert.Equal(0, captured.ApplicationServicesCreated);
            Assert.Equal(0, captured.Pushes);
            Assert.Equal(0, captured.Invokes);
            Assert.Throws<System.ObjectDisposedException>(() => captured.CreateWorkspace());
            var population = inputs.Database.PopulationLifecycle.RequireReady().Incarnation;
            Assert.NotEqual(inputs.Identity.Population, population);
            using (var verifier = inputs.Database.CreateTransaction()) Assert.NotNull(verifier.Instantiate(inputs.Identity.UserId));
            var replacement = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(
                CreateScopedConfiguration(inputs.Meta), () => new Allors.Workspace.WorkspaceServices(), inputs.Database,
                new LocalReadIdentity(population, inputs.Identity.UserId), new Allors.Workspace.Adapters.IdGenerator(),
                new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
            try
            {
                Assert.Equal(MutationOutcome.Committed, (await replacement.CreateWorkspace().CreateSession().PushAsync()).Outcome);
            }
            finally { await replacement.DisposeAsync(); }
        }

        [Fact]
        public async System.Threading.Tasks.Task NormalLocalConnectionClosesSessionsAcrossAllWorkspaces()
        {
            var inputs = this.CreateLocalOwnerInputs();
            var services = new BootstrapServiceProbe();
            var connection = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(
                CreateScopedConfiguration(inputs.Meta), services.CreateWorkspaceServices, inputs.Database, inputs.Identity,
                new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
            try
            {
                var independent = await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(
                    CreateScopedConfiguration(inputs.Meta), () => new Allors.Workspace.WorkspaceServices(), inputs.Database, inputs.Identity,
                    new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "test-read-v1");
                try
                {
                    var workspace = connection.CreateWorkspace();
                    var first = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
                    var sibling = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
                    var other = (Allors.Workspace.Adapters.Remote.Session)independent.CreateWorkspace().CreateSession();
                    var held = first.Create<Allors.Workspace.Domain.C1>();
                    held.Name = "unsaved across native closure";
                    var id = held.Id;
                    var diff = held.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)).ToArray();
                    var closing = connection.DisposeAsync();
                    Assert.Throws<System.ObjectDisposedException>(() => workspace.CreateSession());
                    Assert.Throws<System.ObjectDisposedException>(() => connection.CreateWorkspace());
                    await closing;
                    await connection.DisposeAsync();
                    Assert.True(first.IsClosed);
                    Assert.True(sibling.IsClosed);
                    Assert.False(other.IsClosed);
                    Assert.Equal(id, held.Id);
                    Assert.Equal("unsaved across native closure", held.Name);
                    Assert.Equal(diff, held.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)));
                    Assert.Equal(2, services.SessionDisposals);
                    Assert.Equal(2, services.WorkspaceDisposals);
                    Assert.Equal(MutationOutcome.Committed, (await other.PushAsync()).Outcome);
                    Assert.Equal(inputs.Identity.Population, inputs.Database.PopulationLifecycle.RequireReady().Incarnation);
                }
                finally { await independent.DisposeAsync(); }
            }
            finally { await connection.DisposeAsync(); }
        }

        private static IActionResult ExecutePermissionSetup(TestPermissionController controller, string action, long targetId) => action switch
        {
            "remove-c1-string-read-grant" => controller.RemoveC1StringReadGrant(targetId),
            "add-denied-property-read-revocation" => controller.AddDeniedPropertyReadRevocation(targetId),
            "remove-denied-property-read-revocation" => controller.RemoveDeniedPropertyReadRevocation(targetId),
            _ => throw new System.ArgumentException("Unknown permission fixture action.", nameof(action)),
        };

        private PermissionSetupSnapshot CapturePermissionSetupBaseline()
        {
            Assert.False(this.Transaction.Derive(throwExceptionOnError: false).HasErrors);
            this.Transaction.Commit();
            return new PermissionSetupSnapshot(this.Transaction);
        }

        private void AssertPermissionSetupUncommitted(PermissionSetupSnapshot baseline)
        {
            // Release request locks before checking committed state; rollback cannot undo an accidental commit.
            this.Transaction.Rollback();
            using var verification = this.Transaction.Database.CreateTransaction();
            baseline.AssertSecurityUnchanged(verification);
            Assert.Equal(baseline.UnrelatedName, ((C1)verification.Instantiate(baseline.Unrelated.Id)).Name);
            Assert.Equal(baseline.ValidationObjectIds, new ValiDatas(verification).Extent().Select(value => value.Id).OrderBy(value => value));
        }

        private sealed class PermissionSetupSnapshot
        {
            private readonly string c1String;
            private readonly string deniedProperty;
            private readonly long[] deniedRevocations;
            private readonly Dictionary<long, long[]> rolePermissions;
            private readonly Dictionary<long, long[]> revocationPermissions;

            internal PermissionSetupSnapshot(ITransaction transaction)
            {
                this.C1 = new C1s(transaction).Extent().First(value => value.Name == "c1B");
                this.Denied = new Denieds(transaction).Extent().First;
                this.Unrelated = new C1s(transaction).Extent().First(value => value.Name == "c1A");
                this.UnrelatedName = this.Unrelated.Name;
                this.c1String = this.C1.C1AllorsString;
                this.deniedProperty = this.Denied.DefaultWorkspaceProperty;
                this.deniedRevocations = this.Denied.Revocations.Select(value => value.Id).OrderBy(value => value).ToArray();
                this.rolePermissions = new Roles(transaction).Extent().ToDictionary(role => role.Id,
                    role => role.Permissions.Select(permission => permission.Id).OrderBy(value => value).ToArray());
                this.revocationPermissions = new Revocations(transaction).Extent().ToDictionary(revocation => revocation.Id,
                    revocation => revocation.DeniedPermissions.Select(permission => permission.Id).OrderBy(value => value).ToArray());
                this.ValidationObjectIds = new ValiDatas(transaction).Extent().Select(value => value.Id).OrderBy(value => value).ToArray();
            }

            internal C1 C1 { get; }
            internal Denied Denied { get; }
            internal C1 Unrelated { get; }
            internal string UnrelatedName { get; }
            internal long[] ValidationObjectIds { get; }

            internal long TargetId(string action) => action == "remove-c1-string-read-grant" ? this.C1.Id : this.Denied.Id;

            internal void AssertSecurityUnchanged(ITransaction transaction)
            {
                Assert.Equal(this.c1String, ((C1)transaction.Instantiate(this.C1.Id)).C1AllorsString);
                var denied = (Denied)transaction.Instantiate(this.Denied.Id);
                Assert.Equal(this.deniedProperty, denied.DefaultWorkspaceProperty);
                Assert.Equal(this.deniedRevocations, denied.Revocations.Select(value => value.Id).OrderBy(value => value));
                Assert.Equal(this.rolePermissions.Keys.OrderBy(value => value), new Roles(transaction).Extent().Select(value => value.Id).OrderBy(value => value));
                foreach (var pair in this.rolePermissions)
                {
                    Assert.Equal(pair.Value, ((Role)transaction.Instantiate(pair.Key)).Permissions.Select(value => value.Id).OrderBy(value => value));
                }

                Assert.Equal(this.revocationPermissions.Keys.OrderBy(value => value), new Revocations(transaction).Extent().Select(value => value.Id).OrderBy(value => value));
                foreach (var pair in this.revocationPermissions)
                {
                    Assert.Equal(pair.Value, ((Revocation)transaction.Instantiate(pair.Key)).DeniedPermissions.Select(value => value.Id).OrderBy(value => value));
                }
            }
        }

        private (SqlDatabase Database, LocalReadIdentity Identity, Allors.Workspace.Meta.M Meta) CreateLocalOwnerInputs()
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var user = this.SetUser("jane@example.com");
            this.Transaction.Derive();
            this.Transaction.Commit();
            var population = database.PopulationLifecycle.Renew();
            return (database, new LocalReadIdentity(population.Incarnation, user.Id), new Allors.Workspace.Meta.Lazy.MetaBuilder().Build());
        }

        private sealed class BootstrapServiceProbe
        {
            internal int Workspaces { get; private set; }
            internal int WorkspaceInitializations { get; private set; }
            internal int WorkspaceDisposals { get; private set; }
            internal int Sessions { get; private set; }
            internal int SessionInitializations { get; private set; }
            internal int SessionDisposals { get; private set; }
            internal Allors.Workspace.IWorkspaceServices CreateWorkspaceServices()
            {
                ++this.Workspaces;
                return new WorkspaceProbe(this);
            }
            private sealed class WorkspaceProbe : Allors.Workspace.IWorkspaceServices
            {
                private readonly BootstrapServiceProbe owner;
                private readonly Allors.Workspace.WorkspaceServices inner = new Allors.Workspace.WorkspaceServices();
                internal WorkspaceProbe(BootstrapServiceProbe owner) => this.owner = owner;
                public void OnInit(Allors.Workspace.IWorkspace workspace) { ++this.owner.WorkspaceInitializations; this.inner.OnInit(workspace); }
                public void Dispose() { ++this.owner.WorkspaceDisposals; this.inner.Dispose(); }
                public T Get<T>() => this.inner.Get<T>();
                public Allors.Workspace.ISessionServices CreateSessionServices() { ++this.owner.Sessions; return new SessionProbe(this.owner, this.inner.CreateSessionServices()); }
            }
            private sealed class SessionProbe : Allors.Workspace.ISessionServices
            {
                private readonly BootstrapServiceProbe owner;
                private readonly Allors.Workspace.ISessionServices inner;
                internal SessionProbe(BootstrapServiceProbe owner, Allors.Workspace.ISessionServices inner) { this.owner = owner; this.inner = inner; }
                public void OnInit(Allors.Workspace.ISession session) { ++this.owner.SessionInitializations; this.inner.OnInit(session); }
                public void Dispose() { ++this.owner.SessionDisposals; this.inner.Dispose(); }
                public T Get<T>() => this.inner.Get<T>();
            }
        }

        private sealed class LocalOwnerControlledConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection, Allors.Workspace.Adapters.Remote.IPushReadTransport, Allors.Workspace.Adapters.Remote.IInvokeReadTransport
        {
            private LocalOwnerControlledConnection(Allors.Workspace.Adapters.Remote.Configuration configuration, SqlDatabase database,
                LocalReadIdentity identity, BootstrapServiceProbe services)
                : base(configuration, new Allors.Workspace.Adapters.IdGenerator(), services.CreateWorkspaceServices,
                    new Allors.Ranges.DefaultStructRanges<long>())
            {
                this.Services = services;
                var readConfiguration = new ReadConfiguration(new Dictionary<string, string> { [configuration.Name] = "test-read-v1" });
                using (var scope = new ReadScopeFactory(database, readConfiguration).Open(configuration.Name, identity, null))
                {
                    scope.Complete();
                }
                this.Reads = new LocalOwnerRecordingTransport(new Allors.Workspace.Adapters.Local.ReadTransport(
                    new ReadExecutor(database, readConfiguration), configuration.Name, identity));
                this.pushRead = new Allors.Workspace.Adapters.Local.PushReadTransport(new PushReadExecutor(database, readConfiguration), configuration.Name, identity);
                this.invokeRead = new Allors.Workspace.Adapters.Local.InvokeReadTransport(new InvokeReadExecutor(database, readConfiguration), configuration.Name, identity);
            }

            internal static async System.Threading.Tasks.Task<LocalOwnerControlledConnection> CreateAsync(
                Allors.Workspace.Adapters.Remote.Configuration configuration, SqlDatabase database, LocalReadIdentity identity,
                System.Action<LocalOwnerControlledConnection> configure = null)
            {
                var connection = new LocalOwnerControlledConnection(configuration, database, identity, new BootstrapServiceProbe());
                configure?.Invoke(connection);
                await connection.InitializeAsync(connection.Reads, "test-read-v1", pushReadTransport: connection, invokeReadTransport: connection);
                return connection;
            }

            internal BootstrapServiceProbe Services { get; }
            internal int ApplicationServicesCreated => this.Services.Workspaces;
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.SystemTextJson.UnitConvert();

            private readonly Allors.Workspace.Adapters.Remote.IPushReadTransport pushRead;
            private readonly Allors.Workspace.Adapters.Remote.IInvokeReadTransport invokeRead;
            internal int Invokes { get; private set; }
            internal List<Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest> InvokeRequests { get; } = new List<Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest>();
            internal LocalOwnerRecordingTransport Reads { get; }
            internal int Pushes { get; private set; }
            internal System.Func<System.Threading.Tasks.Task> BeforePush { get; set; }
            internal bool LosePushResponse { get; set; }

            public Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest CaptureRequest(Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest request) => this.invokeRead.CaptureRequest(request);
            public Allors.Protocol.Json.Api.InvokeRead.InvokeReadReceipt CaptureReceipt(string response, Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest request) => this.invokeRead.CaptureReceipt(response, request);
            public object CapturePart(string response, Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest request) => this.invokeRead.CapturePart(response, request);
            public System.Threading.Tasks.Task<string> Send(Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest request, CancellationToken cancellationToken = default)
            {
                ++this.Invokes;
                this.InvokeRequests.Add(this.invokeRead.CaptureRequest(request));
                return this.invokeRead.Send(request, cancellationToken);
            }

            public Allors.Protocol.Json.Api.PushRead.PushReadRequest CaptureRequest(Allors.Protocol.Json.Api.PushRead.PushReadRequest request) => this.pushRead.CaptureRequest(request);
            public Allors.Protocol.Json.Api.PushRead.PushReadReceipt CaptureReceipt(string response, Allors.Protocol.Json.Api.PushRead.PushReadRequest request) => this.pushRead.CaptureReceipt(response, request);
            public object CapturePart(string response, Allors.Protocol.Json.Api.PushRead.PushReadRequest request, bool changed) => this.pushRead.CapturePart(response, request, changed);
            public async System.Threading.Tasks.Task<string> Send(Allors.Protocol.Json.Api.PushRead.PushReadRequest request, CancellationToken cancellationToken = default)
            {
                ++this.Pushes;
                var response = await this.pushRead.Send(request, cancellationToken);
                if (this.BeforePush != null) { await this.BeforePush(); }
                if (this.LosePushResponse) { throw new System.InvalidOperationException("Injected lost response after Local commit."); }
                return response;
            }
        }

        private sealed class LocalOwnerRecordingTransport : Allors.Workspace.Adapters.Remote.IReadTransport
        {
            private readonly Allors.Workspace.Adapters.Remote.IReadTransport inner;
            internal LocalOwnerRecordingTransport(Allors.Workspace.Adapters.Remote.IReadTransport inner) => this.inner = inner;
            internal List<Allors.Protocol.Json.Api.Read.ReadRequest> Requests { get; } = new List<Allors.Protocol.Json.Api.Read.ReadRequest>();
            internal string FailStage { get; set; }
            internal bool Throw { get; set; }
            internal System.Action<Allors.Protocol.Json.Api.Read.ReadRequest> BeforeRead { get; set; }
            public Allors.Protocol.Json.Api.Read.ReadRequest CaptureRequest(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.CaptureRequest(request);
            public object CaptureResponse(object response, Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.CaptureResponse(response, request);
            public int RequestByteCount(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.RequestByteCount(request);
            public System.Threading.Tasks.Task<object> Read(Allors.Protocol.Json.Api.Read.ReadRequest request, CancellationToken cancellationToken = default)
            {
                this.Requests.Add(this.inner.CaptureRequest(request));
                this.BeforeRead?.Invoke(request);
                var fail = request.k == this.FailStage;
                if (fail && this.Throw) { throw new System.InvalidOperationException("Injected Local bootstrap transport failure."); }
                return fail
                    ? System.Threading.Tasks.Task.FromResult<object>(new Allors.Protocol.Json.Api.Read.ReadFailure
                    {
                        i = request.i, k = "error",
                        e = new[] { new Allors.Protocol.Json.Api.Read.ReadError { c = "read-failed", m = "Injected Local bootstrap error result." } },
                    })
                    : this.inner.Read(request, cancellationToken);
            }
        }

        private static Allors.Workspace.Adapters.Remote.Configuration CreateScopedConfiguration(Allors.Workspace.Meta.M metadata) =>
            new Allors.Workspace.Adapters.Remote.Configuration("Default", metadata,
                new Allors.Workspace.Adapters.ReflectionObjectFactory(metadata, typeof(Allors.Workspace.Domain.Person)),
                System.Array.Empty<Allors.Workspace.Derivations.IRule>());

        private async System.Threading.Tasks.Task<ScopedSessionConnection> CreateSessionConnectionAsync(string transport)
        {
            var database = (SqlDatabase)this.Transaction.Database;
            var user = this.SetUser("jane@example.com");
            var target = new C1s(this.Transaction).Extent().First(value => value.Name == "c1B");
            this.Transaction.Derive();
            this.Transaction.Commit();
            var population = database.PopulationLifecycle.Renew();
            var identity = new LocalReadIdentity(population.Incarnation, user.Id);
            var readConfiguration = new ReadConfiguration(new Dictionary<string, string> { ["Default"] = "test-read-v1" });
            var executor = new ReadExecutor(database, readConfiguration);
            var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var configuration = CreateScopedConfiguration(metadata);
            var handler = new ScopedSessionHandler(executor, new PushReadExecutor(database, readConfiguration), new InvokeReadExecutor(database, readConfiguration), identity);
            var http = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            var ids = new Allors.Workspace.Adapters.IdGenerator();
            var ranges = new Allors.Ranges.DefaultStructRanges<long>();
            System.Func<Allors.Workspace.IWorkspaceServices> services = () => new Allors.Workspace.WorkspaceServices();
            Allors.Workspace.Adapters.Remote.DatabaseConnection connection = null;
            try
            {
                connection = transport switch
                {
                    "Local" => await Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(configuration, services, database, identity, ids, ranges, "test-read-v1"),
                    "SystemText" => await Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection.CreateAsync(configuration, services, http, ids, ranges, "test-read-v1"),
                    _ => await Allors.Workspace.Adapters.Remote.Newtonsoft.DatabaseConnection.CreateAsync(configuration, services, http, ids, ranges, "test-read-v1"),
                };
                if (transport != "Local")
                {
                    Assert.Equal(new[] { "read", "read" }, handler.Paths);
                    Assert.Equal(new[] { "discover", "read" }, handler.BootstrapRequests.Select(value => value.k));
                    Assert.All(handler.BootstrapRequests, value => Assert.Empty(value.p.l));
                    handler.Paths.Clear();
                }
                // Only HTTP dispatch exposes endpoint counts; Local shares the scoped Session implementation.
                if (transport == "Local")
                {
                    handler.Paths = null;
                }
                return new ScopedSessionConnection(connection, metadata, target, handler, http);
            }
            catch
            {
                try { if (connection != null) await connection.DisposeAsync(); }
                finally { http.Dispose(); }
                throw;
            }
        }

        private sealed class ScopedSessionConnection : System.IAsyncDisposable
        {
            private readonly System.Net.Http.HttpClient http;
            internal ScopedSessionConnection(Allors.Workspace.Adapters.Remote.DatabaseConnection connection, Allors.Workspace.Meta.M meta, C1 target, ScopedSessionHandler handler, System.Net.Http.HttpClient http)
            {
                this.Connection = connection;
                this.Meta = meta;
                this.Target = target;
                this.Handler = handler;
                this.http = http;
                this.Session = connection.CreateWorkspace().CreateSession();
                this.TargetPull = new Allors.Workspace.Data.Pull
                {
                    ObjectType = meta.C1,
                    ObjectId = target.Id,
                    Results = new[] { new Allors.Workspace.Data.Result { Name = "target" } },
                };
            }
            internal Allors.Workspace.Adapters.Remote.DatabaseConnection Connection { get; }
            internal Allors.Workspace.Meta.M Meta { get; }
            internal C1 Target { get; }
            internal ScopedSessionHandler Handler { get; }
            internal Allors.Workspace.ISession Session { get; }
            internal Allors.Workspace.Data.Pull TargetPull { get; }
            internal void AssertPaths(params string[] expected)
            {
                if (this.Handler.Paths != null)
                {
                    Assert.Equal(expected, this.Handler.Paths);
                }
            }
            public async System.Threading.Tasks.ValueTask DisposeAsync()
            {
                try { await this.Connection.DisposeAsync(); }
                finally { this.http.Dispose(); }
            }
        }

        private sealed class ScopedSessionHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly ReadExecutor executor;
            private readonly PushReadExecutor pushes;
            private readonly InvokeReadExecutor invokes;
            private readonly LocalReadIdentity identity;
            internal ScopedSessionHandler(ReadExecutor executor, PushReadExecutor pushes, InvokeReadExecutor invokes, LocalReadIdentity identity)
            {
                this.executor = executor;
                this.pushes = pushes;
                this.invokes = invokes;
                this.identity = identity;
            }
            internal List<string> Paths { get; set; } = new List<string>();
            internal bool FailMutation { get; set; }
            internal List<Allors.Protocol.Json.Api.Read.ReadRequest> BootstrapRequests { get; } = new List<Allors.Protocol.Json.Api.Read.ReadRequest>();
            internal Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest LastInvocation { get; private set; }
            protected override async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var path = request.RequestUri.Segments.Last();
                this.Paths.Add(path);
                if (path == "read")
                {
                    if (this.BootstrapRequests.Count < 2)
                        this.BootstrapRequests.Add(Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await request.Content.ReadAsStringAsync(cancellationToken)));
                    using var invocation = new System.Net.Http.HttpMessageInvoker(new ReadControllerHandler(this.executor, this.identity.UserId));
                    return await invocation.SendAsync(request, cancellationToken);
                }
                Assert.True(path == "push-read" || path == "invoke-read", "Scoped Session must not call legacy Pull/Sync/Access/Permission endpoints.");
                if (this.FailMutation)
                {
                    throw new System.Net.Http.HttpRequestException("Injected post-dispatch failure; the mutation may have committed.");
                }
                var json = await request.Content.ReadAsStringAsync(cancellationToken);
                if (path == "push-read")
                {
                    var pushed = this.pushes.Execute(json, "Default", this.identity, cancellationToken);
                    return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.StringContent(pushed, System.Text.Encoding.UTF8, "application/json"),
                    };
                }
                this.LastInvocation = Allors.Protocol.Json.SystemTextJson.InvokeReadJson.DeserializeRequest(json);
                var response = this.invokes.Execute(json, "Default", this.identity, cancellationToken);
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(response, System.Text.Encoding.UTF8, "application/json"),
                };
            }
        }

        private sealed class ReadTestConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection
        {
            private ReadTestConnection(Allors.Workspace.Meta.IMetaPopulation metadata)
                : base(new Allors.Workspace.Adapters.Remote.Configuration("Default", metadata,
                    new Allors.Workspace.Adapters.ReflectionObjectFactory(metadata, typeof(Allors.Workspace.Domain.Person)),
                    System.Array.Empty<Allors.Workspace.Derivations.IRule>()), new Allors.Workspace.Adapters.IdGenerator(),
                    () => new Allors.Workspace.WorkspaceServices(), new Allors.Ranges.DefaultStructRanges<long>()) { }

            internal static async System.Threading.Tasks.Task<ReadTestConnection> CreateAsync(Allors.Workspace.Adapters.Remote.IReadTransport read,
                Allors.Workspace.Meta.IMetaPopulation metadata, System.Net.Http.HttpClient client, string revision)
            {
                var connection = new ReadTestConnection(metadata);
                await connection.InitializeAsync(read, revision,
                    pushReadTransport: new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client), invokeReadTransport: new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client));
                return connection;
            }

            internal Allors.Workspace.Adapters.Remote.ReadStore Store => this.ScopedOwner.Store;
            internal System.Threading.Tasks.Task<Allors.Workspace.Adapters.Remote.ReadCompletedResult> Read(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.ScopedOwner.Read(request);
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
        }

        private sealed class CoordinatorRecordingTransport : Allors.Workspace.Adapters.Remote.IReadTransport
        {
            private readonly Allors.Workspace.Adapters.Remote.IReadTransport inner;
            internal CoordinatorRecordingTransport(Allors.Workspace.Adapters.Remote.IReadTransport inner) => this.inner = inner;
            internal List<Allors.Protocol.Json.Api.Read.ReadRequest> Requests { get; } = new List<Allors.Protocol.Json.Api.Read.ReadRequest>();
            internal List<object> Responses { get; } = new List<object>();
            internal System.Action AfterDiscovery { get; set; }
            public Allors.Protocol.Json.Api.Read.ReadRequest CaptureRequest(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.CaptureRequest(request);
            public object CaptureResponse(object response, Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.CaptureResponse(response, request);
            public int RequestByteCount(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.RequestByteCount(request);
            public async System.Threading.Tasks.Task<object> Read(Allors.Protocol.Json.Api.Read.ReadRequest request, CancellationToken cancellationToken = default)
            {
                this.Requests.Add(this.inner.CaptureRequest(request));
                var response = await this.inner.Read(request, cancellationToken);
                this.Responses.Add(response);
                if (request.k == "discover") { this.AfterDiscovery?.Invoke(); }
                return response;
            }
        }

        private sealed class CoordinatorControllerHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly ReadExecutor executor;
            private readonly long userId;
            internal CoordinatorControllerHandler(ReadExecutor executor, long userId)
            {
                this.executor = executor;
                this.userId = userId;
            }

            protected override async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
            {
                using var invocation = new System.Net.Http.HttpMessageInvoker(new ReadControllerHandler(this.executor, this.userId));
                return await invocation.SendAsync(request, cancellationToken);
            }
        }

        private sealed class ReadControllerHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly ReadExecutor executor;
            private readonly ReadResolver resolver;
            private readonly CapturedWorkspace workspace = new CapturedWorkspace();

            internal ReadControllerHandler(ReadExecutor executor, long userId)
            {
                this.executor = executor;
                this.resolver = new ReadResolver(userId);
            }

            internal int Attempts { get; private set; }
            internal int WorkspaceReads => this.workspace.Reads;
            internal int Resolutions => this.resolver.Resolutions;

            protected override async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken cancellationToken)
            {
                ++this.Attempts;
                Assert.Equal("/allors/read", request.RequestUri.AbsolutePath);
                var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
                context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "original") }, "Test"));
                context.Request.Body = new IdentityMutatingStream(await request.Content.ReadAsByteArrayAsync(cancellationToken), () =>
                    context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("user", "changed") }, "Changed")));
                var controller = new ReadController(this.workspace, this.resolver, this.executor)
                {
                    ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = context },
                };
                var result = await controller.Post(cancellationToken);
                if (result is Microsoft.AspNetCore.Mvc.ContentResult content)
                {
                    return new System.Net.Http.HttpResponseMessage((System.Net.HttpStatusCode)(content.StatusCode ?? 200))
                    {
                        Content = new System.Net.Http.StringContent(content.Content),
                    };
                }

                var error = Assert.IsAssignableFrom<Microsoft.AspNetCore.Mvc.ObjectResult>(result);
                return new System.Net.Http.HttpResponseMessage((System.Net.HttpStatusCode)error.StatusCode.Value)
                {
                    Content = new System.Net.Http.StringContent(error.Value?.ToString() ?? string.Empty),
                };
            }
        }

        private sealed class IdentityMutatingStream : System.IO.MemoryStream
        {
            private readonly System.Action mutate;
            internal IdentityMutatingStream(byte[] bytes, System.Action mutate) : base(bytes) => this.mutate = mutate;
            public override System.Threading.Tasks.ValueTask<int> ReadAsync(System.Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                this.mutate();
                return base.ReadAsync(buffer, cancellationToken);
            }
        }

        private sealed class CapturedWorkspace : IWorkspaceService
        {
            internal int Reads { get; private set; }
            public string Name => ++this.Reads == 1 ? "Default" : "invalid-second-read";
        }

        private sealed class ReadResolver : IUserResolver
        {
            private readonly long userId;
            internal ReadResolver(long userId) => this.userId = userId;
            internal int Resolutions { get; private set; }
            public Allors.Database.Domain.User Resolve(System.Security.Claims.ClaimsPrincipal principal, ITransaction transaction)
            {
                ++this.Resolutions;
                Assert.Equal("original", principal.FindFirst("user").Value);
                return (Allors.Database.Domain.User)transaction.Instantiate(this.userId);
            }
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
