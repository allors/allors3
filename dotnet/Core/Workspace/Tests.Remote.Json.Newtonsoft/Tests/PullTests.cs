// <copyright file="PullTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using Allors.Workspace.Adapters.Remote.Newtonsoft;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace.Data;
    using C1 = Allors.Workspace.Domain.C1;
    using PushOptions = Allors.Workspace.PushOptions;
    using MutationOutcome = Allors.Workspace.MutationOutcome;
    using MutationReadStatus = Allors.Workspace.MutationReadStatus;
    using MutationAcknowledgementStatus = Allors.Workspace.MutationAcknowledgementStatus;
    using MutationRefreshApplicationStatus = Allors.Workspace.MutationRefreshApplicationStatus;
    using PushReadWireCodec = Allors.Protocol.Json.Newtonsoft.PushReadJson;
    using InvokeWireCodec = Allors.Protocol.Json.Newtonsoft.InvokeReadJson;
    using ReadWireCodec = Allors.Protocol.Json.Newtonsoft.ReadJson;
    using Polly;
    using Xunit;

    public class PullTests : Workspace.PullTests, IClassFixture<Fixture>
    {
        public PullTests(Fixture fixture) : base(fixture) => this.Profile = new Profile();

        public override IProfile Profile { get; }

        [Fact]
        public async System.Threading.Tasks.Task ReadTransportSendsOneCompleteRequest()
        {
            var profile = (Profile)this.Profile;
            var transport = new ReadTransport(profile.DatabaseConnection.HttpClient);
            var result = await transport.Read("{\"v\":1,\"i\":\"remote\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}");
            var complete = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(result);
            Assert.Equal("remote", complete.i);
            Assert.Equal("core-read-v1", complete.sc.r);
            Assert.Empty(complete.p);
        }

        [Theory]
        [InlineData("{\"v\":1,\"v\":1}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]},\"procedure\":null}")]
        public async System.Threading.Tasks.Task ReadTransportRejectsInvalidRequestBeforeSending(string json)
        {
            var handler = new ReadHandler((_, _) => throw new System.InvalidOperationException("A malformed request must not be sent."));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            await Assert.ThrowsAsync<System.FormatException>(() => new ReadTransport(client).Read(json));
            Assert.Equal(0, handler.Attempts);
        }

        [Theory]
        [InlineData("{\"i\":\"read\",\"i\":\"read\",\"k\":\"error\",\"e\":[{\"c\":\"failed\",\"m\":\"Try again.\"}]}")]
        [InlineData("{\"i\":\"wrong\",\"k\":\"error\",\"e\":[{\"c\":\"failed\",\"m\":\"Try again.\"}]}")]
        public async System.Threading.Tasks.Task ReadTransportStrictlyDecodesTheOriginalResponse(string response)
        {
            var handler = new ReadHandler((_, _) => System.Threading.Tasks.Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(response),
            }));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            await Assert.ThrowsAsync<System.FormatException>(() => new ReadTransport(client).Read(ReadWire));
            Assert.Equal(1, handler.Attempts);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadTransportNeverRetriesHttpFailure()
        {
            var handler = new ReadHandler((_, _) => throw new System.Net.Http.HttpRequestException("Injected failure"));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            await Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(() => new ReadTransport(client).Read(ReadWire));
            Assert.Equal(1, handler.Attempts);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ReadTransportCancellationSuppressesSuccess(bool duringSend)
        {
            using var cancellation = new System.Threading.CancellationTokenSource();
            var handler = new ReadHandler((_, token) =>
            {
                Assert.True(token.CanBeCanceled);
                cancellation.Cancel();
                return System.Threading.Tasks.Task.FromResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent("{\"i\":\"read\",\"k\":\"error\",\"e\":[{\"c\":\"failed\",\"m\":\"Try again.\"}]}"),
                });
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new System.Uri("http://localhost/allors/") };
            if (!duringSend)
            {
                cancellation.Cancel();
            }

            await Assert.ThrowsAnyAsync<System.OperationCanceledException>(() => new ReadTransport(client).Read(ReadWire, cancellation.Token));
            Assert.Equal(duringSend ? 1 : 0, handler.Attempts);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadCoordinatorHttpReadsRetainEquivalentCompleteClosures()
        {
            var profile = (Profile)this.Profile;
            var recording = new CoordinatorTransport(new ReadTransport(profile.DatabaseConnection.HttpClient));
            var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            await using var coordinator = await ReadTestConnection.CreateAsync(recording, metadata, profile.DatabaseConnection.HttpClient, "core-read-v1");
            Assert.Equal(new[] { "discover", "read" }, recording.Phases);
            recording.Phases.Clear();
            recording.Responses.Clear();
            var request = new Allors.Protocol.Json.Api.Read.ReadRequest
            {
                v = 1, i = "coordinator-http", k = "read",
                p = new Allors.Protocol.Json.Api.Read.ReadPull
                {
                    l = new[]
                    {
                        new Allors.Protocol.Json.Data.Pull
                        {
                            e = new Allors.Protocol.Json.Data.Extent
                            {
                                k = Allors.Protocol.Json.Data.ExtentKind.Filter,
                                t = metadata.C1.Tag,
                                s = new[] { new Allors.Protocol.Json.Data.Sort { r = metadata.C1.Name.RelationType.Tag } },
                            },
                            r = new[] { new Allors.Protocol.Json.Data.Result { n = "found" } },
                        },
                        new Allors.Protocol.Json.Data.Pull
                        {
                            t = metadata.C1.Tag, o = 9007199254740991,
                            r = new[] { new Allors.Protocol.Json.Data.Result { n = "missing" } },
                        },
                    },
                    d = System.Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
                },
            };
            var cold = await coordinator.Read(request);
            Assert.NotEmpty(cold.Records);
            Assert.NotEmpty(cold.Security);
            Assert.NotEmpty(cold.Permissions);
            var cached = await coordinator.Read(request);
            var discovered = await coordinator.Read(request);
            Assert.Equal(cold.Records, cached.Records);
            Assert.Equal(cold.Security, cached.Security);
            Assert.Equal(cold.Permissions, cached.Permissions);
            Assert.Equal(cold.Collections["found"], discovered.Collections["found"]);
            Assert.Null(discovered.Objects["missing"]);
            Assert.Equal("unavailable", discovered.Inputs[1].Status);
            Assert.Equal(new[] { "discover", "read", "discover", "read", "discover", "read" }, recording.Phases);
            Assert.All(((Allors.Protocol.Json.Api.Read.ReadSuccess)recording.Responses[3]).p, record => Assert.NotNull(record.p.r));
            await coordinator.DisposeAsync();
            Assert.Null(coordinator.Store.GetRecord(cold.Records.First()));
            Assert.Equal(cold.Records, cached.Records);
            Assert.NotEmpty(discovered.Records);
        }

        [Fact]
        public async System.Threading.Tasks.Task ScopedSessionHttpUsesTwoStepReadAndPreservesPushState()
        {
            var profile = (Profile)this.Profile;
            var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", metadata,
                new Allors.Workspace.Adapters.ReflectionObjectFactory(metadata, typeof(Allors.Workspace.Domain.Person)),
                System.Array.Empty<Allors.Workspace.Derivations.IRule>());
            using var handler = new ScopedSessionHandler(profile.DatabaseConnection.HttpClient);
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = profile.DatabaseConnection.HttpClient.BaseAddress };
            var connection = await DatabaseConnection.CreateAsync(configuration, () => new Allors.Workspace.WorkspaceServices(), client,
                new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "core-read-v1");
            try
            {
                Assert.Equal(new[] { "read", "read" }, handler.Paths);
                handler.Paths.Clear();
                using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
                var pull = new Allors.Workspace.Data.Pull
                {
                    Extent = new Allors.Workspace.Data.Filter(metadata.C1)
                    {
                        Predicate = new Allors.Workspace.Data.Equals(metadata.C1.Name) { Parameter = "name" },
                    },
                    Arguments = new System.Collections.Generic.Dictionary<string, object> { ["name"] = "c1B" },
                    Results = new[] { new Allors.Workspace.Data.Result { Name = "matches" } },
                };
                var result = await session.PullAsync(pull, new Allors.Workspace.Data.Pull
                {
                    ObjectType = metadata.C1, ObjectId = 9007199254740991,
                    Results = new[] { new Allors.Workspace.Data.Result { Name = "missing" } },
                });
                Assert.False(result.HasErrors);
                var target = Assert.Single(result.GetCollection<Allors.Workspace.Domain.C1>("matches"));
                Assert.Equal("c1B", target.Name);
                Assert.Null(result.GetObject<Allors.Workspace.Domain.C1>("missing"));
                Assert.True(target.CanReadName);
                Assert.True(target.CanWriteName);
                Assert.Equal(new[] { "read", "read" }, handler.Paths);
                var original = target.C1AllorsString;
                target.C1AllorsString = "scoped-http-push";
                Assert.False((await session.PushAsync(new PushOptions { Refresh = false })).HasErrors);
                Assert.Equal("scoped-http-push", target.C1AllorsString);
                target.Strategy.Reset();
                Assert.Equal(original, target.C1AllorsString);
                Assert.False((await session.PullAsync(pull)).HasErrors);
                Assert.Equal("scoped-http-push", target.C1AllorsString);
                Assert.Equal(new[] { "read", "read", "push-read", "read", "read" }, handler.Paths);
                await connection.DisposeAsync();
            }
            finally { await connection.DisposeAsync(); }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async System.Threading.Tasks.Task ScopedSessionHttpNeverRetriesMutations(bool invoke)
        {
            var profile = (Profile)this.Profile;
            var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", metadata,
                new Allors.Workspace.Adapters.ReflectionObjectFactory(metadata, typeof(Allors.Workspace.Domain.Person)),
                System.Array.Empty<Allors.Workspace.Derivations.IRule>());
            using var handler = new ScopedSessionHandler(profile.DatabaseConnection.HttpClient) { FailMutation = true };
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = profile.DatabaseConnection.HttpClient.BaseAddress };
            var connection = await DatabaseConnection.CreateAsync(configuration, () => new Allors.Workspace.WorkspaceServices(), client,
                new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "core-read-v1");
            try
            {
                Assert.Equal(new[] { "read", "read" }, handler.Paths);
                handler.Paths.Clear();
                using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
                var result = await session.PullAsync(new Allors.Workspace.Data.Pull
                {
                    Extent = new Allors.Workspace.Data.Filter(metadata.C1)
                    {
                        Predicate = new Allors.Workspace.Data.Equals(metadata.C1.Name) { Value = "c1B" },
                    },
                });
                Assert.False(result.HasErrors);
                var target = Assert.Single(result.GetCollection<Allors.Workspace.Domain.C1>());
                if (invoke)
                {
                    var invoked = await session.InvokeAsync(target.ClassMethod);
                    Assert.Equal(MutationOutcome.Unknown, invoked.Outcome);
                    Assert.True(invoked.ReconciliationRequired);
                    Assert.NotNull(Assert.Single(invoked.Diagnostics.Errors).Cause);
                }
                else
                {
                    target.C1AllorsString = "possibly committed";
                    var rejected = await session.PushAsync(new PushOptions { Refresh = false });
                    Assert.Equal(MutationOutcome.Unknown, rejected.Outcome);
                    Assert.True(rejected.ReconciliationRequired);
                    Assert.True(session.ReconciliationRequired);
                    Assert.True(session.HasChanges);
                }
                Assert.Equal(new[] { "read", "read", invoke ? "invoke-read" : "push-read" }, handler.Paths);
                await connection.DisposeAsync();
            }
            finally { await connection.DisposeAsync(); }
        }

        [Fact]
        public async Task ScopedPushReadHttpChangedOnlyUsesOneRequestAndPreservesNewObjectIdentity()
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            target.C1AllorsString = "changed over HTTP";
            var created = fixture.Session.Create<C1>();
            created.Name = "created by scoped HTTP PushRead";
            created.C1AllorsString = "new object";
            var temporaryId = created.Id;
            fixture.Handler.Clear();

            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true });

            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.False(saved.ReconciliationRequired);
            Assert.Equal(MutationReadStatus.NotRequested, saved.Discovery.Status);
            Assert.Null(saved.PendingQuery);
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            var mapping = Assert.Single(saved.CreationMappings);
            Assert.Equal(temporaryId, mapping.WorkspaceId);
            Assert.Equal(created.Id, mapping.DatabaseId);
            Assert.True(created.Id > 0);
            Assert.Same(created, fixture.Session.Instantiate<C1>(created.Id));
            Assert.Null(fixture.Session.Instantiate<C1>(temporaryId));
            Assert.Equal("changed over HTTP", target.C1AllorsString);
            Assert.Equal("new object", created.C1AllorsString);
            Assert.True(target.CanWriteC1AllorsString);
            Assert.True(created.CanWriteC1AllorsString);
            Assert.False(target.Strategy.HasChanges);
            Assert.False(created.Strategy.HasChanges);
            Assert.Empty(saved.Refresh.Result.Collections);
            Assert.Empty(saved.Refresh.Result.Objects);
            var changed = Assert.IsType<Allors.Protocol.Json.Api.PushRead.PushReadChanged>(fixture.Handler.PushResponses.Single().changed);
            Assert.Contains(changed.p, record => record.i == target.Id);
            Assert.Contains(changed.p, record => record.i == created.Id);
            Assert.All(changed.p, record => Assert.NotNull(record.p.ro));
        }

        [Fact]
        public async Task ScopedPushReadHttpDeferredQueryUsesTwoRequestsAndReusesRefreshedBody()
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            target.C1AllorsString = "saved before query completion";
            var query = fixture.Query();
            fixture.Handler.Clear();

            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { query } });

            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Discovery.Status);
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            Assert.Equal("saved before query completion", target.C1AllorsString);
            query.Arguments["name"] = "c1A";
            var refreshed = await saved.PendingQuery.CompleteAsync();

            Assert.False(refreshed.HasErrors);
            Assert.Same(target, Assert.Single(refreshed.GetCollection<C1>("matches")));
            Assert.Equal(new[] { "push-read", "read" }, fixture.Handler.Paths);
            var request = Assert.Single(fixture.Handler.ReadRequests);
            Assert.Equal("read", request.k);
            var response = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(Assert.Single(fixture.Handler.ReadResponses));
            var changed = Assert.IsType<Allors.Protocol.Json.Api.PushRead.PushReadChanged>(fixture.Handler.PushResponses.Single().changed);
            var committed = changed.p.Single(record => record.i == target.Id);
            var record = response.p.Single(value => value.i == target.Id);
            Assert.NotNull(record.p.r);
            Assert.Null(record.p.ro);
            var retained = request.h.r[record.p.r.Value];
            Assert.Equal(committed.i, retained.i);
            Assert.Equal(committed.c, retained.c);
            Assert.Equal(committed.v, retained.v);
            Assert.Equal(committed.v, record.v);
            await Assert.ThrowsAsync<InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
            Assert.Equal(2, fixture.Handler.Paths.Count);
        }

        [Fact]
        public async Task ScopedPushReadHttpDeferredQueryObservesConcurrentDataAndMembership()
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            target.C1AllorsString = "first commit";
            fixture.Handler.Clear();
            var saved = await fixture.Session.PushAsync(new PushOptions
            {
                Refresh = true,
                Discover = new[]
                {
                    fixture.Query(),
                    new Pull { ObjectId = target.Id, Results = new[] { new Result { Name = "target" } } },
                    new Pull { ObjectType = fixture.Type, ObjectId = 9007199254740991, Results = new[] { new Result { Name = "missing" } } },
                },
            });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Discovery.Status);
            await using var concurrent = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var concurrentTarget = await concurrent.Bind();
            var otherResult = await concurrent.Session.PullAsync(concurrent.Query("c1A"));
            var replacement = Assert.Single(otherResult.GetCollection<C1>("matches"));
            concurrentTarget.Name = "no longer in query";
            concurrentTarget.C1AllorsString = "concurrent commit";
            replacement.Name = "c1B";
            Assert.Equal(MutationOutcome.Committed, (await concurrent.Session.PushAsync(new PushOptions { Refresh = true })).Outcome);

            var refreshed = await saved.PendingQuery.CompleteAsync();

            Assert.False(refreshed.HasErrors);
            Assert.Equal(replacement.Id, Assert.Single(refreshed.GetCollection<C1>("matches")).Id);
            Assert.Same(target, refreshed.GetObject<C1>("target"));
            Assert.Equal("concurrent commit", target.C1AllorsString);
            Assert.Equal("no longer in query", target.Name);
            Assert.Null(refreshed.GetObject<C1>("missing"));
            Assert.Equal("unavailable", Assert.IsType<Allors.Workspace.Adapters.Remote.ScopedPullResult>(refreshed).Inputs[2].Status);
            Assert.Equal(new[] { "push-read", "read" }, fixture.Handler.Paths);
            var response = Assert.IsType<Allors.Protocol.Json.Api.Read.ReadSuccess>(Assert.Single(fixture.Handler.ReadResponses));
            Assert.NotNull(response.p.Single(record => record.i == target.Id).p.ro);
        }

        [Fact]
        public async Task ScopedPushReadHttpDeferredQueryRefreshesConcurrentPermissionChanges()
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            target.C1AllorsString = "saved with read permission";
            fixture.Handler.Clear();
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { fixture.Query() } });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.True(target.CanReadC1AllorsString);
            using var mutation = await ((Profile)this.Profile).DatabaseConnection.HttpClient.PostAsync(
                $"test/permissions/remove-c1-string-read-grant?targetId={target.Id}", null);
            Assert.Equal(System.Net.HttpStatusCode.NoContent, mutation.StatusCode);
            Assert.True(target.CanReadC1AllorsString);

            var refreshed = await saved.PendingQuery.CompleteAsync();

            Assert.False(refreshed.HasErrors);
            Assert.Same(target, Assert.Single(refreshed.GetCollection<C1>("matches")));
            Assert.False(target.CanReadC1AllorsString);
            Assert.Null(target.C1AllorsString);
            Assert.Equal(new[] { "push-read", "read" }, fixture.Handler.Paths);
        }

        [Fact]
        public async Task ScopedPushReadHttpDeferredQueryMarksDeletedObjectsUnavailable()
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            var deleted = fixture.Session.Create<Allors.Workspace.Domain.Cascader>();
            deleted.Cascaded = fixture.Session.Create<Allors.Workspace.Domain.Cascaded>();
            Assert.Equal(MutationOutcome.Committed, (await fixture.Session.PushAsync(new PushOptions { Refresh = true })).Outcome);
            target.C1AllorsString = "save before deletion";
            fixture.Handler.Clear();
            var saved = await fixture.Session.PushAsync(new PushOptions
            {
                Refresh = true,
                Discover = new[] { new Pull { ObjectId = deleted.Id, Results = new[] { new Result { Name = "deleted" } } } },
            });
            Assert.Equal(MutationReadStatus.Succeeded, saved.Discovery.Status);
            using var concurrent = (Allors.Workspace.Adapters.Remote.Session)this.Workspace.CreateSession();
            var loaded = await concurrent.PullAsync(new Pull { ObjectId = deleted.Id });
            Assert.False((await concurrent.InvokeAsync(loaded.GetObject<Allors.Workspace.Domain.Cascader>().Delete)).HasErrors);

            var refreshed = await saved.PendingQuery.CompleteAsync();

            Assert.False(refreshed.HasErrors);
            Assert.Null(refreshed.GetObject<Allors.Workspace.Domain.Cascader>("deleted"));
            Assert.Equal("unavailable", Assert.Single(Assert.IsType<Allors.Workspace.Adapters.Remote.ScopedPullResult>(refreshed).Inputs).Status);
            Assert.False(deleted.Strategy.CanRead(deleted.Strategy.Class.RoleTypes.First()));
            Assert.Equal(new[] { "push-read", "read" }, fixture.Handler.Paths);
        }

        [Theory]
        [InlineData("changed")]
        [InlineData("discovery")]
        public async Task ScopedPushReadHttpKeepsCommitWhenOneReadPartFails(string part)
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            target.C1AllorsString = "committed despite damaged refresh";
            fixture.Handler.Clear();
            fixture.Handler.FailedReadPart = part;

            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { fixture.Query() } });

            Assert.Equal("committed", Assert.Single(fixture.Handler.PushResponses).push.k);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.False(saved.ReconciliationRequired);
            Assert.Equal(part == "changed" ? MutationReadStatus.Failed : MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(part == "changed" ? MutationRefreshApplicationStatus.NotApplicable : MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.Equal(part == "discovery" ? MutationReadStatus.Failed : MutationReadStatus.Succeeded, saved.Discovery.Status);
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            if (part == "changed")
            {
                Assert.False(target.CanWriteC1AllorsString);
                Assert.NotEmpty(saved.Refresh.Errors);
                Assert.False((await saved.PendingQuery.CompleteAsync()).HasErrors);
                Assert.True(target.CanWriteC1AllorsString);
                Assert.Equal(new[] { "push-read", "read" }, fixture.Handler.Paths);
            }
            else
            {
                Assert.Null(saved.PendingQuery);
                Assert.NotEmpty(saved.Discovery.Errors);
                Assert.True(target.CanWriteC1AllorsString);
            }
            Assert.Equal("committed despite damaged refresh", target.C1AllorsString);
            await using var observer = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            Assert.Equal("committed despite damaged refresh", (await observer.Bind()).C1AllorsString);
        }

        [Fact]
        public async Task ScopedPushReadHttpLostCommittedResponseRequiresReconciliationWithoutReplay()
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            var created = fixture.Session.Create<C1>();
            created.Name = "commit with lost response";
            var temporaryId = created.Id;
            target.C1AllorsString = "committed before response loss";
            fixture.Handler.Clear();
            fixture.Handler.LosePushResponse = true;

            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true });

            Assert.Equal("committed", Assert.Single(fixture.Handler.PushResponses).push.k);
            Assert.Equal(MutationOutcome.Unknown, saved.Outcome);
            Assert.True(saved.ReconciliationRequired);
            Assert.NotEmpty(saved.Diagnostics.Errors);
            Assert.Equal(temporaryId, created.Id);
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            Assert.Throws<InvalidOperationException>(() => target.C1AllorsString = "must reconcile first");
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PushAsync());
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            await using var observer = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            Assert.Equal("committed before response loss", (await observer.Bind()).C1AllorsString);
            var observed = await observer.Session.PullAsync(observer.Query("commit with lost response"));
            var persisted = Assert.Single(observed.GetCollection<C1>("matches"));
            Assert.Equal(Assert.Single(fixture.Handler.PushResponses.Single().push.n).d, persisted.Id);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedPushReadHttpPendingCompletionRejectsRetiredConnection(bool duringSave)
        {
            await using var fixture = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            var target = await fixture.Bind();
            target.C1AllorsString = "committed across retirement";
            fixture.Handler.Clear();
            Task closing = null;
            if (duringSave) fixture.Handler.AfterPush = () => closing = fixture.Connection.DisposeAsync();

            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { fixture.Query() } });

            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            if (duringSave)
            {
                Assert.True(saved.ReconciliationRequired);
                Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
                Assert.Equal(MutationReadStatus.Succeeded, saved.Discovery.Status);
                Assert.Equal(MutationAcknowledgementStatus.Failed, saved.Application.Acknowledgement);
                Assert.Equal(MutationRefreshApplicationStatus.Skipped, saved.Application.Refresh);
                Assert.Null(saved.PendingQuery);
            }
            else
            {
                Assert.False(saved.ReconciliationRequired);
                await fixture.Connection.DisposeAsync();
                await Assert.ThrowsAnyAsync<Exception>(() => saved.PendingQuery.CompleteAsync());
                await Assert.ThrowsAsync<InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
            }
            if (closing != null) await closing;
            Assert.Equal(new[] { "push-read" }, fixture.Handler.Paths);
            await using var observer = await PushReadHttpFixture.CreateAsync((Profile)this.Profile);
            Assert.Equal("committed across retirement", (await observer.Bind()).C1AllorsString);
        }

        [Fact]
        public async Task ScopedOwnerHttpBootstrapsBeforeImmediatePushAndClosesEveryWorkspaceSession()
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler();
            var servicesCreated = 0;
            var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler, () =>
            {
                if (servicesCreated == 0) Assert.Equal(new[] { "read", "read" }, handler.Paths);
                ++servicesCreated;
            });
            await using (owner)
            {
                Assert.Equal(1, servicesCreated);
                Assert.Equal(new[] { "read", "read" }, handler.Paths);
                Assert.All(handler.ReadRequests, request => Assert.Empty(request.p.l));
                var first = owner.CreateSession();
                var sibling = owner.CreateSession(owner.Connection.CreateWorkspace());
                Assert.Equal(2, handler.Paths.Count);
                var created = first.Create<C1>();
                created.Name = "owned immediate push";
                Assert.False((await first.PushAsync()).HasErrors);
                Assert.True(created.Id > 0);
                Assert.Equal(new[] { "read", "read", "push-read" }, handler.Paths);
                var pull = new Pull { ObjectType = profile.M.C1, ObjectId = created.Id };
                var result = await sibling.PullAsync(pull);
                Assert.False(result.HasErrors);
                Assert.Equal(created.Name, result.GetObject<C1>().Name);

                var independentHandler = new ScopedOwnerHttpHandler();
                await using var independent = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, independentHandler);
                Assert.NotSame(owner.Connection, independent.Connection);
                Assert.Equal(new[] { "read", "read" }, independentHandler.Paths);
                var observer = independent.CreateSession();
                Assert.Null(observer.Instantiate<C1>(created.Id));
                Assert.False((await observer.PullAsync(pull)).HasErrors);
                Assert.Equal(created.Name, observer.Instantiate<C1>(created.Id).Name);

                Assert.False((await first.PullAsync(pull)).HasErrors);
                created.C1AllorsString = "retained local edit";
                Assert.Equal("retained local edit", created.C1AllorsString);
                Assert.Single(created.Strategy.Diff());
                var diff = created.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)).ToArray();
                await owner.DisposeAsync();
                Assert.True(first.IsClosed);
                Assert.True(sibling.IsClosed);
                Assert.True(handler.IsDisposed);
                Assert.Equal(diff, created.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)));
                Assert.Throws<ObjectDisposedException>(() => owner.CreateSession());
                await Assert.ThrowsAsync<InvalidOperationException>(() => sibling.PullAsync(pull));
                Assert.False(observer.IsClosed);
                Assert.False((await observer.PullAsync(pull)).HasErrors);
            }
        }

        [Fact]
        public async Task NormalHttpConnectionKeepsIdentityAcrossProfileLoginReplacement()
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler();
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            await profile.Login("noperm");
            var session = owner.CreateSession();
            var query = ScopedOwnerQuery(profile);
            var target = Assert.Single((await session.PullAsync(query)).GetCollection<C1>("matches"));
            target.C1AllorsString = "fixed identity push";
            Assert.False((await session.PushAsync(new PushOptions { Refresh = false })).HasErrors);
            Assert.False((await session.PullAsync(query)).HasErrors);
            var invoked = await session.InvokeAsync(target.ClassMethod);
            Assert.False(invoked.HasErrors);
            Assert.Equal(MutationOutcome.Committed, invoked.Outcome);
            Assert.False((await session.PullAsync(query)).HasErrors);
            target.C1AllorsString = "fixed identity push-read";
            var saved = await session.PushAsync(new PushOptions { Refresh = true });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.Equal(new[] { "invoke-read", "push-read", "read" }, handler.Paths.Distinct().OrderBy(value => value));
            Assert.All(handler.Credentials, credential => Assert.Equal(Allors.Database.Domain.Users.AdministratorId.ToString(), credential));

            var replacementHandler = new ScopedOwnerHttpHandler();
            await using var replacement = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, replacementHandler);
            Assert.Equal(new[] { "read", "read" }, replacementHandler.Paths);
            Assert.All(replacementHandler.Credentials, credential => Assert.Equal(Allors.Database.Domain.Users.NoPermId.ToString(), credential));
            var replacementSession = replacement.CreateSession();
            Assert.Null(replacementSession.Instantiate<C1>(target.Id));
            Assert.False(replacementSession.HasChanges);
            await owner.DisposeAsync();
            Assert.True(session.IsClosed);
            Assert.False(replacementSession.IsClosed);
            Assert.False((await replacementSession.PullAsync()).HasErrors);
        }

        [Theory]
        [InlineData("error")]
        [InlineData("transport")]
        public async Task ScopedOwnerHttpFailedBootstrapDisposesTransportWithoutMutationOrRetry(string failure)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler { BootstrapFailure = failure };
            var servicesCreated = 0;
            var exception = await Assert.ThrowsAnyAsync<Exception>(() => profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler, () => ++servicesCreated));
            Assert.Equal(0, servicesCreated);
            Assert.Contains("bootstrap", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(new[] { "read", "read" }, handler.Paths);
            Assert.True(handler.IsDisposed);
            Assert.All(handler.ReadRequests, request => Assert.Empty(request.p.l));
            var recoveryHandler = new ScopedOwnerHttpHandler();
            await using var recovery = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, recoveryHandler);
            Assert.Equal(new[] { "read", "read" }, recoveryHandler.Paths);
            Assert.False((await recovery.CreateSession().PullAsync()).HasErrors);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedOwnerHttpDisposalDrainsCommittedMutationRejectsQueueAndRetainsInspection(bool loseResponse)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler { DelayPushReadResponse = true, LosePushReadResponse = loseResponse };
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            var editing = owner.CreateSession();
            var queued = owner.CreateSession(owner.Connection.CreateWorkspace());
            var target = Assert.Single((await editing.PullAsync(ScopedOwnerQuery(profile))).GetCollection<C1>("matches"));
            target.C1AllorsString = "committed while owner drains";
            var diff = target.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)).ToArray();
            var saving = editing.PushAsync(new PushOptions { Refresh = true });
            await handler.MutationResponded.Task.WaitAsync(TimeSpan.FromSeconds(30));
            var waiting = queued.PullAsync();
            var disposing = owner.DisposeAsync().AsTask();
            try
            {
                Assert.False(disposing.IsCompleted);
                Assert.False(editing.IsClosed);
                Assert.False(queued.IsClosed);
                Assert.False(handler.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => owner.CreateSession());
            }
            finally
            {
                handler.ReleaseMutation.TrySetResult(true);
            }
            var saved = await saving;
            await Assert.ThrowsAsync<InvalidOperationException>(() => waiting);
            await disposing;
            Assert.Equal(loseResponse ? MutationOutcome.Unknown : MutationOutcome.Committed, saved.Outcome);
            Assert.True(saved.ReconciliationRequired);
            Assert.True(editing.IsClosed);
            Assert.True(queued.IsClosed);
            Assert.True(handler.IsDisposed);
            Assert.Equal(diff, target.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)));
            Assert.Equal(new[] { "read", "read", "read", "read", "push-read" }, handler.Paths);
            await Assert.ThrowsAsync<InvalidOperationException>(() => editing.PushAsync());
            Assert.Single(handler.Paths, path => path == "push-read");
            await using var observer = await profile.CreateHttpTestResourcesAsync("core-read-v1", null);
            var observed = Assert.Single((await observer.CreateSession().PullAsync(ScopedOwnerQuery(profile))).GetCollection<C1>("matches"));
            Assert.Equal("committed while owner drains", observed.C1AllorsString);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedPushRefreshAndSkipUseOneRequest(bool skip)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler();
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            var session = owner.CreateSession();
            var created = session.Create<C1>();
            created.Name = "protected default or skip";
            created.C1AllorsString = "accepted saved value";
            var temporaryId = created.Id;
            var beforeSave = handler.Paths.Count;

            var saved = skip ? await session.PushAsync(new PushOptions { Refresh = false }) : await session.PushAsync();

            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.False(saved.HasErrors);
            Assert.Equal(MutationAcknowledgementStatus.Applied, saved.Application.Acknowledgement);
            Assert.Equal(skip ? MutationReadStatus.NotRequested : MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(skip ? MutationRefreshApplicationStatus.NotApplicable : MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            Assert.Equal(new[] { "push-read" }, handler.Paths.Skip(beforeSave));
            var mapping = Assert.Single(saved.CreationMappings);
            Assert.Equal(temporaryId, mapping.WorkspaceId);
            Assert.Equal(created.Id, mapping.DatabaseId);
            Assert.True(created.Id > 0);
            Assert.Same(created, session.Instantiate<C1>(created.Id));
            Assert.Null(session.Instantiate<C1>(temporaryId));
            Assert.Equal(!skip, created.CanWriteC1AllorsString);
            if (!skip) created.C1AllorsString = "later local edit";
            created.Strategy.Reset();
            Assert.Equal(skip ? null : "accepted saved value", created.C1AllorsString);
            if (skip)
            {
                Assert.False((await session.PullAsync(new Pull { ObjectType = profile.M.C1, ObjectId = created.Id })).HasErrors);
                Assert.Equal("accepted saved value", created.C1AllorsString);
                Assert.True(created.CanWriteC1AllorsString);
                Assert.Equal(new[] { "push-read", "read", "read" }, handler.Paths.Skip(beforeSave));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedInvokeHttpRefreshAndSkipUseOneRequest(bool skip)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler();
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            var session = owner.CreateSession();
            var target = (await session.PullAsync(new Pull { Extent = new Filter(profile.M.Organisation) }))
                .GetCollection<Allors.Workspace.Domain.Organisation>().First();
            var version = target.Strategy.Version;
            Assert.False(target.JustDidIt);
            Assert.False(target.JustDidItDerived);
            var before = handler.Paths.Count;

            var invoked = skip
                ? await session.InvokeAsync(target.JustDoIt, new Allors.Workspace.InvokeOptions { Refresh = false })
                : await session.InvokeAsync(target.JustDoIt);

            Assert.Equal(MutationOutcome.Committed, invoked.Outcome);
            Assert.False(invoked.HasErrors);
            Assert.False(invoked.ReconciliationRequired);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
            Assert.Equal(skip ? MutationReadStatus.NotRequested : MutationReadStatus.Succeeded, invoked.Refresh.Status);
            Assert.Equal(skip ? MutationRefreshApplicationStatus.NotApplicable : MutationRefreshApplicationStatus.Applied, invoked.Application.Refresh);
            Assert.Equal(new[] { "invoke-read" }, handler.Paths.Skip(before));
            var request = Assert.Single(handler.InvokeRequests);
            Assert.Equal(!skip, request.refresh);
            Assert.Equal(version, Assert.Single(request.l).v);
            Assert.Equal(!skip, target.JustDidIt);
            Assert.Equal(!skip, target.JustDidItDerived);
            Assert.False(session.HasChanges);
            if (skip)
            {
                Assert.Equal(version, target.Strategy.Version);
                Assert.False((await session.PullAsync(new Pull { Object = target })).HasErrors);
                Assert.True(target.JustDidIt);
                Assert.True(target.JustDidItDerived);
                Assert.Equal(new[] { "invoke-read", "read", "read" }, handler.Paths.Skip(before));
            }
            Assert.True(target.Strategy.Version > version);
            await using var observer = await profile.CreateHttpTestResourcesAsync("core-read-v1", null);
            var persisted = (await observer.CreateSession().PullAsync(new Pull { ObjectType = profile.M.Organisation, ObjectId = target.Id }))
                .GetObject<Allors.Workspace.Domain.Organisation>();
            Assert.True(persisted.JustDidIt);
            Assert.True(persisted.JustDidItDerived);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedInvokeHttpPreservesUnsavedEditsAndReportsConflicts(bool conflict)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler();
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            var session = owner.CreateSession();
            var target = (await session.PullAsync(new Pull { Extent = new Filter(profile.M.Organisation) }))
                .GetCollection<Allors.Workspace.Domain.Organisation>().First();
            var originalName = target.Name;
            var version = target.Strategy.Version;
            session.Checkpoint();
            target.Name = "unsaved Invoke name";
            if (conflict) target.JustDidIt = true;
            var created = session.Create<C1>();
            created.Name = "unsaved Invoke creation";
            var temporaryId = created.Id;
            var before = handler.Paths.Count;

            var invoked = await session.InvokeAsync(target.JustDoIt);

            Assert.Equal(MutationOutcome.Committed, invoked.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, invoked.Refresh.Status);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
            Assert.Equal(conflict ? MutationRefreshApplicationStatus.Conflicted : MutationRefreshApplicationStatus.Applied, invoked.Application.Refresh);
            Assert.Equal(conflict, invoked.HasErrors);
            Assert.False(invoked.ReconciliationRequired);
            Assert.Equal(new[] { "invoke-read" }, handler.Paths.Skip(before));
            Assert.Equal("unsaved Invoke name", target.Name);
            Assert.Equal(temporaryId, created.Id);
            Assert.True(temporaryId < 0);
            Assert.True(created.Strategy.IsNew);
            Assert.Same(created, session.Instantiate<C1>(temporaryId));
            Assert.True(session.HasChanges);
            Assert.Contains(target.Strategy.Diff(), value => value.RelationType == profile.M.Organisation.Name.RelationType);
            Assert.Contains(created.Strategy, session.Checkpoint().Created);
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
            await using var observer = await profile.CreateHttpTestResourcesAsync("core-read-v1", null);
            var observed = observer.CreateSession();
            var persisted = (await observed.PullAsync(new Pull { ObjectType = profile.M.Organisation, ObjectId = target.Id }))
                .GetObject<Allors.Workspace.Domain.Organisation>();
            Assert.Equal(originalName, persisted.Name);
            Assert.True(persisted.JustDidIt);
            Assert.True(persisted.JustDidItDerived);
            Assert.Empty((await observed.PullAsync(new Pull
            {
                Extent = new Filter(profile.M.C1) { Predicate = new Allors.Workspace.Data.Equals(profile.M.C1.Name) { Value = created.Name } },
            })).GetCollection<C1>());
            target.Strategy.Reset();
            Assert.Equal(originalName, target.Name);
            Assert.Equal(!conflict, target.JustDidIt);
            Assert.True(session.HasChanges);
        }

        [Theory]
        [InlineData("damaged-refresh")]
        [InlineData("lost-response")]
        public async Task ProtectedInvokeHttpCommittedReceiptSurvivesRefreshDamageOrLoss(string failure)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler { DamageInvokeRefresh = failure == "damaged-refresh", LoseInvokeResponse = failure == "lost-response" };
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            var session = owner.CreateSession();
            var target = (await session.PullAsync(new Pull { Extent = new Filter(profile.M.Organisation) }))
                .GetCollection<Allors.Workspace.Domain.Organisation>().First();
            var originalVersion = target.Strategy.Version;
            target.Name = "retained after response damage";
            var before = handler.Paths.Count;

            var invoked = await session.InvokeAsync(target.JustDoIt);

            var lost = failure == "lost-response";
            Assert.Equal(lost ? MutationOutcome.Unknown : MutationOutcome.Committed, invoked.Outcome);
            Assert.Equal(lost ? MutationReadStatus.Unavailable : MutationReadStatus.Failed, invoked.Refresh.Status);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
            Assert.Equal(lost, invoked.ReconciliationRequired);
            Assert.True(invoked.HasErrors);
            Assert.Equal("retained after response damage", target.Name);
            Assert.Equal(originalVersion, target.Strategy.Version);
            Assert.False(target.JustDidIt);
            Assert.Equal(new[] { "invoke-read" }, handler.Paths.Skip(before));
            Assert.Single(handler.InvokeRequests);
            if (lost)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => session.InvokeAsync(target.JustDoIt));
                await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync());
            }
            else
            {
                target.Name = "editing remains available";
                Assert.False((await session.PullAsync(new Pull { Object = target })).HasErrors);
                Assert.True(target.JustDidIt);
            }
            await using var observer = await profile.CreateHttpTestResourcesAsync("core-read-v1", null);
            var persisted = (await observer.CreateSession().PullAsync(new Pull { ObjectType = profile.M.Organisation, ObjectId = target.Id }))
                .GetObject<Allors.Workspace.Domain.Organisation>();
            Assert.True(persisted.JustDidIt);
            Assert.True(persisted.JustDidItDerived);
            Assert.Single(handler.InvokeRequests);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedInvokeHttpOwnerDisposalDrainsAndKeepsOutcome(bool loseResponse)
        {
            var profile = (Profile)this.Profile;
            var handler = new ScopedOwnerHttpHandler { DelayInvokeResponse = true, LoseInvokeResponse = loseResponse };
            await using var owner = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, handler);
            var session = owner.CreateSession();
            var queued = owner.CreateSession(owner.Connection.CreateWorkspace());
            var target = (await session.PullAsync(new Pull { Extent = new Filter(profile.M.Organisation) }))
                .GetCollection<Allors.Workspace.Domain.Organisation>().First();
            target.Name = "retained while Invoke drains";
            var diff = target.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)).ToArray();
            var before = handler.Paths.Count;
            var invoking = session.InvokeAsync(target.JustDoIt);
            await handler.MutationResponded.Task.WaitAsync(TimeSpan.FromSeconds(30));
            var waiting = queued.PullAsync();
            var disposing = owner.DisposeAsync().AsTask();
            try
            {
                Assert.False(disposing.IsCompleted);
                Assert.False(session.IsClosed);
                Assert.False(queued.IsClosed);
                Assert.False(handler.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => owner.CreateSession());
            }
            finally { handler.ReleaseMutation.TrySetResult(true); }
            var invoked = await invoking;
            await Assert.ThrowsAsync<InvalidOperationException>(() => waiting);
            await disposing;
            Assert.Equal(loseResponse ? MutationOutcome.Unknown : MutationOutcome.Committed, invoked.Outcome);
            Assert.Equal(loseResponse ? MutationReadStatus.Unavailable : MutationReadStatus.Succeeded, invoked.Refresh.Status);
            Assert.Equal(loseResponse ? MutationRefreshApplicationStatus.NotApplicable : MutationRefreshApplicationStatus.Skipped, invoked.Application.Refresh);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, invoked.Application.Acknowledgement);
            Assert.Equal(loseResponse, invoked.ReconciliationRequired);
            Assert.True(session.IsClosed);
            Assert.True(queued.IsClosed);
            Assert.True(handler.IsDisposed);
            Assert.Equal(diff, target.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)));
            Assert.False((bool?)((Allors.Workspace.Adapters.Remote.Strategy)target.Strategy).DatabaseOriginState.GetUnitRole(profile.M.Organisation.JustDidIt) ?? false);
            Assert.Equal(new[] { "invoke-read" }, handler.Paths.Skip(before));
            Assert.Single(handler.InvokeRequests);
            await using var observer = await profile.CreateHttpTestResourcesAsync("core-read-v1", null);
            var persisted = (await observer.CreateSession().PullAsync(new Pull { ObjectType = profile.M.Organisation, ObjectId = target.Id }))
                .GetObject<Allors.Workspace.Domain.Organisation>();
            Assert.True(persisted.JustDidIt);
            Assert.True(persisted.JustDidItDerived);
        }

        private static Pull ScopedOwnerQuery(Profile profile) => new Pull
        {
            Extent = new Filter(profile.M.C1)
            {
                Predicate = new Allors.Workspace.Data.Equals(profile.M.C1.Name) { Value = "c1B" },
            },
            Results = new[] { new Result { Name = "matches" } },
        };

        [Theory]
        [InlineData("push")]
        [InlineData("invoke")]
        public async Task NormalProfileLoginDrainsBeforeReplacingCapturedIdentity(string operation)
        {
            var handlers = new List<ProfileReplacementHandler>();
            var profile = new Profile(() =>
            {
                var handler = new ProfileReplacementHandler();
                handlers.Add(handler);
                return handler;
            });
            try
            {
                await profile.InitializeAsync();
                var root = profile.DatabaseConnection;
                var workspace = profile.Workspace;
                var session = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
                var queuedSession = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
                var exclusive = (Allors.Workspace.Adapters.Remote.Session)(await profile.CreateExclusiveWorkspaceAsync()).CreateSession();
                var target = session.Create<Allors.Workspace.Domain.Organisation>();
                target.Name = "Profile mutation before replacement";
                Assert.Equal(MutationOutcome.Committed, (await session.PushAsync()).Outcome);
                var held = exclusive.Create<C1>();
                held.Name = "exclusive edit is not transferred";
                var heldId = held.Id;
                var heldDiff = held.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)).ToArray();
                var oldHandler = handlers[0];
                oldHandler.DelayOperation = operation == "push" ? "push-read" : "invoke-read";
                oldHandler.Paths.Clear();
                if (operation == "push") target.Name = "committed before replacement";
                Task<Allors.Workspace.IMutationResult> mutation = operation == "push"
                    ? ObservePush(session)
                    : ObserveInvoke(session, target);
                await oldHandler.MutationResponded.Task.WaitAsync(TimeSpan.FromSeconds(30));
                var queued = queuedSession.PullAsync();
                var replacing = profile.Login("noperm");
                try
                {
                    Assert.False(replacing.IsCompleted);
                    Assert.False(oldHandler.IsDisposed);
                    Assert.False(session.IsClosed);
                    Assert.Throws<ObjectDisposedException>(() => workspace.CreateSession());
                    Assert.Throws<InvalidOperationException>(() => profile.CreateWorkspace());
                }
                finally { oldHandler.ReleaseMutation.TrySetResult(true); }
                var result = await mutation;
                await Assert.ThrowsAsync<InvalidOperationException>(() => queued);
                await replacing;
                Assert.Equal(MutationOutcome.Committed, result.Outcome);
                Assert.Single(oldHandler.Paths);
                Assert.Equal(operation == "push" ? "push-read" : "invoke-read", oldHandler.Paths[0]);
                Assert.All(oldHandler.Credentials, credential => Assert.Equal(Allors.Database.Domain.Users.AdministratorId.ToString(), credential));
                Assert.True(session.IsClosed);
                Assert.True(queuedSession.IsClosed);
                Assert.True(exclusive.IsClosed);
                Assert.True(oldHandler.IsDisposed);
                Assert.True(handlers[1].IsDisposed);
                Assert.NotSame(root, profile.DatabaseConnection);
                Assert.Equal(heldId, held.Id);
                Assert.Equal(heldDiff, held.Strategy.Diff().Cast<Allors.Workspace.IUnitDiff>().Select(value => (value.RelationType, value.OriginalRole, value.ChangedRole)));
                Assert.NotEmpty(held.Strategy.Diff());
                var replacement = (Allors.Workspace.Adapters.Remote.Session)profile.Workspace.CreateSession();
                Assert.False(replacement.HasChanges);
                Assert.Null(replacement.Instantiate<C1>(heldId));
                Assert.Equal(new[] { "read", "read" }, handlers[2].Paths);
                Assert.All(handlers[2].Credentials, credential => Assert.Equal(Allors.Database.Domain.Users.NoPermId.ToString(), credential));
                await using var observer = await ((Profile)this.Profile).CreateHttpTestResourcesAsync("core-read-v1", null);
                var persisted = (await observer.CreateSession().PullAsync(new Pull { ObjectId = target.Id })).GetObject<Allors.Workspace.Domain.Organisation>();
                if (operation == "push") Assert.Equal("committed before replacement", persisted.Name);
                else Assert.True(persisted.JustDidIt);
            }
            finally { await profile.DisposeAsync(); }

            static async Task<Allors.Workspace.IMutationResult> ObservePush(Allors.Workspace.Adapters.Remote.Session session) => await session.PushAsync();
            static async Task<Allors.Workspace.IMutationResult> ObserveInvoke(Allors.Workspace.Adapters.Remote.Session session, Allors.Workspace.Domain.Organisation target) => await session.InvokeAsync(target.JustDoIt);
        }

        [Fact]
        public async Task NormalProfileExclusiveWorkspaceHasIndependentOwnedConnection()
        {
            var handlers = new List<ProfileReplacementHandler>();
            var profile = new Profile(() =>
            {
                var handler = new ProfileReplacementHandler();
                handlers.Add(handler);
                return handler;
            });
            try
            {
                await profile.InitializeAsync();
                var root = profile.DatabaseConnection;
                var first = (Allors.Workspace.Adapters.Remote.Session)profile.Workspace.CreateSession();
                var same = (Allors.Workspace.Adapters.Remote.Session)profile.CreateWorkspace().CreateSession();
                Assert.Single(handlers);
                Assert.Equal(new[] { "read", "read" }, handlers[0].Paths);
                var separateWorkspace = await profile.CreateExclusiveWorkspaceAsync();
                var separate = (Allors.Workspace.Adapters.Remote.Session)separateWorkspace.CreateSession();
                Assert.NotSame(root, ((Allors.Workspace.Adapters.Remote.Workspace)separateWorkspace).DatabaseConnection);
                Assert.Equal(new[] { "read", "read" }, handlers[1].Paths);
                var created = first.Create<C1>();
                created.Name = "independent ready connections";
                Assert.Equal(MutationOutcome.Committed, (await first.PushAsync()).Outcome);
                Assert.Null(separate.Instantiate<C1>(created.Id));
                var loaded = (await separate.PullAsync(new Pull { ObjectId = created.Id })).GetObject<C1>();
                Assert.NotSame(created, loaded);
                Assert.Equal(created.Name, loaded.Name);
                var callerHandler = new ProfileReplacementHandler();
                await using var callerOwned = await profile.CreateHttpTestResourcesAsync("core-read-v1", null, callerHandler);
                var callerSession = callerOwned.CreateSession();
                await profile.DisposeAsync();
                Assert.True(first.IsClosed);
                Assert.True(same.IsClosed);
                Assert.True(separate.IsClosed);
                Assert.All(handlers, handler => Assert.True(handler.IsDisposed));
                Assert.False(callerHandler.IsDisposed);
                Assert.False(callerSession.IsClosed);
                Assert.False((await callerSession.PullAsync(new Pull { ObjectId = created.Id })).HasErrors);
                await callerOwned.DisposeAsync();
                Assert.True(callerSession.IsClosed);
                Assert.True(callerHandler.IsDisposed);
            }
            finally { await profile.DisposeAsync(); }
        }

        [Theory]
        [InlineData("discover")]
        [InlineData("read")]
        public async Task NormalProfileFailedReplacementPublishesNoWorkspace(string stage)
        {
            var handlers = new List<ProfileReplacementHandler>();
            string failure = null;
            var profile = new Profile(() =>
            {
                var handler = new ProfileReplacementHandler { FailBootstrapStage = failure };
                handlers.Add(handler);
                return handler;
            });
            try
            {
                await profile.InitializeAsync();
                var old = (Allors.Workspace.Adapters.Remote.Session)profile.Workspace.CreateSession();
                failure = stage;
                var error = await Assert.ThrowsAnyAsync<Exception>(() => profile.Login("noperm"));
                Assert.Contains("bootstrap", error.Message, StringComparison.OrdinalIgnoreCase);
                Assert.True(old.IsClosed);
                Assert.True(handlers[0].IsDisposed);
                Assert.Null(profile.Workspace);
                Assert.Null(profile.DatabaseConnection);
                Assert.Throws<InvalidOperationException>(() => profile.CreateWorkspace());
                Assert.True(handlers[1].IsDisposed);
                Assert.Equal(stage == "discover" ? 1 : 2, handlers[1].Paths.Count);
                Assert.All(handlers[1].Paths, path => Assert.Equal("read", path));
                Assert.All(handlers[1].ReadRequests, request => Assert.Empty(request.p.l));
                failure = null;
                await profile.Login("administrator");
                Assert.Equal(new[] { "read", "read" }, handlers[2].Paths);
                var recovered = profile.Workspace.CreateSession();
                Assert.False(recovered.HasChanges);
                Assert.Equal(MutationOutcome.Committed, (await recovered.PushAsync()).Outcome);
            }
            finally { await profile.DisposeAsync(); }
        }

        private sealed class ProfileReplacementHandler : System.Net.Http.DelegatingHandler
        {
            internal ProfileReplacementHandler() : base(new System.Net.Http.HttpClientHandler()) { }
            internal List<string> Paths { get; } = new List<string>();
            internal List<string> Credentials { get; } = new List<string>();
            internal List<Allors.Protocol.Json.Api.Read.ReadRequest> ReadRequests { get; } = new List<Allors.Protocol.Json.Api.Read.ReadRequest>();
            internal string FailBootstrapStage { get; set; }
            internal string DelayOperation { get; set; }
            internal bool IsDisposed { get; private set; }
            internal TaskCompletionSource<bool> MutationResponded { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal TaskCompletionSource<bool> ReleaseMutation { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                Assert.False(this.IsDisposed);
                var path = request.RequestUri.Segments.Last();
                this.Paths.Add(path);
                this.Credentials.Add(Assert.Single(request.Headers.GetValues("X-Allors-TestUser")));
                if (path == "read")
                {
                    var read = ReadWireCodec.DeserializeRequest(await request.Content.ReadAsStringAsync(cancellationToken));
                    this.ReadRequests.Add(read);
                    if (read.k == this.FailBootstrapStage)
                    {
                        return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                        {
                            Content = new System.Net.Http.StringContent("{\"i\":\"" + read.i + "\",\"k\":\"error\",\"e\":[{\"c\":\"bootstrap-failed\",\"m\":\"Injected Profile bootstrap failure.\"}]}"),
                        };
                    }
                }
                var response = await base.SendAsync(request, cancellationToken);
                if (path == this.DelayOperation)
                {
                    response.EnsureSuccessStatusCode();
                    var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                    var body = await request.Content.ReadAsStringAsync(cancellationToken);
                    if (path == "push-read") Assert.Equal("committed", PushReadWireCodec.DeserializeResponse(raw, PushReadWireCodec.DeserializeRequest(body)).push.k);
                    else Assert.Equal("committed", InvokeWireCodec.DeserializeResponse(raw, InvokeWireCodec.DeserializeRequest(body)).invoke.k);
                    this.MutationResponded.TrySetResult(true);
                    await this.ReleaseMutation.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
                    Assert.False(this.IsDisposed);
                }
                return response;
            }

            protected override void Dispose(bool disposing)
            {
                this.IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class ScopedOwnerHttpHandler : System.Net.Http.DelegatingHandler
        {
            internal ScopedOwnerHttpHandler() : base(new System.Net.Http.HttpClientHandler()) { }
            internal List<string> Paths { get; } = new List<string>();
            internal List<string> Credentials { get; } = new List<string>();
            internal List<Allors.Protocol.Json.Api.Read.ReadRequest> ReadRequests { get; } = new List<Allors.Protocol.Json.Api.Read.ReadRequest>();
            internal string BootstrapFailure { get; set; }
            internal List<Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest> InvokeRequests { get; } = new List<Allors.Protocol.Json.Api.InvokeRead.InvokeReadRequest>();
            internal bool DelayInvokeResponse { get; set; }
            internal bool LoseInvokeResponse { get; set; }
            internal bool DamageInvokeRefresh { get; set; }
            internal bool DelayPushReadResponse { get; set; }
            internal bool LosePushReadResponse { get; set; }
            internal bool IsDisposed { get; private set; }
            internal TaskCompletionSource<bool> MutationResponded { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal TaskCompletionSource<bool> ReleaseMutation { get; } = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var path = request.RequestUri.Segments.Last();
                this.Paths.Add(path);
                this.Credentials.Add(Assert.Single(request.Headers.GetValues("X-Allors-TestUser")));
                if (path == "read")
                {
                    var read = ReadWireCodec.DeserializeRequest(await request.Content.ReadAsStringAsync(cancellationToken));
                    this.ReadRequests.Add(read);
                    if (this.BootstrapFailure != null && read.k == "read")
                    {
                        if (this.BootstrapFailure == "transport")
                            throw new System.Net.Http.HttpRequestException("Injected bootstrap transport failure.");
                        return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                        {
                            Content = new System.Net.Http.StringContent("{\"i\":\"" + read.i + "\",\"k\":\"error\",\"e\":[{\"c\":\"bootstrap-failed\",\"m\":\"Injected bootstrap error result.\"}]}"),
                        };
                    }
                }
                var response = await base.SendAsync(request, cancellationToken);
                if (path == "push-read")
                {
                    response.EnsureSuccessStatusCode();
                    var body = await request.Content.ReadAsStringAsync(cancellationToken);
                    var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                    Assert.Equal("committed", PushReadWireCodec.DeserializeResponse(raw, PushReadWireCodec.DeserializeRequest(body)).push.k);
                    this.MutationResponded.TrySetResult(true);
                    if (this.DelayPushReadResponse) await this.ReleaseMutation.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
                    if (this.LosePushReadResponse)
                    {
                        response.Dispose();
                        throw new System.Net.Http.HttpRequestException("Injected loss of a committed mutation response.");
                    }
                }
                if (path == "invoke-read")
                {
                    response.EnsureSuccessStatusCode();
                    var captured = InvokeWireCodec.DeserializeRequest(await request.Content.ReadAsStringAsync(cancellationToken));
                    this.InvokeRequests.Add(captured);
                    var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                    Assert.Equal("committed", InvokeWireCodec.DeserializeResponse(raw, captured).invoke.k);
                    this.MutationResponded.TrySetResult(true);
                    if (this.DelayInvokeResponse) await this.ReleaseMutation.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
                    if (this.LoseInvokeResponse)
                    {
                        response.Dispose();
                        throw new System.Net.Http.HttpRequestException("Injected loss after the server committed Invoke.");
                    }
                    if (this.DamageInvokeRefresh)
                    {
                        var changed = System.Text.Json.Nodes.JsonNode.Parse(raw);
                        changed["changed"] = 17;
                        response.Content.Dispose();
                        response.Content = new System.Net.Http.StringContent(changed.ToJsonString(), System.Text.Encoding.UTF8, "application/json");
                    }
                }
                return response;
            }

            protected override void Dispose(bool disposing)
            {
                this.IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        private sealed class PushReadHttpFixture : IAsyncDisposable
        {
            private readonly System.Net.Http.HttpClient client;
            internal readonly PushReadHttpHandler Handler;
            internal readonly DatabaseConnection Connection;
            internal readonly Allors.Workspace.Adapters.Remote.Session Session;
            internal readonly Allors.Workspace.Meta.IClass Type;
            private readonly Allors.Workspace.Meta.IRoleType name;

            private PushReadHttpFixture(System.Net.Http.HttpClient client, PushReadHttpHandler handler,
                DatabaseConnection connection, Allors.Workspace.Meta.M metadata)
            {
                this.client = client;
                this.Handler = handler;
                this.Connection = connection;
                this.Type = metadata.C1;
                this.name = metadata.C1.Name;
                this.Session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            }

            internal static async Task<PushReadHttpFixture> CreateAsync(Profile profile)
            {
                var metadata = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
                var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", metadata,
                    new Allors.Workspace.Adapters.ReflectionObjectFactory(metadata, typeof(Allors.Workspace.Domain.Person)),
                    Array.Empty<Allors.Workspace.Derivations.IRule>());
                var handler = new PushReadHttpHandler(profile.DatabaseConnection.HttpClient);
                var client = new System.Net.Http.HttpClient(handler) { BaseAddress = profile.DatabaseConnection.HttpClient.BaseAddress };
                DatabaseConnection connection = null;
                try
                {
                    connection = await DatabaseConnection.CreateAsync(configuration, () => new Allors.Workspace.WorkspaceServices(), client,
                        new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "core-read-v1");
                    Assert.Equal(new[] { "read", "read" }, handler.Paths);
                    Assert.Equal(new[] { "discover", "read" }, handler.ReadRequests.Select(request => request.k));
                    Assert.All(handler.ReadRequests, request => Assert.Empty(request.p.l));
                    handler.Clear();
                    return new PushReadHttpFixture(client, handler, connection, metadata);
                }
                catch
                {
                    try { if (connection != null) await connection.DisposeAsync(); }
                    finally { client.Dispose(); }
                    throw;
                }
            }

            internal Pull Query(string value = "c1B") => new Pull
            {
                Extent = new Filter(this.Type) { Predicate = new Allors.Workspace.Data.Equals(this.name) { Parameter = "name" } },
                Arguments = new Dictionary<string, object> { ["name"] = value },
                Results = new[] { new Result { Name = "matches" } },
            };

            internal async Task<C1> Bind()
            {
                var result = await this.Session.PullAsync(this.Query());
                Assert.False(result.HasErrors);
                return Assert.Single(result.GetCollection<C1>("matches"));
            }

            public async ValueTask DisposeAsync()
            {
                try { await this.Connection.DisposeAsync(); }
                finally { this.client.Dispose(); }
            }
        }

        private sealed class PushReadHttpHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly System.Net.Http.HttpClient inner;
            internal PushReadHttpHandler(System.Net.Http.HttpClient inner) => this.inner = inner;
            internal List<string> Paths { get; } = new List<string>();
            internal List<Allors.Protocol.Json.Api.PushRead.PushReadResponse> PushResponses { get; } = new List<Allors.Protocol.Json.Api.PushRead.PushReadResponse>();
            internal List<Allors.Protocol.Json.Api.Read.ReadRequest> ReadRequests { get; } = new List<Allors.Protocol.Json.Api.Read.ReadRequest>();
            internal List<object> ReadResponses { get; } = new List<object>();
            internal string FailedReadPart { get; set; }
            internal bool LosePushResponse { get; set; }
            internal Action AfterPush { get; set; }

            internal void Clear()
            {
                this.Paths.Clear();
                this.PushResponses.Clear();
                this.ReadRequests.Clear();
                this.ReadResponses.Clear();
            }

            protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var path = request.RequestUri.Segments.Last();
                Assert.True(path == "read" || path == "push-read", "Scoped PushRead must use only read and push-read endpoints.");
                this.Paths.Add(path);
                var body = await request.Content.ReadAsStringAsync(cancellationToken);
                using var forwarded = new System.Net.Http.HttpRequestMessage(request.Method, request.RequestUri)
                {
                    Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json"),
                };
                var response = await this.inner.SendAsync(forwarded, cancellationToken);
                response.EnsureSuccessStatusCode();
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
                if (path == "read")
                {
                    var read = ReadWireCodec.DeserializeRequest(body);
                    this.ReadRequests.Add(read);
                    this.ReadResponses.Add(ReadWireCodec.DeserializeResponse(raw, read));
                }
                else
                {
                    var push = PushReadWireCodec.DeserializeRequest(body);
                    this.PushResponses.Add(PushReadWireCodec.DeserializeResponse(raw, push));
                    this.AfterPush?.Invoke();
                    if (this.LosePushResponse)
                    {
                        response.Dispose();
                        throw new System.Net.Http.HttpRequestException("Injected loss after the real HTTP server returned its committed receipt.");
                    }
                    if (this.FailedReadPart != null)
                    {
                        // Damage only one read part after the real server has committed and returned both outcomes.
                        var marker = "\"" + this.FailedReadPart + "\":{";
                        Assert.Contains(marker, raw);
                        response.Content.Dispose();
                        response.Content = new System.Net.Http.StringContent(raw.Replace(marker, marker + "\"k\":\"invalid\","), System.Text.Encoding.UTF8, "application/json");
                    }
                }
                return response;
            }
        }

        private sealed class ScopedSessionHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly System.Net.Http.HttpClient inner;
            internal ScopedSessionHandler(System.Net.Http.HttpClient inner) => this.inner = inner;
            internal System.Collections.Generic.List<string> Paths { get; } = new System.Collections.Generic.List<string>();
            internal bool FailMutation { get; set; }
            protected override async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var path = request.RequestUri.Segments[request.RequestUri.Segments.Length - 1];
                this.Paths.Add(path);
                Assert.True(path == "read" || path == "push-read" || path == "invoke-read", "Scoped Session must not call legacy Pull/Sync/Access/Permission endpoints.");
                if (path != "read" && this.FailMutation)
                {
                    throw new System.Net.Http.HttpRequestException("Injected post-dispatch failure; the mutation may have committed.");
                }
                using var forwarded = new System.Net.Http.HttpRequestMessage(request.Method, request.RequestUri)
                {
                    Content = new System.Net.Http.StringContent(await request.Content.ReadAsStringAsync(cancellationToken), System.Text.Encoding.UTF8, "application/json"),
                };
                return await this.inner.SendAsync(forwarded, cancellationToken);
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
                    pushReadTransport: new PushReadTransport(client), invokeReadTransport: new InvokeReadTransport(client));
                return connection;
            }

            internal Allors.Workspace.Adapters.Remote.ReadStore Store => this.ScopedOwner.Store;
            internal System.Threading.Tasks.Task<Allors.Workspace.Adapters.Remote.ReadCompletedResult> Read(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.ScopedOwner.Read(request);
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.Newtonsoft.UnitConvert();
        }

        private sealed class CoordinatorTransport : Allors.Workspace.Adapters.Remote.IReadTransport
        {
            private readonly Allors.Workspace.Adapters.Remote.IReadTransport inner;
            internal CoordinatorTransport(Allors.Workspace.Adapters.Remote.IReadTransport inner) => this.inner = inner;
            internal System.Collections.Generic.List<string> Phases { get; } = new System.Collections.Generic.List<string>();
            internal System.Collections.Generic.List<object> Responses { get; } = new System.Collections.Generic.List<object>();
            public Allors.Protocol.Json.Api.Read.ReadRequest CaptureRequest(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.CaptureRequest(request);
            public object CaptureResponse(object response, Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.CaptureResponse(response, request);
            public int RequestByteCount(Allors.Protocol.Json.Api.Read.ReadRequest request) => this.inner.RequestByteCount(request);
            public async System.Threading.Tasks.Task<object> Read(Allors.Protocol.Json.Api.Read.ReadRequest request, System.Threading.CancellationToken cancellationToken = default)
            {
                this.Phases.Add(request.k);
                var response = await this.inner.Read(request, cancellationToken);
                this.Responses.Add(response);
                return response;
            }
        }

        private const string ReadWire = "{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";

        private sealed class ReadHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly System.Func<System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken, System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>> send;
            internal ReadHandler(System.Func<System.Net.Http.HttpRequestMessage, System.Threading.CancellationToken, System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>> send) => this.send = send;
            internal int Attempts { get; private set; }
            protected override System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                ++this.Attempts;
                Assert.Equal("/allors/read", request.RequestUri.AbsolutePath);
                return this.send(request, cancellationToken);
            }
        }
    }
}
