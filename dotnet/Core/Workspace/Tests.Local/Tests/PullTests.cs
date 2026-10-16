// <copyright file="PullTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Services;
    using Allors.Database.Tracing;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Api.InvokeRead;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;
    using Allors.Workspace.Data;
    using Newtonsoft.Json;
    using Xunit;
    using PushOptions = Allors.Workspace.PushOptions;
    using MutationOutcome = Allors.Workspace.MutationOutcome;
    using MutationReadStatus = Allors.Workspace.MutationReadStatus;
    using MutationAcknowledgementStatus = Allors.Workspace.MutationAcknowledgementStatus;
    using MutationRefreshApplicationStatus = Allors.Workspace.MutationRefreshApplicationStatus;
    using MutationApplicationStage = Allors.Workspace.MutationApplicationStage;

    using JsonSerializer = System.Text.Json.JsonSerializer;
    using ReadPermission = Allors.Protocol.Json.Api.Read.ReadPermission;
    using ReadSecurityKey = Allors.Workspace.Adapters.Remote.ReadSecurityKey;
    using ReadStore = Allors.Workspace.Adapters.Remote.ReadStore;
    using ReadStoreScope = Allors.Workspace.Adapters.Remote.ReadStoreScope;
    using ReadStoredPermission = Allors.Workspace.Adapters.Remote.ReadStoredPermission;
    using ReadStoredRecord = Allors.Workspace.Adapters.Remote.ReadStoredRecord;
    using ReadStoredSecurity = Allors.Workspace.Adapters.Remote.ReadStoredSecurity;

    public class PullTests : Workspace.PullTests, IClassFixture<Fixture>
    {
        public PullTests(Fixture fixture) : base(fixture) => this.Profile = new Profile(fixture);

        public static TheoryData<string, object> ProtocolUnits => new()
        {
            { UnitTags.Binary, new byte[] { 0, 127, 255 } },
            { UnitTags.Boolean, true },
            { UnitTags.Boolean, false },
            { UnitTags.DateTime, new DateTime(2026, 10, 9, 12, 34, 56, DateTimeKind.Utc) },
            { UnitTags.Decimal, 123456789.0123456789m },
            { UnitTags.Float, -123.456d },
            { UnitTags.Integer, int.MinValue },
            { UnitTags.Integer, int.MaxValue },
            { UnitTags.String, "a string" },
            { UnitTags.Unique, new Guid("0208bb9b-e87b-4ced-8dec-516e6778cd66") },
            { UnitTags.String, null },
        };

        [Fact]
        public void NormalConnectionPublicContractHasOnlyReadyFactories()
        {
            foreach (var type in new[] { typeof(Allors.Workspace.Adapters.Local.DatabaseConnection), typeof(Allors.Workspace.Adapters.Remote.Newtonsoft.DatabaseConnection), typeof(Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection) })
            {
                Assert.Empty(type.GetConstructors());
                Assert.NotNull(type.GetMethod("CreateAsync"));
                Assert.NotNull(type.GetMethod("DisposeAsync"));
                foreach (var name in new[] { "CreateScoped", "Retire", "RetireAsync", "Pull", "Sync", "Push", "Invoke", "Access", "Permission" }) Assert.Null(type.GetMethod(name));
                Assert.Null(type.GetProperty("IsScoped"));
                Assert.Null(type.GetProperty("Policy"));
            }
            Assert.Empty(typeof(Allors.Workspace.Adapters.Remote.Workspace).GetConstructors());
            Assert.Empty(typeof(Allors.Workspace.Adapters.Remote.Session).GetConstructors());
            Assert.False(typeof(ReadStore).IsPublic);
            Assert.False(typeof(Allors.Workspace.Adapters.Remote.ReadCoordinator).IsPublic);
            Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(Allors.Workspace.Adapters.Remote.ReadCompletedResult)));
            Assert.Null(typeof(ReadStore).Assembly.GetType("Allors.Workspace.Adapters.Remote.ReadStorePin"));
            Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(Allors.Workspace.ISession)));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NormalHttpFactoryBootstrapsOnceWithoutApplicationServices(bool newtonsoft)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var services = new List<LifecycleWorkspaceServices>();
            var factory = new ScopedFactory(handler.Meta) { OnCreate = () => throw new InvalidOperationException("No objects during empty bootstrap.") };
            var configuration = handler.Configuration(factory: factory);
            var connection = await handler.CreateAsync(client, newtonsoft, () => { var value = new LifecycleWorkspaceServices(); services.Add(value); return value; }, configuration, "captured");
            Assert.Equal(new[] { "discover", "read" }, handler.Reads.Select(v => v.k));
            Assert.All(handler.Reads, request => { Assert.Empty(request.p.l); Assert.Equal("captured", request.x); });
            Assert.Empty(services);
            var workspace = connection.CreateWorkspace();
            var session = workspace.CreateSession();
            connection.CreateWorkspace().CreateSession();
            Assert.Equal(2, handler.Reads.Count);
            Assert.Equal(MutationOutcome.Committed, (await session.PushAsync()).Outcome);
            Assert.Equal(MutationOutcome.Committed, (await session.InvokeAsync(Array.Empty<Allors.Workspace.Method>())).Outcome);
            Assert.Equal(1, handler.Pushes);
            Assert.Equal(1, handler.Invokes);
            await connection.DisposeAsync();
            Assert.All(services, value => { Assert.Equal(1, value.Disposals); Assert.All(value.Sessions, item => Assert.Equal(1, item.Disposals)); });
            Assert.True((await client.GetAsync("borrowed")).IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(false, "discover", "structured")]
        [InlineData(false, "discover", "transport")]
        [InlineData(false, "discover", "malformed")]
        [InlineData(false, "discover", "cancel")]
        [InlineData(false, "read", "structured")]
        [InlineData(false, "read", "transport")]
        [InlineData(false, "read", "malformed")]
        [InlineData(false, "read", "cancel")]
        [InlineData(true, "discover", "structured")]
        [InlineData(true, "discover", "transport")]
        [InlineData(true, "discover", "malformed")]
        [InlineData(true, "discover", "cancel")]
        [InlineData(true, "read", "structured")]
        [InlineData(true, "read", "transport")]
        [InlineData(true, "read", "malformed")]
        [InlineData(true, "read", "cancel")]
        public async Task NormalFactoryBootstrapFailureReturnsNoConnectionAndBorrowsTransport(bool newtonsoft, string stage, string failure)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            using var cancellation = new System.Threading.CancellationTokenSource();
            var services = 0;
            handler.OnRead = request =>
            {
                if (request.k != stage) return Task.FromResult(EmptyBootstrapResponse(request));
                if (failure == "transport") throw new System.Net.Http.HttpRequestException("Bootstrap transport failed.");
                if (failure == "cancel") { cancellation.Cancel(); return Task.FromResult(EmptyBootstrapResponse(request)); }
                return Task.FromResult<object>(failure == "malformed" ? "{invalid" : new ReadFailure
                { i = request.i, k = "error", e = new[] { new ReadError { c = "bootstrap-refused", m = "Create a connection with a current identity.", o = new long[] { 999 } } } });
            };
            var error = await Record.ExceptionAsync(() => handler.CreateAsync(client, newtonsoft, () => { ++services; return new LifecycleWorkspaceServices(); }, cancellationToken: cancellation.Token));
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
            if (failure == "structured")
            {
                var diagnostic = Assert.IsType<Allors.Workspace.Adapters.Remote.ReadOperationException>(error);
                Assert.Equal("bootstrap-refused", Assert.Single(diagnostic.Errors).Code);
                Assert.Equal(999, Assert.Single(diagnostic.Errors.Single().ObjectIds));
            }
            Assert.Equal(stage == "discover" ? 1 : 2, handler.Reads.Count);
            Assert.Equal(0, services);
            Assert.Equal(0, handler.Pushes + handler.Invokes);
            Assert.True((await client.GetAsync("borrowed")).IsSuccessStatusCode);
            handler.OnRead = null;
            await using var replacement = await handler.CreateAsync(client, newtonsoft);
            Assert.NotNull(replacement.CreateWorkspace().CreateSession());
        }

        [Theory]
        [InlineData("read")]
        [InlineData("push")]
        [InlineData("invoke")]
        public async Task NormalFactoryRejectsMissingCapabilitiesBeforeBootstrap(string missing)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var services = 0;
            FactoryTestConnection captured = null;
            await Assert.ThrowsAsync<ArgumentException>(() => FactoryTestConnection.CreateAsync(handler.Configuration(), () => { ++services; return new LifecycleWorkspaceServices(); },
                missing == "read" ? null : new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client),
                missing == "push" ? null : new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client),
                missing == "invoke" ? null : new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client), capture: value => captured = value));
            Assert.Empty(handler.Reads);
            Assert.Equal(0, services + handler.Pushes + handler.Invokes);
            Assert.Throws<ObjectDisposedException>(() => captured.CreateWorkspace());
            Assert.True((await client.GetAsync("borrowed")).IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NormalFactoryCapturesScopeAndRulesBeforeAwait(bool newtonsoft)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var first = new LifecycleRule();
            var replacement = new LifecycleRule();
            var rules = new Allors.Workspace.Derivations.IRule[] { first };
            var configuration = handler.Configuration(rules);
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            handler.OnRead = async request => { if (request.k == "discover") { entered.TrySetResult(true); await release.Task; } return EmptyBootstrapResponse(request); };
            var creating = handler.CreateAsync(client, newtonsoft, configuration: configuration, context: "fixed");
            await entered.Task;
            rules[0] = replacement;
            configuration.Rules[0] = replacement;
            release.SetResult(true);
            await using var connection = await creating;
            Assert.Same(first, Assert.Single(connection.Configuration.Rules));
            Assert.All(handler.Reads, request => Assert.Equal("fixed", request.x));
            Assert.Equal("Default", ScopedStore(connection).Scope.Workspace);
            Assert.Equal("v1", ScopedStore(connection).Scope.Revision);
            var prior = handler.Reads.Count;
            using var canceled = new System.Threading.CancellationTokenSource();
            canceled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.CreateAsync(client, newtonsoft, cancellationToken: canceled.Token));
            Assert.Equal(prior, handler.Reads.Count);
            using var raced = new System.Threading.CancellationTokenSource();
            handler.OnRead = request => { if (request.k == "read") raced.Cancel(); return Task.FromResult(EmptyBootstrapResponse(request)); };
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.CreateAsync(client, newtonsoft, cancellationToken: raced.Token));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NormalConnectionTracksAllWorkspaceSessionsAndIdempotentClosure(bool newtonsoft)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var connection = await handler.CreateAsync(client, newtonsoft);
            var workspace = connection.CreateWorkspace();
            var first = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
            var second = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var created = first.Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "held";
            first.Dispose(); first.Dispose();
            var one = connection.DisposeAsync();
            var two = connection.DisposeAsync();
            Assert.Same(one, two);
            await one;
            Assert.True(first.IsClosed); Assert.True(second.IsClosed);
            Assert.Equal("held", ((Allors.Workspace.IUnitDiff)Assert.Single(created.Strategy.Diff())).ChangedRole);
            Assert.True(first.HasChanges);
            Assert.Throws<ObjectDisposedException>(() => workspace.CreateSession());
            Assert.Throws<ObjectDisposedException>(() => connection.CreateWorkspace());
        }

        [Theory]
        [InlineData(false, "push", false)]
        [InlineData(false, "push", true)]
        [InlineData(false, "invoke", false)]
        [InlineData(false, "invoke", true)]
        [InlineData(true, "push", false)]
        [InlineData(true, "push", true)]
        [InlineData(true, "invoke", false)]
        [InlineData(true, "invoke", true)]
        public async Task NormalConnectionDisposeDrainsKnownAndUnknownMutationOutcomes(bool newtonsoft, string operation, bool lost)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var service = new LifecycleWorkspaceServices();
            var connection = await handler.CreateAsync(client, newtonsoft, () => service);
            var workspace = connection.CreateWorkspace();
            var session = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
            var queuedSession = workspace.CreateSession();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            handler.BeforeMutation = async () => { entered.SetResult(true); await release.Task; if (lost) throw new System.Net.Http.HttpRequestException("Receipt lost after dispatch."); };
            Task<Allors.Workspace.IMutationResult> Mutate(Allors.Workspace.ISession target) => operation == "push" ? Save(target) : Invoke(target);
            var active = Mutate(session);
            await entered.Task;
            var queued = Mutate(queuedSession);
            var close = connection.DisposeAsync();
            Assert.False(close.IsCompleted);
            Assert.Throws<ObjectDisposedException>(() => connection.CreateWorkspace());
            Assert.Throws<ObjectDisposedException>(() => workspace.CreateSession());
            Assert.Equal(0, service.Disposals);
            release.SetResult(true);
            var result = await active;
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => queued);
            await close;
            Assert.Equal(lost ? MutationOutcome.Unknown : MutationOutcome.Committed, result.Outcome);
            Assert.Equal(lost || operation == "push", result.ReconciliationRequired);
            if (!lost) Assert.Equal(MutationReadStatus.Succeeded, result.Refresh.Status);
            Assert.Equal(1, handler.Pushes + handler.Invokes);
            Assert.All(service.Sessions, value => Assert.Equal(1, value.Disposals));
            Assert.Equal(1, service.Disposals);
            Assert.True(session.IsClosed);
            if (!lost)
            {
                handler.BeforeMutation = null;
                var notificationService = new LifecycleWorkspaceServices();
                var notificationConnection = await handler.CreateAsync(client, newtonsoft, () => notificationService);
                var notificationSession = (Allors.Workspace.Adapters.Remote.Session)notificationConnection.CreateWorkspace().CreateSession();
                var type = handler.Meta.FindByName("C1");
                var role = type.RoleTypes.Single(value => value.Name == "C1AllorsString");
                PushReadChanged Changed(string correlation, ReadScope scope)
                {
                    var read = SessionReadResponse(new ReadRequest { i = correlation }, type.Tag, role.RelationType.Tag);
                    return new PushReadChanged { i = correlation, sc = scope, k = "changed", p = read.p, s = read.s, pm = read.pm, u = Array.Empty<long>() };
                }
                handler.OnPush = request => Task.FromResult<object>(new PushReadResponse { i = request.i, sc = request.sc,
                    push = new PushReadOutcome { k = "committed", n = Array.Empty<PushResponseNewObject>() }, changed = Changed(request.i, request.sc) });
                handler.OnInvoke = request => Task.FromResult<object>(new InvokeReadResponse { i = request.i, sc = request.sc,
                    invoke = new InvokeReadOutcome { k = "committed" }, changed = Changed(request.i, request.sc) });
                Task notificationClose = null;
                var notificationFailure = new InvalidOperationException("Notification failed after application.");
                notificationSession.OnChange += (_, _) =>
                {
                    notificationClose = notificationConnection.DisposeAsync();
                    Assert.False(notificationClose.IsCompleted);
                    Assert.Equal(0, notificationService.Disposals);
                    Assert.Equal(0, notificationService.Sessions.Single().Disposals);
                    throw notificationFailure;
                };
                var notified = await Mutate(notificationSession);
                Assert.Equal(MutationOutcome.Committed, notified.Outcome);
                Assert.Equal(MutationReadStatus.Succeeded, notified.Refresh.Status);
                Assert.Equal(MutationRefreshApplicationStatus.Applied, notified.Application.Refresh);
                var error = Assert.Single(notified.Application.Errors);
                Assert.Equal(MutationApplicationStage.Notification, error.Stage);
                Assert.Same(notificationFailure, error.Cause);
                Assert.False(notified.ReconciliationRequired);
                Assert.NotNull(notificationClose);
                await notificationClose;
                Assert.Equal(1, notificationService.Disposals);
                Assert.Equal(1, notificationService.Sessions.Single().Disposals);
                Assert.True(notificationSession.IsClosed);
            }

        }

        [Theory]
        [InlineData("read")]
        [InlineData("push")]
        [InlineData("invoke")]
        public async Task EarlySessionCloseRetainsOwnedServicesUntilConnectionDrain(string operation)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var services = new LifecycleWorkspaceServices();
            var connection = await handler.CreateAsync(client, false, () => services);
            var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (operation == "read") handler.OnRead = async request => { entered.TrySetResult(true); await release.Task; return EmptyBootstrapResponse(request); };
            else handler.BeforeMutation = async () => { entered.TrySetResult(true); await release.Task; };
            Task active = operation == "read" ? session.PullAsync() : operation == "push" ? session.PushAsync() : session.InvokeAsync(Array.Empty<Allors.Workspace.Method>());
            await entered.Task;
            session.Dispose();
            Assert.True(session.IsClosed);
            Assert.Equal(0, services.Sessions.Single().Disposals);
            var close = connection.DisposeAsync();
            Assert.False(close.IsCompleted);
            release.SetResult(true);
            var error = await Record.ExceptionAsync(() => active);
            if (operation == "read") Assert.IsAssignableFrom<InvalidOperationException>(error); else Assert.Null(error);
            await close;
            Assert.Equal(1, services.Sessions.Single().Disposals);
        }

        [Fact]
        public async Task NormalSessionInitializationRunsOnceAfterRegistration()
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var services = new LifecycleWorkspaceServices();
            var connection = await handler.CreateAsync(client, false, () => services);
            Assert.Equal(0, services.Initializations);
            var workspace = connection.CreateWorkspace();
            var session = workspace.CreateSession();
            Assert.Equal(1, services.Initializations);
            Assert.Equal(1, services.Sessions.Single().Initializations);
            Assert.Same(session, services.Sessions.Single().Session);
            await connection.DisposeAsync();
            Assert.Equal(1, services.Sessions.Single().Disposals);
            Assert.True(((Allors.Workspace.Adapters.Remote.Session)session).IsClosed);
        }

        [Theory]
        [InlineData("servicesBuilder", "throw")]
        [InlineData("servicesBuilder", "close")]
        [InlineData("CreateSessionServices", "throw")]
        [InlineData("CreateSessionServices", "close")]
        [InlineData("Workspace.OnInit", "throw")]
        [InlineData("Workspace.OnInit", "close")]
        [InlineData("Session.OnInit", "throw")]
        [InlineData("Session.OnInit", "close")]
        public async Task NormalConstructionCallbacksCannotPublishAfterShutdown(string stage, string action)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var services = new List<LifecycleWorkspaceServices>();
            var enabled = false;
            Task close = null;
            Task callbackOperation = null;
            var operationRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Allors.Workspace.Adapters.Remote.DatabaseConnection connection = null;
            Allors.Workspace.IWorkspace capturedWorkspace = null;
            Allors.Workspace.ISession capturedSession = null;
            var original = new InvalidOperationException("Construction callback failed.");
            var cleanupFailure = new InvalidOperationException("Construction resource cleanup failed.");
            var failCleanup = action == "throw" && (stage == "Workspace.OnInit" || stage == "Session.OnInit");
            void Callback(string current)
            {
                if (!enabled || stage != current) return;
                if (action == "throw") throw original;
                close = connection.DisposeAsync();
                Assert.False(close.IsCompleted);
            }
            connection = await handler.CreateAsync(client, false, () =>
            {
                Callback("servicesBuilder");
                var value = new LifecycleWorkspaceServices();
                value.BeforeSession = () => Callback("CreateSessionServices");
                value.WorkspaceInit = workspace => { capturedWorkspace = workspace; Callback("Workspace.OnInit"); };
                value.SessionInit = session =>
                {
                    capturedSession = session;
                    if (enabled && stage == "Session.OnInit") callbackOperation = session.PullAsync();
                    Callback("Session.OnInit");
                };
                services.Add(value);
                return value;
            });
            var siblingWorkspace = connection.CreateWorkspace();
            var sibling = (Allors.Workspace.Adapters.Remote.Session)siblingWorkspace.CreateSession();
            capturedWorkspace = null; capturedSession = null;
            handler.OnRead = async request => { await operationRelease.Task; return EmptyBootstrapResponse(request); };
            enabled = true;
            var failure = Record.Exception(() =>
            {
                if (stage == "CreateSessionServices" || stage == "Session.OnInit") siblingWorkspace.CreateSession();
                else connection.CreateWorkspace();
            });
            Assert.NotNull(failure);
            if (action == "throw")
            {
                Assert.Same(original, failure);
                Assert.False(sibling.IsClosed);
                if (capturedWorkspace != null) Assert.Throws<InvalidOperationException>(() => capturedWorkspace.CreateSession());
                if (capturedSession != null) Assert.True(((Allors.Workspace.Adapters.Remote.Session)capturedSession).IsClosed);
                if (failCleanup) services.Last().DisposeAction = () => throw cleanupFailure;
                enabled = false;
                Assert.NotNull(siblingWorkspace.CreateSession());
                close = connection.DisposeAsync();
            }
            if (callbackOperation != null)
            {
                Assert.False(callbackOperation.IsCompleted);
                Assert.False(close.IsCompleted);
                Assert.All(services, value => Assert.Equal(0, value.Disposals));
                operationRelease.SetResult(true);
                await Assert.ThrowsAnyAsync<InvalidOperationException>(() => callbackOperation);
            }
            var cleanup = await Record.ExceptionAsync(() => close);
            if (failCleanup)
            {
                Assert.Same(cleanupFailure, Assert.Single(Assert.IsType<AggregateException>(cleanup).InnerExceptions));
                Assert.Same(original, failure);
            }
            else Assert.Null(cleanup);
            Assert.True(sibling.IsClosed);
            Assert.All(services, value => { Assert.Equal(1, value.Disposals); Assert.All(value.Sessions, session => Assert.Equal(1, session.Disposals)); });
        }

        [Theory]
        [InlineData("workspace")]
        [InlineData("session")]
        public async Task NormalConnectionCleanupContinuesAfterServiceDisposalFailure(string failing)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var services = new List<LifecycleWorkspaceServices>();
            var order = new List<string>();
            var connection = await handler.CreateAsync(client, false, () =>
            {
                var value = new LifecycleWorkspaceServices { DisposeAction = () => { order.Add("workspace"); if (failing == "workspace") throw new InvalidOperationException("workspace cleanup"); },
                    SessionDisposeAction = () => { order.Add("session"); if (failing == "session") throw new InvalidOperationException("session cleanup"); } };
                services.Add(value); return value;
            });
            connection.CreateWorkspace().CreateSession();
            connection.CreateWorkspace().CreateSession();
            var close = connection.DisposeAsync();
            Assert.Same(close, connection.DisposeAsync());
            var error = await Assert.ThrowsAsync<AggregateException>(() => close);
            Assert.Equal(2, error.InnerExceptions.Count);
            Assert.Equal(new[] { "session", "session", "workspace", "workspace" }, order);
            Assert.All(services, value => { Assert.Equal(1, value.Disposals); Assert.Equal(1, value.Sessions.Single().Disposals); });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NormalConnectionCloseDistinguishesPendingQueryGapFromCompletion(bool dispatched)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var saved = await fixture.Session.PushAsync(new PushOptions { Discover = new[] { fixture.Query } });
            var before = fixture.Requests;
            if (!dispatched)
            {
                await fixture.Connection.DisposeAsync();
                await Assert.ThrowsAnyAsync<InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
                Assert.Equal(before, fixture.Requests);
                return;
            }
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnRead = async request => { entered.TrySetResult(true); await release.Task; return fixture.Read(request, 3, "complete"); };
            var completion = saved.PendingQuery.CompleteAsync();
            await entered.Task;
            var close = fixture.Connection.DisposeAsync();
            Assert.False(close.IsCompleted);
            release.SetResult(true);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => completion);
            await close;
            Assert.Equal(before + 1, fixture.Requests);
            Assert.True(fixture.Session.IsClosed);
            var owned = new LifecycleWorkspaceServices();
            await using var notifying = await PushReadClientFixture.CreateAsync(services: () => owned);
            await notifying.Bind();
            var savedAgain = await notifying.Session.PushAsync(new PushOptions { Discover = new[] { notifying.Query } });
            notifying.OnRead = request => Task.FromResult<object>(notifying.Read(request, 3, "completed"));
            Task notificationClose = null;
            notifying.Session.OnChange += (_, _) =>
            {
                notificationClose = notifying.Connection.DisposeAsync();
                Assert.False(notificationClose.IsCompleted);
                Assert.Equal(0, owned.Disposals);
                Assert.Equal(0, owned.Sessions.Single().Disposals);
            };
            var completed = await savedAgain.PendingQuery.CompleteAsync();
            Assert.False(completed.HasErrors);
            Assert.NotNull(notificationClose);
            await notificationClose;
            Assert.True(notifying.Session.IsClosed);
            Assert.Equal(1, owned.Disposals);
            Assert.Equal(1, owned.Sessions.Single().Disposals);
            Assert.Equal(0, owned.AccessAfterDisposal + owned.Sessions.Single().AccessAfterDisposal);

        }

        [Theory]
        [InlineData("read")]
        [InlineData("push")]
        [InlineData("invoke")]
        [InlineData("pending-query")]
        public async Task NormalQueuedOperationsCannotTouchResourcesAfterClose(string operation)
        {
            var owned = new List<LifecycleWorkspaceServices>();
            await using var fixture = await PushReadClientFixture.CreateAsync(services: () =>
            {
                var services = new LifecycleWorkspaceServices();
                owned.Add(services);
                return services;
            });
            await fixture.Bind();
            var saved = await fixture.Session.PushAsync(new PushOptions { Discover = new[] { fixture.Query } });
            var sibling = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnRead = async request => { entered.TrySetResult(true); await release.Task; return request.k == "discover" ? DiscoveryResponse(fixture.Read(request)) : (object)fixture.Read(request); };
            var active = sibling.PullAsync(fixture.Query);
            await entered.Task;
            var before = fixture.Requests;
            Task queued = operation == "read" ? fixture.Session.PullAsync(fixture.Query) : operation == "push" ? fixture.Session.PushAsync() : operation == "invoke" ? fixture.Session.InvokeAsync(Array.Empty<Allors.Workspace.Method>()) : saved.PendingQuery.CompleteAsync();
            var close = fixture.Connection.DisposeAsync();
            release.SetResult(true);
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => active);
            await close;
            await Assert.ThrowsAnyAsync<InvalidOperationException>(() => queued);
            Assert.Equal(before, fixture.Requests);
            Assert.True(fixture.Session.IsClosed);
            Assert.All(owned, service =>
            {
                Assert.Equal(1, service.Disposals);
                Assert.Equal(0, service.AccessAfterDisposal);
                Assert.All(service.Sessions, session =>
                {
                    Assert.Equal(1, session.Disposals);
                    Assert.Equal(0, session.AccessAfterDisposal);
                });
            });
            if (operation == "push")
            {
                using var handler = new LifecycleHttpHandler();
                using var client = handler.CreateClient();
                var captureServices = new LifecycleWorkspaceServices();
                var push = new LifecyclePushTransport(new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client));
                var connection = await FactoryTestConnection.CreateAsync(handler.Configuration(), () => captureServices,
                    new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), push,
                    new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client));
                var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
                Task captureClose = null;
                push.OnCapture = count =>
                {
                    if (count != 2) return;
                    captureClose = connection.DisposeAsync();
                    Assert.False(captureClose.IsCompleted);
                    Assert.Equal(0, captureServices.Disposals);
                };
                var captureFailure = await Record.ExceptionAsync(() => session.PushAsync());
                Assert.NotNull(captureClose);
                await captureClose;
                Assert.Equal(0, handler.Pushes);
                Assert.IsAssignableFrom<InvalidOperationException>(captureFailure);
                Assert.False(session.ReconciliationRequired);
                Assert.Equal(1, captureServices.Disposals);
                Assert.Equal(0, captureServices.AccessAfterDisposal);
            }
        }

        [Fact]
        public async Task NormalLocalProfileLoginDrainsAllOwnedConnections()
        {
            await this.Profile.Login("administrator");
            var oldWorkspace = this.Profile.CreateWorkspace();
            var old = (Allors.Workspace.Adapters.Remote.Session)oldWorkspace.CreateSession();
            var independent = (Allors.Workspace.Adapters.Remote.Session)(await this.Profile.CreateExclusiveWorkspaceAsync()).CreateSession();
            var created = old.Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "retained across Login";
            var database = ((Allors.Workspace.Adapters.Local.DatabaseConnection)old.Workspace.DatabaseConnection).Database;
            await this.Profile.Login("administrator");
            Assert.True(old.IsClosed); Assert.True(independent.IsClosed);
            Assert.Equal("retained across Login", ((Allors.Workspace.IUnitDiff)Assert.Single(created.Strategy.Diff())).ChangedRole);
            Assert.Throws<ObjectDisposedException>(() => oldWorkspace.CreateSession());
            var fresh = (Allors.Workspace.Adapters.Remote.Session)this.Profile.CreateWorkspace().CreateSession();
            Assert.False(fresh.HasChanges);
            Assert.Same(database, ((Allors.Workspace.Adapters.Local.DatabaseConnection)fresh.Workspace.DatabaseConnection).Database);
            Assert.Equal(MutationOutcome.Committed, (await fresh.PushAsync()).Outcome);
        }

        private static async Task<Allors.Workspace.IMutationResult> Save(Allors.Workspace.ISession session) => await session.PushAsync();
        private static async Task<Allors.Workspace.IMutationResult> Invoke(Allors.Workspace.ISession session) => await session.InvokeAsync(Array.Empty<Allors.Workspace.Method>());

        private sealed class LifecycleRule : Allors.Workspace.Derivations.IRule
        {
            public Allors.Workspace.Meta.IComposite ObjectType => null;
            public Allors.Workspace.Meta.IRoleType RoleType => null;
            public IEnumerable<Allors.Workspace.Meta.IDependency> Dependencies => Array.Empty<Allors.Workspace.Meta.IDependency>();
            public object Derive(Allors.Workspace.IObject match) => null;
        }

        private sealed class LifecycleWorkspaceServices : Allors.Workspace.IWorkspaceServices
        {
            private readonly Allors.Workspace.WorkspaceServices inner = new Allors.Workspace.WorkspaceServices();
            internal int Initializations;
            internal int Disposals;
            internal int AccessAfterDisposal;
            internal Action BeforeSession;
            internal Action<Allors.Workspace.IWorkspace> WorkspaceInit;
            internal Action<Allors.Workspace.ISession> SessionInit;
            internal Action DisposeAction;
            internal Action SessionDisposeAction;
            internal readonly List<LifecycleSessionServices> Sessions = new List<LifecycleSessionServices>();
            public void OnInit(Allors.Workspace.IWorkspace workspace) { this.RequireAlive(); ++this.Initializations; this.inner.OnInit(workspace); this.WorkspaceInit?.Invoke(workspace); }
            public Allors.Workspace.ISessionServices CreateSessionServices()
            {
                this.RequireAlive();
                this.BeforeSession?.Invoke();
                var service = new LifecycleSessionServices { Init = this.SessionInit, DisposeAction = this.SessionDisposeAction };
                this.Sessions.Add(service); return service;
            }
            public T Get<T>() { this.RequireAlive(); return this.inner.Get<T>(); }
            private void RequireAlive()
            {
                if (this.Disposals == 0) return;
                ++this.AccessAfterDisposal;
                throw new ObjectDisposedException(this.GetType().Name);
            }
            public void Dispose() { ++this.Disposals; this.inner.Dispose(); this.DisposeAction?.Invoke(); }
        }

        private sealed class LifecycleSessionServices : Allors.Workspace.ISessionServices
        {
            private readonly Allors.Workspace.SessionServices inner = new Allors.Workspace.SessionServices();
            internal int Initializations;
            internal int Disposals;
            internal int AccessAfterDisposal;
            internal Allors.Workspace.ISession Session;
            internal Action<Allors.Workspace.ISession> Init;
            internal Action DisposeAction;
            public void OnInit(Allors.Workspace.ISession session) { this.RequireAlive(); ++this.Initializations; this.Session = session; this.inner.OnInit(session); this.Init?.Invoke(session); }
            public T Get<T>() { this.RequireAlive(); return this.inner.Get<T>(); }
            private void RequireAlive()
            {
                if (this.Disposals == 0) return;
                ++this.AccessAfterDisposal;
                throw new ObjectDisposedException(this.GetType().Name);
            }
            public void Dispose() { ++this.Disposals; this.inner.Dispose(); this.DisposeAction?.Invoke(); }
        }

        private sealed class LifecyclePushTransport : Allors.Workspace.Adapters.Remote.IPushReadTransport
        {
            private readonly Allors.Workspace.Adapters.Remote.IPushReadTransport inner;
            private int captures;
            internal LifecyclePushTransport(Allors.Workspace.Adapters.Remote.IPushReadTransport inner) => this.inner = inner;
            internal Action<int> OnCapture;
            public PushReadRequest CaptureRequest(PushReadRequest request)
            {
                var captured = this.inner.CaptureRequest(request);
                this.OnCapture?.Invoke(++this.captures);
                return captured;
            }
            public Task<string> Send(PushReadRequest request, System.Threading.CancellationToken cancellationToken = default) => this.inner.Send(request, cancellationToken);
            public PushReadReceipt CaptureReceipt(string response, PushReadRequest request) => this.inner.CaptureReceipt(response, request);
            public object CapturePart(string response, PushReadRequest request, bool changed) => this.inner.CapturePart(response, request, changed);
        }

        private sealed class LifecycleHttpHandler : System.Net.Http.HttpMessageHandler
        {
            internal readonly Allors.Workspace.Meta.IMetaPopulation Meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            internal readonly List<ReadRequest> Reads = new List<ReadRequest>();
            internal int Pushes;
            internal int Invokes;
            internal Func<ReadRequest, Task<object>> OnRead;
            internal Func<Task> BeforeMutation;
            internal Func<PushReadRequest, Task<object>> OnPush;
            internal Func<InvokeReadRequest, Task<object>> OnInvoke;
            internal System.Net.Http.HttpClient CreateClient() => new System.Net.Http.HttpClient(this, false) { BaseAddress = new Uri("http://localhost/allors/") };
            internal Allors.Workspace.Adapters.Remote.Configuration Configuration(Allors.Workspace.Derivations.IRule[] rules = null, Allors.Workspace.Adapters.ReflectionObjectFactory factory = null) =>
                new Allors.Workspace.Adapters.Remote.Configuration("Default", this.Meta, factory ?? new ScopedFactory(this.Meta), rules ?? Array.Empty<Allors.Workspace.Derivations.IRule>());
            internal async Task<Allors.Workspace.Adapters.Remote.DatabaseConnection> CreateAsync(System.Net.Http.HttpClient client, bool newtonsoft,
                Func<Allors.Workspace.IWorkspaceServices> services = null, Allors.Workspace.Adapters.Remote.Configuration configuration = null, string context = null,
                System.Threading.CancellationToken cancellationToken = default) => newtonsoft
                ? await Allors.Workspace.Adapters.Remote.Newtonsoft.DatabaseConnection.CreateAsync(configuration ?? this.Configuration(), services ?? (() => new LifecycleWorkspaceServices()), client,
                    new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "v1", context, cancellationToken)
                : await Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection.CreateAsync(configuration ?? this.Configuration(), services ?? (() => new LifecycleWorkspaceServices()), client,
                    new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "v1", context, cancellationToken);
            protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage message, System.Threading.CancellationToken cancellationToken)
            {
                if (message.Method == System.Net.Http.HttpMethod.Get) return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);
                var raw = await message.Content.ReadAsStringAsync(cancellationToken);
                object result;
                if (message.RequestUri.AbsolutePath.EndsWith("push-read", StringComparison.Ordinal))
                {
                    ++this.Pushes;
                    var request = Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(raw);
                    if (this.BeforeMutation != null) await this.BeforeMutation();
                    result = this.OnPush != null ? await this.OnPush(request) : new PushReadResponse { i = request.i, sc = request.sc, push = new PushReadOutcome { k = "committed", n = Array.Empty<PushResponseNewObject>() },
                        changed = request.returnChanged == true ? EmptyChanged(request.i, request.sc) : null };
                }
                else if (message.RequestUri.AbsolutePath.EndsWith("invoke-read", StringComparison.Ordinal))
                {
                    ++this.Invokes;
                    var request = InvokeReadJson.DeserializeRequest(raw);
                    if (this.BeforeMutation != null) await this.BeforeMutation();
                    result = this.OnInvoke != null ? await this.OnInvoke(request) : new InvokeReadResponse { i = request.i, sc = request.sc, invoke = new InvokeReadOutcome { k = "committed" }, changed = request.refresh ? EmptyChanged(request.i, request.sc) : null };
                }
                else
                {
                    var request = ReadJson.DeserializeRequest(raw);
                    this.Reads.Add(request);
                    result = this.OnRead == null ? EmptyBootstrapResponse(request) : await this.OnRead(request);
                }
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new System.Net.Http.StringContent(result is string text ? text : JsonSerializer.Serialize(result)) };
            }
            private static PushReadChanged EmptyChanged(string correlation, ReadScope scope) => new PushReadChanged
            { i = correlation, sc = scope, k = "changed", p = Array.Empty<ReadRecord>(), s = Array.Empty<ReadSecurity>(), pm = Array.Empty<ReadPermission>(), u = Array.Empty<long>() };
        }

        [Fact]
        public void ProtectedInvokePublicContractUsesSharedResultAndRefreshOnly()
        {
            Assert.True(typeof(Allors.Workspace.IMutationResult).IsAssignableFrom(typeof(Allors.Workspace.IInvokeResult)));
            Assert.True(typeof(Allors.Workspace.IResult).IsAssignableFrom(typeof(Allors.Workspace.IInvokeResult)));
            var options = typeof(Allors.Workspace.InvokeOptions);
            Assert.Equal(new[] { "Refresh" }, options.GetProperties().Select(v => v.Name));
            Assert.Equal(true, options.GetProperty("Refresh").GetValue(new Allors.Workspace.InvokeOptions()));
            foreach (var type in new[] { typeof(Allors.Workspace.ISession), typeof(Allors.Workspace.Adapters.Session), typeof(Allors.Workspace.Adapters.Remote.Session) })
            {
                foreach (var target in new[] { typeof(Allors.Workspace.Method), typeof(Allors.Workspace.Method[]) })
                {
                    Assert.NotNull(type.GetMethod("InvokeAsync", new[] { target, options }));
                    Assert.NotNull(type.GetMethod("InvokeAsync", new[] { target, options, typeof(System.Threading.CancellationToken) }));
                }
                Assert.DoesNotContain(type.GetMethods(), method => method.Name.Contains("InvokeRead"));
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ProtectedInvokeDefaultsToRefreshAndExplicitSkipPreservesEdits(bool newtonsoft, bool skip)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var target = fixture.Item;
            target.Strategy.SetUnitRole(fixture.Role, "local edit");
            fixture.Session.Checkpoint();
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "local creation";
            var id = created.Id;
            fixture.OnInvoke = request =>
            {
                Assert.Equal(!skip, request.refresh);
                Assert.Equal(101, Assert.Single(request.l).i);
                Assert.Equal(1, request.l[0].v);
                return Task.FromResult<object>(fixture.InvokeResponse(request));
            };
            var result = skip ? await fixture.Session.InvokeAsync(fixture.Method, new Allors.Workspace.InvokeOptions { Refresh = false }) : await fixture.Session.InvokeAsync(fixture.Method);
            Assert.Equal(MutationOutcome.Committed, result.Outcome);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, result.Application.Acknowledgement);
            Assert.Equal(skip ? MutationReadStatus.NotRequested : MutationReadStatus.Succeeded, result.Refresh.Status);
            Assert.Equal(skip ? 1 : 2, ((Allors.Workspace.Adapters.Strategy)target.Strategy).DatabaseOriginState.Version);
            Assert.Equal("local edit", target.Strategy.GetUnitRole(fixture.Role));
            Assert.Equal(id, created.Id);
            Assert.Contains((Allors.Workspace.Adapters.Strategy)created.Strategy, fixture.Session.PushToDatabaseTracker.Created);
            Assert.Equal("local creation", created.C1AllorsString);
            Assert.True(target.Strategy.CanWrite(fixture.Role));
            Assert.Equal(1, fixture.Invokes);
            Assert.Equal(0, fixture.Pushes);
            Assert.Equal(3, fixture.Requests);
            fixture.Session.Reset();
            Assert.Equal("local edit", target.Strategy.GetUnitRole(fixture.Role));
            target.Strategy.Reset();
            Assert.Equal("value", target.Strategy.GetUnitRole(fixture.Role));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ProtectedInvokeRefreshPreservesEditsAndCheckpointOrReportsConflict(bool newtonsoft, bool conflict)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var held = fixture.Item;
            held.Strategy.SetUnitRole(fixture.Role, "local");
            fixture.Session.Checkpoint();
            fixture.OnInvoke = request => Task.FromResult<object>(fixture.InvokeResponse(request, conflict ? "remote" : "value"));
            var result = await fixture.Session.InvokeAsync(fixture.Method);
            Assert.Equal(MutationOutcome.Committed, result.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, result.Refresh.Status);
            Assert.Equal(conflict ? MutationRefreshApplicationStatus.Conflicted : MutationRefreshApplicationStatus.Applied, result.Application.Refresh);
            Assert.Equal(conflict, result.HasErrors);
            Assert.Equal(conflict ? 1 : 2, ((Allors.Workspace.Adapters.Strategy)held.Strategy).DatabaseOriginState.Version);
            Assert.Equal("local", held.Strategy.GetUnitRole(fixture.Role));
            Assert.Equal("value", Assert.IsAssignableFrom<Allors.Workspace.IUnitDiff>(Assert.Single(held.Strategy.Diff())).OriginalRole);
            Assert.Same(held, fixture.Item);
            if (conflict) Assert.Same(held, Assert.Single(result.Refresh.Result.MergeErrors));
            else Assert.Empty(result.Refresh.Result.MergeErrors);
            Assert.False(result.ReconciliationRequired);
            fixture.OnInvoke = request =>
            {
                var response = fixture.InvokeResponse(request, conflict ? "remote" : "value");
                var changed = (PushReadChanged)response.changed;
                changed.s[0].v += 1;
                changed.s[0].p.p = new long[] { 203 };
                changed.pm = changed.pm.Where(v => v.i == 203).ToArray();
                return Task.FromResult<object>(response);
            };
            var revoked = await fixture.Session.InvokeAsync(fixture.Method);
            Assert.Equal(MutationOutcome.Committed, revoked.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, revoked.Refresh.Status);
            Assert.False(held.Strategy.CanRead(fixture.Role));
            Assert.False(held.Strategy.CanWrite(fixture.Role));
            Assert.Equal("local", Assert.IsAssignableFrom<Allors.Workspace.IUnitDiff>(Assert.Single(held.Strategy.Diff())).ChangedRole);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedInvokeOwnsMethodsOptionsAndRawDiagnostics(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft, directInvoke: true);
            await fixture.Bind();
            using var sibling = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnRead = async request => { entered.TrySetResult(true); await release.Task; var read = fixture.Read(request); return request.k == "discover" ? DiscoveryResponse(read) : (object)read; };
            var blocking = sibling.PullAsync(fixture.Query);
            await entered.Task;
            var methods = new[] { fixture.Method, fixture.Method };
            var options = new Allors.Workspace.InvokeOptions { Refresh = false };
            InvokeReadResponse response = null;
            fixture.OnInvoke = request =>
            {
                Assert.False(request.refresh);
                Assert.Equal(new long[] { 101, 101 }, request.l.Select(v => v.i));
                Assert.All(request.l, item => Assert.Equal(1, item.v));
                response = new InvokeReadResponse { i = request.i, sc = request.sc, invoke = new InvokeReadOutcome
                { k = "rejected", _e = "Correct the methods.", e = new[] { new ReadError { c = "declared", m = "Review the method.", o = new long[] { 101, 996 } } },
                    _v = new long[] { 101, 998 }, _a = new long[] { 999 }, _m = new long[] { 997 },
                    _d = new[] { new Allors.Protocol.Json.Api.ResponseDerivationError { m = null, r = new[] { new Allors.Protocol.Json.Api.DerivationRelation { i = -5, r = "unknown-relation" } } }, new Allors.Protocol.Json.Api.ResponseDerivationError { m = string.Empty, r = Array.Empty<Allors.Protocol.Json.Api.DerivationRelation>() } } } };
                var raw = JsonSerializer.Serialize(response);
                request.l[0].i = 9999;
                request.sc.p = "host-mutated";
                return Task.FromResult<object>(raw);
            };
            var invoking = fixture.Session.InvokeAsync(methods, options);
            methods[0] = default;
            options.Refresh = true;
            release.SetResult(true);
            await blocking;
            var result = await invoking;
            response.invoke._v[0] = 9999;
            response.invoke._d[0].r[0].r = "changed";
            fixture.Session.Dispose();
            fixture.Factory.OnCreate = () => throw new InvalidOperationException("Diagnostics must not instantiate objects.");
            Assert.Equal(MutationOutcome.Rejected, result.Outcome);
            Assert.Equal(new long[] { 101, 998 }, result.Diagnostics.VersionErrorIds);
            Assert.Equal(new long[] { 999 }, result.Diagnostics.AccessErrorIds);
            Assert.Equal(new long[] { 997 }, result.Diagnostics.MissingErrorIds);
            Assert.Equal("Correct the methods.", result.Diagnostics.ErrorMessage);
            Assert.Equal("declared", Assert.Single(result.Diagnostics.Errors).Code);
            Assert.Equal(2, result.Diagnostics.DerivationErrors.Count);
            Assert.Equal(string.Empty, result.Diagnostics.DerivationErrors[1].Message);
            var derivation = result.Diagnostics.DerivationErrors[0];
            Assert.Null(derivation.Message);
            Assert.Equal(-5, Assert.Single(derivation.Relations).ObjectId);
            Assert.Equal("unknown-relation", derivation.Relations[0].RelationTypeTag);
            Assert.Single(result.VersionErrors);
            Assert.Empty(result.AccessErrors);
            Assert.Empty(result.MissingErrors);
            Assert.Throws<NotSupportedException>(() => ((IList<long>)result.Diagnostics.VersionErrorIds)[0] = 3);
            Assert.False(result.ReconciliationRequired);
            Assert.Equal(1, fixture.Invokes);
        }

        [Theory]
        [InlineData(false, "null-methods")]
        [InlineData(false, "default-method")]
        [InlineData(false, "null-method-type")]
        [InlineData(false, "new-target")]
        [InlineData(false, "foreign-method")]
        [InlineData(true, "null-methods")]
        [InlineData(true, "default-method")]
        [InlineData(true, "null-method-type")]
        [InlineData(true, "new-target")]
        [InlineData(true, "foreign-method")]
        public async Task ProtectedInvokeRejectsInvalidTargetsBeforeDispatch(bool newtonsoft, string invalid)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var methods = new[] { fixture.Method };
            if (invalid == "null-methods") methods = null;
            if (invalid == "default-method") methods[0] = default;
            if (invalid == "null-method-type") methods[0] = new Allors.Workspace.Method(fixture.Item, null);
            if (invalid == "new-target") methods[0] = new Allors.Workspace.Method(fixture.Session.Create<Allors.Workspace.Domain.C1>(), fixture.Method.MethodType);
            if (invalid == "foreign-method") methods[0] = new Allors.Workspace.Method(fixture.Item, fixture.Meta.MethodTypes.First(v => !fixture.Type.MethodTypes.Contains(v)));
            var error = await Assert.ThrowsAsync<ArgumentException>(() => fixture.Session.InvokeAsync(methods));
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
            Assert.Equal(0, fixture.Invokes);
            Assert.False(fixture.Session.ReconciliationRequired);
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "editable");
            Assert.Equal("editable", fixture.Item.Strategy.GetUnitRole(fixture.Role));
        }

        [Theory]
        [InlineData(false, "missing-capability")]
        [InlineData(false, "cold")]
        [InlineData(true, "missing-capability")]
        [InlineData(true, "cold")]
        public async Task NormalFactoryRequiresInvokeCapabilityAndReadiness(bool newtonsoft, string invalid)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var services = 0;
            FactoryTestConnection captured = null;
            handler.OnRead = async request => { entered.TrySetResult(true); await release.Task; return EmptyBootstrapResponse(request); };
            var creating = FactoryTestConnection.CreateAsync(handler.Configuration(), () => { ++services; return new LifecycleWorkspaceServices(); },
                newtonsoft ? new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(client) : new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client),
                new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client),
                invalid == "missing-capability" ? null : new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client), newtonsoft, capture: value => captured = value);
            if (invalid == "missing-capability")
            {
                await Assert.ThrowsAsync<ArgumentException>(() => creating);
                Assert.Empty(handler.Reads);
            }
            else
            {
                await entered.Task;
                Assert.False(creating.IsCompleted);
                Assert.Throws<InvalidOperationException>(() => captured.CreateWorkspace());
                Assert.Equal(0, services);
                release.SetResult(true);
                await using var ready = await creating;
                Assert.Equal(MutationOutcome.Committed, (await ready.CreateWorkspace().CreateSession().InvokeAsync(Array.Empty<Allors.Workspace.Method>())).Outcome);
            }
            Assert.Equal(invalid == "cold" ? 1 : 0, handler.Invokes);
            Assert.Equal(0, handler.Pushes);
            Assert.True((await client.GetAsync("borrowed")).IsSuccessStatusCode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedInvokePausesEditingAndSameSessionOperations(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnInvoke = async request =>
            {
                entered.SetResult(true);
                await release.Task;
                var response = fixture.InvokeResponse(request);
                var extra = fixture.Read(new ReadRequest { i = request.i }).p[0];
                extra.i = 501;
                ((PushReadChanged)response.changed).p = ((PushReadChanged)response.changed).p.Concat(new[] { extra }).ToArray();
                return response;
            };
            var invoking = fixture.Session.InvokeAsync(fixture.Method);
            await entered.Task;
            Assert.Throws<InvalidOperationException>(() => fixture.Item.Strategy.SetUnitRole(fixture.Role, "blocked"));
            Assert.Throws<InvalidOperationException>(() => fixture.Session.Create<Allors.Workspace.Domain.C1>());
            Assert.Throws<InvalidOperationException>(() => fixture.Session.Reset());
            Assert.Throws<InvalidOperationException>(() => fixture.Session.Checkpoint());
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PullAsync(fixture.Query));
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PushAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.InvokeAsync(fixture.Method));
            var strategy = (Allors.Workspace.Adapters.Strategy)fixture.Item.Strategy;
            Assert.Throws<InvalidOperationException>(() => strategy.DatabaseOriginState.SetUnitRole(fixture.Role, "direct"));
            var sessionUnit = fixture.Type.RoleTypes.First(v => v.Origin == Origin.Session && v.ObjectType.IsUnit);
            Assert.Throws<InvalidOperationException>(() => fixture.Session.SessionOriginState.SetUnitRole(strategy, sessionUnit, null));
            Assert.Equal("value", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            using var sibling = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            var read = sibling.PullAsync(fixture.Query);
            Assert.False(read.IsCompleted);
            var factoryCalled = false;
            fixture.Factory.OnCreate = () => { factoryCalled = true; Assert.Throws<InvalidOperationException>(() => fixture.Item.Strategy.SetUnitRole(fixture.Role, "factory")); };
            var notified = false;
            EventHandler callback = (_, _) => { notified = true; Assert.Throws<InvalidOperationException>(() => fixture.Item.Strategy.SetUnitRole(fixture.Role, "callback")); };
            fixture.Session.OnChange += callback;
            release.SetResult(true);
            Assert.Equal(MutationOutcome.Committed, (await invoking).Outcome);
            Assert.True(notified);
            Assert.True(factoryCalled);
            fixture.Factory.OnCreate = null;
            fixture.Session.OnChange -= callback;
            Assert.False((await read).HasErrors);
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "editable again");
            Assert.Equal(1, fixture.Invokes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedInvokeQueuedCancellationReleasesPauseWithoutDispatch(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            using var sibling = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnRead = async request => { entered.TrySetResult(true); await release.Task; var response = fixture.Read(request); return request.k == "discover" ? DiscoveryResponse(response) : (object)response; };
            var read = sibling.PullAsync(fixture.Query);
            await entered.Task;
            using var cancellation = new System.Threading.CancellationTokenSource();
            var invoking = fixture.Session.InvokeAsync(fixture.Method, null, cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => invoking);
            Assert.Equal(0, fixture.Invokes);
            Assert.False(fixture.Session.ReconciliationRequired);
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "editable");
            release.SetResult(true);
            await read;
            Assert.Equal(MutationOutcome.Committed, (await fixture.Session.InvokeAsync(fixture.Method)).Outcome);
            Assert.Equal(1, fixture.Invokes);
        }

        [Theory]
        [InlineData(false, "malformed-refresh")]
        [InlineData(false, "missing-refresh")]
        [InlineData(false, "server-read-error")]
        [InlineData(false, "wrong-refresh-scope")]
        [InlineData(true, "malformed-refresh")]
        [InlineData(true, "missing-refresh")]
        [InlineData(true, "server-read-error")]
        [InlineData(true, "wrong-refresh-scope")]
        public async Task ProtectedInvokeClassifiesIndependentReceiptAndRefresh(bool newtonsoft, string invalid)
        {
            foreach (var changedScope in invalid == "server-read-error" ? new[] { false, true } : new[] { false })
            {
                await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
                await fixture.Bind();
                fixture.OnInvoke = request =>
                {
                    var response = fixture.InvokeResponse(request);
                    if (invalid == "malformed-refresh") response.changed = new { broken = true };
                    if (invalid == "missing-refresh") response.changed = null;
                    if (invalid == "wrong-refresh-scope") ((PushReadChanged)response.changed).sc = new ReadScope { p = "other", u = request.sc.u, w = request.sc.w, r = request.sc.r };
                    if (invalid == "server-read-error") response.changed = new ReadFailure { i = request.i, k = "error", sc = changedScope ? new ReadScope { p = "other", u = request.sc.u, w = request.sc.w, r = request.sc.r } : request.sc, e = new[] { new ReadError { c = "read-failed", m = "Read the affected objects again." } } };
                    return Task.FromResult<object>(response);
                };
                var result = await fixture.Session.InvokeAsync(fixture.Method);
                Assert.Equal(MutationOutcome.Committed, result.Outcome);
                Assert.Equal(MutationReadStatus.Failed, result.Refresh.Status);
                Assert.Equal(MutationRefreshApplicationStatus.NotApplicable, result.Application.Refresh);
                Assert.False(result.ReconciliationRequired);
                Assert.Equal("value", ((Allors.Workspace.Adapters.Strategy)fixture.Item.Strategy).DatabaseOriginState.GetUnitRole(fixture.Role));
                Assert.Equal(1, ((Allors.Workspace.Adapters.Strategy)fixture.Item.Strategy).DatabaseOriginState.Version);
                if (changedScope) await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PullAsync());
                else
                {
                    fixture.OnInvoke = request => Task.FromResult<object>(fixture.InvokeResponse(request));
                    Assert.Equal(MutationOutcome.Committed, (await fixture.Session.InvokeAsync(fixture.Method, new Allors.Workspace.InvokeOptions { Refresh = false })).Outcome);
                    Assert.Equal(2, fixture.Invokes);
                }
            }
        }

        [Theory]
        [InlineData(false, "wrong-correlation")]
        [InlineData(false, "wrong-commit-scope")]
        [InlineData(true, "wrong-correlation")]
        [InlineData(true, "wrong-commit-scope")]
        public async Task ProtectedInvokeUntrustedReceiptRequiresReconciliation(bool newtonsoft, string invalid)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            fixture.OnInvoke = request =>
            {
                var response = fixture.InvokeResponse(request);
                if (invalid == "wrong-correlation") response.i = "other";
                else response.sc.p = "other";
                return Task.FromResult<object>(response);
            };
            var result = await fixture.Session.InvokeAsync(fixture.Method);
            Assert.Equal(MutationOutcome.Unknown, result.Outcome);
            Assert.True(result.ReconciliationRequired);
            Assert.NotNull(Assert.Single(result.Diagnostics.Errors).Cause);
            Assert.Equal("value", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.InvokeAsync(fixture.Method));
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PushAsync());
            Assert.Equal(1, fixture.Invokes);
        }

        [Theory]
        [InlineData(false, "cancellation")]
        [InlineData(false, "session-close")]
        [InlineData(false, "connection-retire")]
        [InlineData(true, "cancellation")]
        [InlineData(true, "session-close")]
        [InlineData(true, "connection-retire")]
        public async Task ProtectedInvokePostDispatchLifecycleKeepsKnownOutcome(bool newtonsoft, string lifecycle)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft, directInvoke: true);
            await fixture.Bind();
            var held = fixture.Item;
            using var cancellation = new System.Threading.CancellationTokenSource();
            fixture.OnInvoke = request =>
            {
                var response = fixture.InvokeResponse(request, "remote");
                if (lifecycle == "cancellation") cancellation.Cancel();
                if (lifecycle == "session-close") fixture.Session.Dispose();
                if (lifecycle == "connection-retire") _ = fixture.Connection.DisposeAsync();
                return Task.FromResult<object>(response);
            };
            var result = await fixture.Session.InvokeAsync(fixture.Method, null, cancellation.Token);
            Assert.Equal(MutationOutcome.Committed, result.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, result.Refresh.Status);
            Assert.Equal(MutationRefreshApplicationStatus.Skipped, result.Application.Refresh);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, result.Application.Acknowledgement);
            Assert.NotNull(Assert.Single(result.Application.Errors).Cause);
            Assert.False(result.ReconciliationRequired);
            Assert.Equal("value", ((Allors.Workspace.Adapters.Strategy)held.Strategy).DatabaseOriginState.GetUnitRole(fixture.Role));
            Assert.Equal(1, fixture.Invokes);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ProtectedInvokeApplicationAndNotificationFailuresRemainDistinct(bool newtonsoft, bool throwFactory)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var error = new InvalidOperationException(throwFactory ? "Factory failure" : "Notification failure");
            if (throwFactory) fixture.Factory.OnCreate = () => throw error;
            else fixture.Session.OnChange += (_, _) => throw error;
            fixture.OnInvoke = request =>
            {
                var response = fixture.InvokeResponse(request, "remote");
                if (throwFactory)
                {
                    var extra = fixture.Read(new ReadRequest { i = request.i }).p[0];
                    extra.i = 501;
                    ((PushReadChanged)response.changed).p = ((PushReadChanged)response.changed).p.Concat(new[] { extra }).ToArray();
                }
                return Task.FromResult<object>(response);
            };
            var result = await fixture.Session.InvokeAsync(fixture.Method);
            Assert.Equal(MutationOutcome.Committed, result.Outcome);
            Assert.Equal(MutationReadStatus.Succeeded, result.Refresh.Status);
            Assert.Equal(throwFactory ? MutationRefreshApplicationStatus.Failed : MutationRefreshApplicationStatus.Applied, result.Application.Refresh);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, result.Application.Acknowledgement);
            Assert.Equal(throwFactory, result.ReconciliationRequired);
            var application = Assert.Single(result.Application.Errors);
            Assert.Equal(throwFactory ? MutationApplicationStage.Refresh : MutationApplicationStage.Notification, application.Stage);
            Assert.Same(error, application.Cause);
            fixture.Factory.OnCreate = null;
            using var sibling = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            Assert.False((await sibling.PullAsync(fixture.Query)).HasErrors);
            sibling.Dispose();
            fixture.Session.Dispose();
            await fixture.Connection.DisposeAsync();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NormalInvokeReadinessAndResultsUseOneProtectedPath(bool newtonsoft)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            handler.OnRead = async request => { entered.TrySetResult(true); await release.Task; return EmptyBootstrapResponse(request); };
            var creating = handler.CreateAsync(client, newtonsoft);
            await entered.Task;
            Assert.False(creating.IsCompleted);
            Assert.Equal(0, handler.Invokes);
            release.SetResult(true);
            await using var connection = await creating;
            var session = connection.CreateWorkspace().CreateSession();
            using var cancellation = new System.Threading.CancellationTokenSource();
            var methods = Array.Empty<Allors.Workspace.Method>();
            var result = await session.InvokeAsync(methods, new Allors.Workspace.InvokeOptions { Refresh = false }, cancellation.Token);
            Assert.Equal(MutationOutcome.Committed, result.Outcome);
            Assert.Equal(MutationReadStatus.NotRequested, result.Refresh.Status);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, result.Application.Acknowledgement);
            Assert.False(result.ReconciliationRequired);
            Assert.Equal(1, handler.Invokes);
            handler.OnInvoke = request => Task.FromResult<object>(new InvokeReadResponse { i = request.i, sc = request.sc,
                invoke = new InvokeReadOutcome { k = "rejected", _e = "Correct the invocation." } });
            var rejected = await session.InvokeAsync(methods);
            Assert.Equal(MutationOutcome.Rejected, rejected.Outcome);
            Assert.True(rejected.HasErrors);
            Assert.Equal("Correct the invocation.", rejected.Diagnostics.ErrorMessage);
            Assert.Equal(MutationAcknowledgementStatus.NotApplicable, rejected.Application.Acknowledgement);
            Assert.Equal(MutationReadStatus.Unavailable, rejected.Refresh.Status);
            Assert.Equal(2, handler.Invokes);
        }

        [Theory]
        [MemberData(nameof(ProtocolUnits))]
        public void UnitConversionWithoutSerialization(string tag, object value)
        {
            var convert = new UnitConvert();
            Assert.Equal(value, convert.UnitFromJson(tag, convert.ToJson(value)));
        }

        [Theory]
        [MemberData(nameof(ProtocolUnits))]
        public void UnitConversionWithSerialization(string tag, object value)
        {
            var convert = new UnitConvert();
            var json = JsonSerializer.SerializeToElement(convert.ToJson(value));
            Assert.Equal(value, convert.UnitFromJson(tag, json));
        }

        [Fact]
        public void PublicContractsDoNotExposeStandaloneResultValues()
        {
            foreach (var type in new[]
            {
                typeof(Allors.Workspace.IPullResult),
                typeof(Allors.Workspace.Adapters.Remote.PullResult),
                typeof(Allors.Workspace.Adapters.Remote.ScopedPullResult),
                typeof(Allors.Workspace.Adapters.Remote.ReadCompletedResult),
            })
            {
                Assert.Null(type.GetProperty("Values"));
                Assert.DoesNotContain(type.GetMethods(), method => method.Name == "GetValue");
            }

            Assert.Null(typeof(Allors.Protocol.Json.Api.Pull.PullResponse).GetProperty("v"));
            Assert.Null(typeof(ReadSuccess).GetProperty("v"));
            Assert.DoesNotContain(typeof(PullResponseBuilder).GetMethods(), method => method.Name == "AddValue");
        }

        [Theory]
        [InlineData("SystemText", "{}")]
        [InlineData("SystemText", "{\"retired\":42}")]
        [InlineData("SystemText", "null")]
        [InlineData("Newtonsoft", "{}")]
        [InlineData("Newtonsoft", "{\"retired\":42}")]
        [InlineData("Newtonsoft", "null")]
        public void LegacyPullResponsesIgnoreRemovedResultValues(string serializer, string removedValues)
        {
            var json = "{\"c\":{\"found\":[4294967297],\"empty\":[]},\"o\":{\"first\":4294967297,\"missing\":0},\"p\":[],\"v\":" + removedValues + "}";
            var response = serializer == "SystemText"
                ? JsonSerializer.Deserialize<Allors.Protocol.Json.Api.Pull.PullResponse>(json)
                : JsonConvert.DeserializeObject<Allors.Protocol.Json.Api.Pull.PullResponse>(json);

            Assert.False(response.HasErrors);
            Assert.Equal(new long[] { 4294967297 }, response.c["found"]);
            Assert.Empty(response.c["empty"]);
            Assert.Equal(4294967297, response.o["first"]);
            Assert.Equal(0, response.o["missing"]);
            Assert.Empty(response.p);
            var serialized = serializer == "SystemText" ? JsonSerializer.Serialize(response) : JsonConvert.SerializeObject(response);
            using var document = JsonDocument.Parse(serialized);
            Assert.False(document.RootElement.TryGetProperty("v", out _));
            Assert.Equal(4294967297, document.RootElement.GetProperty("o").GetProperty("first").GetInt64());
            Assert.Equal(0, document.RootElement.GetProperty("o").GetProperty("missing").GetInt64());
            Assert.Equal(4294967297, Assert.Single(document.RootElement.GetProperty("c").GetProperty("found").EnumerateArray()).GetInt64());
            Assert.Empty(document.RootElement.GetProperty("c").GetProperty("empty").EnumerateArray());
        }

        [Fact]
        public void PublicContractsDoNotExposeNamedWorkspaceRoutes()
        {
            foreach (var type in new[]
            {
                typeof(Allors.Workspace.ISession),
                typeof(Allors.Workspace.Adapters.Session),
                typeof(Allors.Workspace.Adapters.Remote.Session),
            })
            {
                Assert.DoesNotContain(type.GetMethods(), method => method.Name == "CallAsync");
                Assert.NotNull(type.GetMethod("PullAsync", new[] { typeof(Pull[]) }));
                Assert.NotNull(type.GetMethod("InvokeAsync", new[] { typeof(Allors.Workspace.Method), typeof(Allors.Workspace.InvokeOptions) }));
            }

            foreach (var type in new[]
            {
                typeof(Allors.Workspace.Adapters.Remote.DatabaseConnection),
                typeof(Allors.Workspace.Adapters.Local.DatabaseConnection),
                typeof(Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection),
                typeof(Allors.Workspace.Adapters.Remote.Newtonsoft.DatabaseConnection),
            })
            {
                Assert.Null(type.GetMethod("Pull", new[] { typeof(object), typeof(string) }));
                Assert.Null(type.GetMethod("Pull", new[] { typeof(Allors.Protocol.Json.Api.Pull.PullRequest) }));
            }

            Assert.NotNull(typeof(Api).GetMethod("Pull", new[] { typeof(Allors.Protocol.Json.Api.Pull.PullRequest) }));
            Assert.DoesNotContain(typeof(Api).GetMethods(), method => method.Name == "CreatePullResponseBuilder");
        }

        [Fact]
        public void PublicContractsDoNotExposeProcedures()
        {
            Assert.Null(typeof(Allors.Database.Data.Pull).Assembly.GetType("Allors.Database.Data.Procedure"));
            Assert.Null(typeof(Allors.Workspace.Data.Pull).Assembly.GetType("Allors.Workspace.Data.Procedure"));
            Assert.Null(typeof(Allors.Protocol.Json.Data.Pull).Assembly.GetType("Allors.Protocol.Json.Data.Procedure"));

            var databaseWorkspace = typeof(Allors.Database.Services.IPermissions).Assembly;
            foreach (var name in new[] { "IProcedure", "IProcedureContext", "IProcedureInput", "IProcedureOutput", "IProcedures", "ProcedureInput" })
            {
                Assert.Null(databaseWorkspace.GetType("Allors.Database." + name));
            }

            Assert.Null(typeof(Allors.Protocol.Json.Api.Pull.PullRequest).GetProperty("p"));
            var calls = typeof(Allors.Workspace.ISession).GetMethods().Where(v => v.Name == "CallAsync").ToArray();
            Assert.DoesNotContain(calls, v => v.GetParameters().Any(p => p.ParameterType.FullName == "Allors.Workspace.Data.Procedure"));
        }

        [Fact]
        public void ProtectedPushPublicContractHasOneSaveOperation()
        {
            var domain = typeof(Allors.Workspace.ISession).Assembly;
            Assert.NotNull(domain.GetType("Allors.Workspace.PushOptions"));
            Assert.NotNull(typeof(Allors.Workspace.IPushResult).GetProperty("CreationMappings"));
            Assert.NotNull(typeof(Allors.Workspace.ISession).GetProperty("ReconciliationRequired"));
            Assert.DoesNotContain(typeof(Allors.Workspace.Adapters.Remote.Session).GetMethods(), method => method.Name == "PushReadAsync");
            Assert.Null(typeof(Allors.Workspace.Adapters.Remote.Session).Assembly.GetType("Allors.Workspace.Adapters.Remote.PushReadOptions"));
            Assert.Null(typeof(Allors.Workspace.Adapters.Remote.Session).Assembly.GetType("Allors.Workspace.Adapters.Remote.ScopedPushReadResult"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedPushDefaultRefreshesAndExplicitSkipPreservesBaseline(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "submitted");
            fixture.OnPush = request =>
            {
                Assert.True(request.returnChanged);
                return Task.FromResult<object>(fixture.Response(request));
            };
            var saved = await fixture.Session.PushAsync();
            Assert.False(saved.HasErrors);
            Assert.Equal(1, fixture.Pushes);
            Assert.Equal("saved", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            fixture.Session.Reset();
            Assert.Equal("saved", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            Assert.True(fixture.Item.Strategy.CanWrite(fixture.Role));

            await using var skipped = await PushReadClientFixture.CreateAsync(newtonsoft);
            await skipped.Bind();
            skipped.Item.Strategy.SetUnitRole(skipped.Role, "submitted");
            skipped.OnPush = request =>
            {
                Assert.False(request.returnChanged);
                return Task.FromResult<object>(skipped.Response(request));
            };
            var withoutRefresh = await skipped.Session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(MutationReadStatus.NotRequested, withoutRefresh.Refresh.Status);
            Assert.Equal(MutationAcknowledgementStatus.Applied, withoutRefresh.Application.Acknowledgement);
            Assert.False(skipped.Item.Strategy.CanWrite(skipped.Role));
            Assert.Equal(1, skipped.Pushes);
            skipped.Session.Reset();
            Assert.Equal("value", skipped.Item.Strategy.GetUnitRole(skipped.Role));
            await skipped.Session.PullAsync(skipped.Query);
            Assert.Equal("saved", skipped.Item.Strategy.GetUnitRole(skipped.Role));
            Assert.True(skipped.Item.Strategy.CanWrite(skipped.Role));
        }

        [Theory]
        [InlineData(false, "missing")]
        [InlineData(true, "missing")]
        [InlineData(false, "duplicate")]
        [InlineData(true, "duplicate")]
        [InlineData(false, "collision")]
        [InlineData(true, "collision")]
        public async Task ProtectedPushMappingFailuresPreserveAllLocalIdentities(bool newtonsoft, string invalid)
        {
            foreach (var duplicateDatabase in invalid == "duplicate" ? new[] { false, true } : new[] { false })
            {
                await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
                var session = fixture.Session;
                fixture.OnPush = request =>
                {
                    var response = fixture.Response(request);
                    var mappings = (request.push.n ?? Array.Empty<PushRequestNewObject>()).Select((value, index) =>
                        new PushResponseNewObject { w = value.w, d = fixture.Pushes == 1 ? 900 : 901 + index }).ToArray();
                    if (fixture.Pushes > 1)
                    {
                        if (invalid == "missing") mappings = mappings.Take(1).ToArray();
                        if (invalid == "collision") mappings[1].d = 900;
                        if (invalid == "duplicate" && duplicateDatabase) mappings[1].d = mappings[0].d;
                        if (invalid == "duplicate" && !duplicateDatabase) mappings[1].w = mappings[0].w;
                    }
                    response.push.n = mappings;
                    return Task.FromResult<object>(response);
                };
                var held = session.Create<Allors.Workspace.Domain.C1>();
                await session.PushAsync(new PushOptions { Refresh = false });
                Assert.Equal(900, held.Id);
                var first = session.Create<Allors.Workspace.Domain.C1>();
                var second = session.Create<Allors.Workspace.Domain.C1>();
                first.C1AllorsString = "retained";
                var ids = new[] { first.Id, second.Id };
                var result = await session.PushAsync(new PushOptions { Refresh = false });
                Assert.Equal(invalid == "collision" ? MutationOutcome.Committed : MutationOutcome.Unknown, result.Outcome);
                if (invalid == "collision") Assert.Equal(MutationAcknowledgementStatus.Failed, result.Application.Acknowledgement);
                Assert.True(result.ReconciliationRequired);
                Assert.Equal(ids, new[] { first.Id, second.Id });
                Assert.Equal(2, session.PushToDatabaseTracker.Created.Count);
                Assert.Equal("retained", first.C1AllorsString);
                Assert.Same(held, session.GetStrategy(900).Object);
                await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync());
                Assert.Equal(2, fixture.Pushes);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedPushDiagnosticsRemainInspectableAfterClosure(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var held = fixture.Item;
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "retained";
            var createdId = created.Id;
            fixture.OnPush = request => Task.FromResult<object>(new PushReadResponse
            {
                i = request.i, sc = request.sc,
                push = new PushReadOutcome
                {
                    k = "rejected", _e = "Correct the declared values.",
                    e = new[] { new ReadError { c = "declared", m = "Correct the selected object.", o = new[] { createdId, 996L } } },
                    _v = new long[] { 101, 998 }, _a = new[] { createdId, 999L }, _m = new long[] { 997 },
                    _d = new[] { new Allors.Protocol.Json.Api.ResponseDerivationError
                    {
                        m = null, r = new[]
                        {
                            new Allors.Protocol.Json.Api.DerivationRelation { i = createdId, r = fixture.Role.RelationType.Tag },
                            new Allors.Protocol.Json.Api.DerivationRelation { i = 995, r = "unknown-relation" },
                        },
                    } },
                },
            });
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal("retained", created.C1AllorsString);
            fixture.Session.Dispose();
            fixture.Factory.OnCreate = () => throw new InvalidOperationException("Diagnostic inspection must not construct objects.");
            Assert.Equal(MutationOutcome.Rejected, saved.Outcome);
            Assert.True(saved.HasErrors);
            Assert.False(saved.ReconciliationRequired);
            Assert.Equal(1, fixture.Pushes);
            Assert.Equal(createdId, created.Id);
            Assert.Equal("Correct the declared values.", saved.Diagnostics.ErrorMessage);
            var diagnostic = Assert.Single(saved.Diagnostics.Errors);
            Assert.Equal("declared", diagnostic.Code);
            Assert.Equal(new[] { createdId, 996L }, diagnostic.ObjectIds);
            Assert.Null(diagnostic.SelectionIndex);
            Assert.Equal(new long[] { 101, 998 }, saved.Diagnostics.VersionErrorIds);
            Assert.Equal(new[] { createdId, 999L }, saved.Diagnostics.AccessErrorIds);
            Assert.Equal(new long[] { 997 }, saved.Diagnostics.MissingErrorIds);
            Assert.Same(held, Assert.Single(saved.VersionErrors));
            Assert.Same(created, Assert.Single(saved.AccessErrors));
            Assert.Empty(saved.MissingErrors);
            var derivation = Assert.Single(saved.Diagnostics.DerivationErrors);
            Assert.Null(derivation.Message);
            Assert.Equal(new[] { createdId, 995L }, derivation.Relations.Select(v => v.ObjectId));
            Assert.Equal(new[] { fixture.Role.RelationType.Tag, "unknown-relation" }, derivation.Relations.Select(v => v.RelationTypeTag));
            Assert.Single(Assert.Single(saved.DerivationErrors).Roles);
            Assert.Throws<NotSupportedException>(() => ((IList<long>)saved.Diagnostics.VersionErrorIds)[0] = 44);
            Assert.Throws<NotSupportedException>(() => ((IList<Allors.Workspace.MutationDerivationRelation>)derivation.Relations).Clear());

            foreach (var outcome in new[] { "rejected", "unknown" })
            {
                await using var textless = await PushReadClientFixture.CreateAsync(newtonsoft);
                await textless.Bind();
                textless.OnPush = request => Task.FromResult<object>(new PushReadResponse
                {
                    i = request.i, sc = request.sc,
                    push = new PushReadOutcome { k = outcome, _d = new[]
                    {
                        new Allors.Protocol.Json.Api.ResponseDerivationError { m = null, r = Array.Empty<Allors.Protocol.Json.Api.DerivationRelation>() },
                    } },
                });
                var result = await textless.Session.PushAsync(new PushOptions { Refresh = false });
                Assert.Equal(outcome == "rejected" ? MutationOutcome.Rejected : MutationOutcome.Unknown, result.Outcome);
                Assert.Equal(outcome == "unknown", result.ReconciliationRequired);
                Assert.Null(Assert.Single(result.Diagnostics.DerivationErrors).Message);
                var explanation = Assert.Single(result.Diagnostics.Errors);
                Assert.Equal(outcome == "rejected" ? "push-rejected" : "mutation-unknown", explanation.Code);
                if (outcome == "unknown") Assert.Contains("do not replay", explanation.Message);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task ProtectedPushSeparatesRefreshReceiptFromApplicationFailure(bool newtonsoft, bool throwFactory)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "submitted");
            var cause = new InvalidOperationException(throwFactory ? "Factory failed." : "Notification failed.");
            if (throwFactory) fixture.Factory.OnCreate = () => throw cause;
            else fixture.Session.OnChange += (_, _) => throw cause;
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                if (throwFactory)
                {
                    var additional = fixture.Read(new ReadRequest { i = request.i }, 2, "additional").p[0];
                    additional.i = 501;
                    ((PushReadChanged)response.changed).p = ((PushReadChanged)response.changed).p.Concat(new[] { additional }).ToArray();
                }
                return Task.FromResult<object>(response);
            };
            var saved = await fixture.Session.PushAsync();
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.True(saved.Refresh.Status == MutationReadStatus.Succeeded, string.Join(" | ", saved.Refresh.Errors.Select(v => v.Cause?.ToString() ?? v.Message)));
            Assert.Equal(MutationAcknowledgementStatus.Applied, saved.Application.Acknowledgement);
            Assert.Equal(throwFactory ? MutationRefreshApplicationStatus.Failed : MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
            var applicationError = Assert.Single(saved.Application.Errors);
            Assert.Equal(throwFactory ? MutationApplicationStage.Refresh : MutationApplicationStage.Notification, applicationError.Stage);
            Assert.Same(cause, applicationError.Cause);
            Assert.Contains(cause.Message, saved.ErrorMessage);
            Assert.Equal(throwFactory, saved.ReconciliationRequired);
            Assert.Equal(throwFactory, fixture.Session.ReconciliationRequired);
            Assert.True(saved.HasErrors);
            Assert.Equal(1, fixture.Pushes);
            Assert.Null(fixture.Session.GetStrategy(501));
            fixture.Factory.OnCreate = null;
            using var sibling = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            Assert.False((await sibling.PullAsync(fixture.Query)).HasErrors);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.True(saved.Refresh.Status == MutationReadStatus.Succeeded, string.Join(" | ", saved.Refresh.Errors.Select(v => v.Cause?.ToString() ?? v.Message)));
            sibling.Dispose();
            fixture.Session.Dispose();
            await fixture.Connection.DisposeAsync();
        }

        [Theory]
        [InlineData(false, "correlation")]
        [InlineData(true, "correlation")]
        [InlineData(false, "scope")]
        [InlineData(true, "scope")]
        public async Task ProtectedPushUntrustedHeaderReturnsUnknown(bool newtonsoft, string invalid)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            var id = created.Id;
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                if (invalid == "correlation") response.i = "different-request";
                else response.sc.p = "different-population";
                return Task.FromResult<object>(response);
            };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(MutationOutcome.Unknown, saved.Outcome);
            Assert.True(saved.ReconciliationRequired);
            Assert.True(fixture.Session.ReconciliationRequired);
            Assert.Empty(saved.CreationMappings);
            Assert.Equal(id, created.Id);
            Assert.Equal(1, fixture.Pushes);
            Assert.IsType<FormatException>(Assert.Single(saved.Diagnostics.Errors).Cause);
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PushAsync());
            Assert.Equal(1, fixture.Pushes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ProtectedPushRefreshFailureAllowsUnrelatedPermittedMutation(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "first submission");
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                if (fixture.Pushes == 1)
                {
                    Assert.Single(request.push.o);
                    response.changed = new ReadFailure { i = request.i, sc = request.sc, k = "error",
                        e = new[] { new ReadError { c = "refresh-unavailable", m = "Pull the affected object again.", o = new long[] { 101 } } } };
                }
                else
                {
                    Assert.False(request.returnChanged);
                    Assert.Empty(request.push.o ?? Array.Empty<PushRequestObject>());
                    Assert.Single(request.push.n);
                }
                return Task.FromResult<object>(response);
            };
            var first = await fixture.Session.PushAsync();
            Assert.Equal(MutationOutcome.Committed, first.Outcome);
            Assert.Equal(MutationReadStatus.Failed, first.Refresh.Status);
            Assert.Equal("refresh-unavailable", Assert.Single(first.Refresh.Errors).Code);
            Assert.Contains("Pull the affected object again.", first.ErrorMessage);
            Assert.False(first.ReconciliationRequired);
            Assert.False(fixture.Item.Strategy.CanWrite(fixture.Role));
            var unrelated = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            unrelated.C1AllorsString = "second submission";
            var second = await fixture.Session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(MutationOutcome.Committed, second.Outcome);
            Assert.False(second.HasErrors);
            Assert.Equal(500, unrelated.Id);
            Assert.Equal(2, fixture.Pushes);
            Assert.False(fixture.Item.Strategy.CanWrite(fixture.Role));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task NormalPushReadinessAndResultsUseOneProtectedPath(bool newtonsoft)
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            handler.OnRead = async request => { entered.TrySetResult(true); await release.Task; return EmptyBootstrapResponse(request); };
            var creating = handler.CreateAsync(client, newtonsoft);
            await entered.Task;
            Assert.False(creating.IsCompleted);
            Assert.Equal(0, handler.Pushes);
            release.SetResult(true);
            await using var connection = await creating;
            var session = connection.CreateWorkspace().CreateSession();
            var saved = await session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.NotRequested, saved.Refresh.Status);
            Assert.Equal(MutationReadStatus.NotRequested, saved.Discovery.Status);
            Assert.Equal(MutationAcknowledgementStatus.Applied, saved.Application.Acknowledgement);
            Assert.Empty(saved.CreationMappings);
            Assert.NotNull(saved.Diagnostics);
            Assert.Null(saved.PendingQuery);
            Assert.False(saved.ReconciliationRequired);
            Assert.False(saved.HasErrors);
            Assert.Equal(1, handler.Pushes);
            handler.OnPush = request => Task.FromResult<object>(handler.Pushes == 2 ? new PushReadResponse { i = request.i, sc = request.sc,
                push = new PushReadOutcome { k = "rejected", _d = new[] { new Allors.Protocol.Json.Api.ResponseDerivationError { m = null, r = Array.Empty<Allors.Protocol.Json.Api.DerivationRelation>() } } } } : "null");
            var rejected = await session.PushAsync();
            Assert.Equal(MutationOutcome.Rejected, rejected.Outcome);
            Assert.Null(Assert.Single(rejected.Diagnostics.DerivationErrors).Message);
            Assert.Equal("push-rejected", Assert.Single(rejected.Diagnostics.Errors).Code);
            var unknown = await session.PushAsync();
            Assert.Equal(MutationOutcome.Unknown, unknown.Outcome);
            Assert.True(unknown.ReconciliationRequired);
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync());
            Assert.Equal(3, handler.Pushes);
        }



        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedPushReadAppliesChangedBeforeDeferredFinalQuery(bool newtonsoft)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var paths = new List<string>();
            using var handler = new PushReadClientHandler(async message =>
            {
                var body = await message.Content.ReadAsStringAsync();
                paths.Add(message.RequestUri.AbsolutePath.Split('/').Last());
                if (paths.Last() == "push-read")
                {
                    var request = Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(body);
                    var read = SessionReadResponse(new ReadRequest { i = request.i }, type.Tag, role.RelationType.Tag);
                    read.p[0].v = 2;
                    read.p[0].p.ro[0].v = "saved";
                    return new PushReadResponse { i = request.i, sc = request.sc,
                        push = new PushReadOutcome { k = "committed", n = Array.Empty<PushResponseNewObject>() },
                        changed = new PushReadChanged { i = request.i, sc = request.sc, k = "changed", p = read.p, s = read.s, pm = read.pm, u = Array.Empty<long>() },
                        discovery = DiscoveryResponse(read) };
                }
                var input = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(body);
                var response = SessionReadResponse(input, type.Tag, role.RelationType.Tag);
                if (paths.Count == 4)
                {
                    Assert.Equal("read", input.k);
                    Assert.Equal(2, Assert.Single(input.h.r).v);
                    response.p[0].v = 2;
                    response.p[0].p = new ReadRecordPayload { r = 0 };
                }
                return input.k == "discover" ? DiscoveryResponse(response) : (object)response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await PushReadClientConnectionAsync(meta, client, newtonsoft);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var query = new Pull { ObjectId = 101 };
            await session.PullAsync(query);
            var item = session.Instantiate<Allors.Workspace.IObject>(101);
            item.Strategy.SetUnitRole(role, "submitted");
            var saved = await session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { query } });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.True(saved.Refresh.Status == MutationReadStatus.Succeeded, string.Join(" | ", saved.Refresh.Errors.Select(v => v.Cause?.ToString() ?? v.Message)));
            Assert.Equal(MutationReadStatus.Succeeded, saved.Discovery.Status);
            Assert.Equal("saved", item.Strategy.GetUnitRole(role));
            Assert.Same(item, session.Instantiate<Allors.Workspace.IObject>(101));
            Assert.Equal(3, paths.Count);
            Assert.False(saved.ReconciliationRequired);
            var final = await saved.PendingQuery.CompleteAsync();
            Assert.False(final.HasErrors);
            Assert.Equal(new[] { "read", "read", "push-read", "read" }, paths);
            await Assert.ThrowsAsync<InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
        }

        [Fact]
        public async Task ScopedPushReadPausesEditingAndOverlappingOperationsButKeepsInspection()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var handler = new PushReadClientHandler(async message =>
            {
                var body = await message.Content.ReadAsStringAsync();
                if (message.RequestUri.AbsolutePath.EndsWith("push-read", StringComparison.Ordinal))
                {
                    var request = Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(body);
                    entered.SetResult(true);
                    await release.Task;
                    return new PushReadResponse { i = request.i, sc = request.sc, push = new PushReadOutcome { k = "committed", n = Array.Empty<PushResponseNewObject>() } };
                }
                var input = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(body);
                var read = SessionReadResponse(input, type.Tag, role.RelationType.Tag);
                return input.k == "discover" ? DiscoveryResponse(read) : (object)read;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await PushReadClientConnectionAsync(meta, client, false);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var other = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            await session.PullAsync(new Pull { ObjectId = 101 });
            var item = session.Instantiate<Allors.Workspace.IObject>(101);
            item.Strategy.SetUnitRole(role, "submitted");
            var save = session.PushAsync(new PushOptions { Refresh = false });
            await entered.Task;
            Assert.Equal("submitted", item.Strategy.GetUnitRole(role));
            Assert.True(item.Strategy.HasChanges);
            Assert.Throws<InvalidOperationException>(() => item.Strategy.SetUnitRole(role, "overlap"));
            Assert.Throws<InvalidOperationException>(() => item.Strategy.RestoreRole(role));
            Assert.Throws<InvalidOperationException>(() => item.Strategy.Reset());
            Assert.Throws<InvalidOperationException>(() => session.Reset());
            Assert.Throws<InvalidOperationException>(() => session.Checkpoint());
            Assert.Throws<InvalidOperationException>(() => session.Create<Allors.Workspace.IObject>((Allors.Workspace.Meta.IClass)type));
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync(new PushOptions { Refresh = false }));
            Assert.NotNull(other.Create<Allors.Workspace.IObject>((Allors.Workspace.Meta.IClass)type));
            release.SetResult(true);
            var saved = await save;
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.NotRequested, saved.Refresh.Status);
            Assert.False(item.Strategy.CanWrite(role));
            Assert.NotNull(session.Create<Allors.Workspace.IObject>((Allors.Workspace.Meta.IClass)type));
        }

        [Theory]
        [InlineData(false, "changed")]
        [InlineData(true, "changed")]
        [InlineData(false, "discovery")]
        [InlineData(true, "discovery")]
        [InlineData(false, "changed-duplicate")]
        [InlineData(true, "changed-duplicate")]
        [InlineData(false, "discovery-duplicate")]
        [InlineData(true, "discovery-duplicate")]
        public async Task ScopedPushReadKeepsCommittedReceiptAndIndependentValidRead(bool newtonsoft, string invalid)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "submitted");
            fixture.OnPush = request =>
            {
                var response = JsonNode.Parse(JsonSerializer.Serialize(fixture.Response(request)));
                var part = invalid.StartsWith("changed", StringComparison.Ordinal) ? "changed" : "discovery";
                if (invalid.EndsWith("duplicate", StringComparison.Ordinal))
                {
                    var raw = response.ToJsonString();
                    var marker = "\"" + part + "\":{";
                    return Task.FromResult<object>(raw.Replace(marker, marker + "\"k\":\"invalid\","));
                }
                response[part] = new JsonObject { ["k"] = "bad" };
                return Task.FromResult<object>(response.ToJsonString());
            };
            var saved = await fixture.Session.PushAsync(new PushOptions
            { Refresh = true, Discover = new[] { fixture.Query } });
            var badChanged = invalid.StartsWith("changed", StringComparison.Ordinal);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.False(saved.ReconciliationRequired);
            Assert.Equal(badChanged ? MutationReadStatus.Failed : MutationReadStatus.Succeeded, saved.Refresh.Status);
            Assert.Equal(badChanged ? MutationReadStatus.Succeeded : MutationReadStatus.Failed, saved.Discovery.Status);
            Assert.Equal(badChanged ? "submitted" : "saved", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            Assert.Equal(badChanged, saved.PendingQuery != null);
            if (saved.PendingQuery != null) Assert.False((await saved.PendingQuery.CompleteAsync()).HasErrors);
        }

        [Theory]
        [InlineData("rejected")]
        [InlineData("unknown")]
        [InlineData("lost")]
        [InlineData("mapping-collision")]
        [InlineData("mapping-omitted")]
        public async Task ScopedPushReadSeparatesMutationCertaintyAndReconciliation(string outcome)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var first = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            var second = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            var firstId = first.Id;
            var secondId = second.Id;
            fixture.OnPush = request =>
            {
                if (outcome == "lost") throw new System.IO.IOException("Connection lost after dispatch.");
                var response = fixture.Response(request);
                if (outcome == "rejected" || outcome == "unknown") response.push = new PushReadOutcome
                { k = outcome, e = new[] { new ReadError { c = "test", m = "Reconcile or correct this mutation." } } };
                if (outcome == "mapping-collision") response.push.n[1].d = 101;
                if (outcome == "mapping-omitted") response.push.n = response.push.n.Take(1).ToArray();
                return Task.FromResult<object>(response);
            };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(outcome == "mapping-collision" ? MutationOutcome.Committed : outcome == "rejected" ? MutationOutcome.Rejected : MutationOutcome.Unknown, saved.Outcome);
            Assert.Equal(firstId, first.Id);
            Assert.Equal(secondId, second.Id);
            Assert.Equal(outcome != "rejected", saved.ReconciliationRequired);
            if (outcome != "rejected")
            {
                Assert.Throws<InvalidOperationException>(() => first.C1AllorsString = "again");
                await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PushAsync(new PushOptions { Refresh = false }));
            }
            else first.C1AllorsString = "corrected";
            Assert.False((await fixture.Session.PullAsync(fixture.Query)).HasErrors);
            Assert.Equal(1, fixture.Pushes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedPushReadNewIdsPreserveObjectIdentityAndCheckpointOwnership(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            fixture.Session.Checkpoint();
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "local";
            var localId = created.Id;
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                var changed = (PushReadChanged)response.changed;
                changed.p = changed.p.Concat(new[] { new ReadRecord { i = 500, c = fixture.Type.Tag, v = 1,
                    g = new[] { 0 }, r = Array.Empty<int>(), p = new ReadRecordPayload { ro = new[] { new SyncResponseRole { t = fixture.Role.RelationType.Tag, v = "created" } } } } }).ToArray();
                return Task.FromResult<object>(response);
            };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.True(saved.Refresh.Status == MutationReadStatus.Succeeded, string.Join(" | ", saved.Refresh.Errors.Select(v => v.Cause?.ToString() ?? v.Message)));
            Assert.Equal(500, created.Id);
            Assert.Same(created, fixture.Session.Instantiate<Allors.Workspace.Domain.C1>(500));
            Assert.Null(fixture.Session.Instantiate<Allors.Workspace.Domain.C1>(localId));
            Assert.Equal("created", created.C1AllorsString);
            Assert.False(created.Strategy.HasChanges);
            var checkpoint = fixture.Session.Checkpoint();
            Assert.Contains(created.Strategy, checkpoint.Created);
            Assert.Contains(fixture.Item.Strategy, checkpoint.AssociationsByRoleType[fixture.Role]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedPushReadPendingCompletionMergesInterveningEditsAndOwnsCapturedQuery(bool conflict)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            fixture.Session.Checkpoint();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "submitted");
            var query = new Pull { ObjectId = 101 };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { query } });
            query.ObjectId = 999;
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "later edit");
            fixture.OnRead = request =>
            {
                Assert.Equal("read", request.k);
                Assert.Equal(101, request.p.l.Single().o);
                Assert.Equal(2, Assert.Single(request.h.r).v);
                var response = fixture.Read(request, conflict ? 3 : 2, conflict ? "other commit" : "saved");
                return Task.FromResult<object>(response);
            };
            var refreshed = await saved.PendingQuery.CompleteAsync();
            Assert.Equal(conflict ? 1 : 0, refreshed.MergeErrors.Count());
            Assert.Equal("later edit", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            Assert.Contains(fixture.Item.Strategy, fixture.Session.Checkpoint().AssociationsByRoleType[fixture.Role]);
            Assert.Equal(4, fixture.Requests);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationRefreshApplicationStatus.Applied, saved.Application.Refresh);
        }

        [Theory]
        [InlineData("close")]
        [InlineData("retire")]
        [InlineData("cancel")]
        public async Task ScopedPushReadKeepsCommitAcrossPostDispatchLifecycleChanges(string action)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            using var cancellation = new System.Threading.CancellationTokenSource();
            fixture.OnPush = request =>
            {
                if (action == "close") fixture.Session.Dispose();
                if (action == "retire") _ = fixture.Connection.DisposeAsync();
                if (action == "cancel") cancellation.Cancel();
                return Task.FromResult<object>(fixture.Response(request));
            };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true }, cancellation.Token);
            Assert.Equal(action == "cancel" ? MutationOutcome.Unknown : MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(action == "cancel" ? MutationReadStatus.Unavailable : MutationReadStatus.Succeeded, saved.Refresh.Status);
            if (action != "cancel") Assert.Equal(MutationRefreshApplicationStatus.Skipped, saved.Application.Refresh);
            Assert.True(saved.ReconciliationRequired);
            Assert.Equal(1, fixture.Pushes);
        }

        [Theory]
        [InlineData("close")]
        [InlineData("retire")]
        [InlineData("cancel")]
        public async Task ScopedPushReadPendingReadRejectsInvalidLifetimeAndIsOneUse(string action)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = false, Discover = new[] { fixture.Query } });
            using var cancellation = new System.Threading.CancellationTokenSource();
            if (action == "close") fixture.Session.Dispose();
            if (action == "retire") _ = fixture.Connection.DisposeAsync();
            if (action == "cancel") cancellation.Cancel();
            await Assert.ThrowsAnyAsync<Exception>(() => saved.PendingQuery.CompleteAsync(cancellation.Token));
            await Assert.ThrowsAsync<InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
            Assert.Equal(3, fixture.Requests);
        }

        [Fact]
        public async Task ScopedPushReadRejectsEarlierOperationAndRecoversAfterPredispatchFailure()
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnRead = async request => { entered.TrySetResult(true); await release.Task; return request.k == "discover" ? DiscoveryResponse(fixture.Read(request)) : (object)fixture.Read(request); };
            var pull = fixture.Session.PullAsync(fixture.Query);
            await entered.Task;
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PushAsync(new PushOptions { Refresh = false }));
            Assert.Equal(0, fixture.Pushes);
            release.SetResult(true);
            await pull;
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            await Assert.ThrowsAnyAsync<Exception>(() => fixture.Session.PushAsync(new PushOptions { Refresh = false, Discover = new[] { new Pull { Object = created } } }));
            created.C1AllorsString = "still editable";
            using var cancelled = new System.Threading.CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Session.PushAsync(new PushOptions { Refresh = false }, cancellationToken: cancelled.Token));
            Assert.Equal(0, fixture.Pushes);
            Assert.Equal(MutationOutcome.Committed, (await fixture.Session.PushAsync(new PushOptions { Refresh = false })).Outcome);
        }

        [Fact]
        public async Task ScopedPushReadRejectsUnsavedObjectArgumentsBeforeDispatch()
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            await Assert.ThrowsAnyAsync<Exception>(() => fixture.Session.PushAsync(new PushOptions
            { Discover = new[] { new Pull { ObjectId = 101, Arguments = new Dictionary<string, object> { ["item"] = created } } } }));
            Assert.Equal(0, fixture.Pushes);
            created.C1AllorsString = "editable";
        }

        [Fact]
        public async Task ScopedPushReadEditingPauseCoversStrategyAndBothDirectOriginStateSurfaces()
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var source = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            var target = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            using var other = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            var otherObject = other.Create<Allors.Workspace.Domain.C1>();
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            fixture.OnPush = async request => { entered.SetResult(true); await release.Task; return fixture.Response(request); };
            var save = fixture.Session.PushAsync(new PushOptions { Refresh = false });
            await entered.Task;
            var strategy = (Allors.Workspace.Adapters.Strategy)source.Strategy;
            var targetStrategy = (Allors.Workspace.Adapters.Strategy)target.Strategy;
            var state = strategy.DatabaseOriginState;
            var sessionState = fixture.Session.SessionOriginState;
            var one = fixture.Type.RoleTypes.Single(v => v.Name == "C1C1One2One");
            var many = fixture.Type.RoleTypes.First(v => v.Origin == Origin.Database && v.IsMany && v.ObjectType == fixture.Type);
            var sessionMany = fixture.Type.RoleTypes.First(v => v.Origin == Origin.Session && v.IsMany && v.ObjectType == fixture.Type);
            var sessionOne = fixture.Type.RoleTypes.First(v => v.Origin == Origin.Session && v.IsOne && v.ObjectType.IsComposite);
            var sessionUnit = fixture.Type.RoleTypes.First(v => v.Origin == Origin.Session && v.ObjectType.IsUnit);
            var range = strategy.Ranges.Load(new[] { targetStrategy });
            var changes = new Allors.Workspace.Adapters.ChangeSet(fixture.Session, null, null);
            Action[] mutations =
            {
                () => strategy.SetRole(fixture.Role, "x"),
                () => strategy.RemoveRole(fixture.Role),
                () => strategy.SetCompositeRole(one, target),
                () => strategy.SetCompositesRole(many, new[] { target }),
                () => strategy.AddCompositesRole(many, target),
                () => strategy.RemoveCompositesRole(many, target),
                () => state.SetUnitRole(fixture.Role, "x"),
                () => state.SetCompositeRole(one, targetStrategy),
                () => state.SetCompositesRole(many, range),
                () => state.AddCompositesRole(many, targetStrategy),
                () => state.RemoveCompositesRole(many, targetStrategy),
                () => state.RestoreRole(fixture.Role),
                () => state.Reset(),
                () => state.Checkpoint(changes),
                () => sessionState.SetUnitRole(strategy, sessionUnit, null),
                () => sessionState.SetCompositeRole(strategy, sessionOne, targetStrategy),
                () => sessionState.SetCompositesRole(strategy, sessionMany, range),
                () => sessionState.AddCompositesRole(strategy, sessionMany, targetStrategy),
                () => sessionState.RemoveCompositesRole(strategy, sessionMany, targetStrategy),
                () => sessionState.SetUnitRole((Allors.Workspace.Adapters.Strategy)otherObject.Strategy, sessionUnit, null),
                () => sessionState.Checkpoint(changes),
                () => fixture.Session.Activate(Array.Empty<Allors.Workspace.Derivations.IRule>()),
            };
            foreach (var mutate in mutations) Assert.Throws<InvalidOperationException>(mutate);
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.InvokeAsync(Array.Empty<Allors.Workspace.Method>()));
            Assert.Equal("value", fixture.Item.Strategy.GetUnitRole(fixture.Role));
            release.SetResult(true);
            Assert.Equal(MutationOutcome.Committed, (await save).Outcome);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedPushReadReportsRequestedReadFailuresAndUnavailableObjects(bool unavailable)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "submitted");
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                if (unavailable) response.changed = new PushReadChanged { i = request.i, sc = request.sc, k = "changed", p = Array.Empty<ReadRecord>(), s = Array.Empty<ReadSecurity>(), pm = Array.Empty<ReadPermission>(), u = new long[] { 101 } };
                else response.changed = new ReadFailure { i = request.i, sc = request.sc, k = "error", e = new[] { new ReadError { c = "refresh", m = "Refresh the submitted objects." } } };
                response.discovery = new ReadFailure { i = request.i, sc = request.sc, k = "error", e = new[] { new ReadError { c = "query", m = "Correct the query." } } };
                return Task.FromResult<object>(response);
            };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { fixture.Query } });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(unavailable ? MutationReadStatus.Succeeded : MutationReadStatus.Failed, saved.Refresh.Status);
            Assert.Equal(MutationReadStatus.Failed, saved.Discovery.Status);
            Assert.IsType<Allors.Workspace.Adapters.Remote.ReadOperationException>(saved.Discovery.Errors.Single().Cause);
            Assert.Null(saved.PendingQuery);
            Assert.False(fixture.Item.Strategy.CanWrite(fixture.Role));
            if (unavailable) Assert.False(fixture.Item.Strategy.CanRead(fixture.Role));
            Assert.NotEmpty(fixture.Item.Strategy.Diff());
        }

        [Fact]
        public async Task NormalFactoryRequiresPushCapabilityBeforeBootstrap()
        {
            using var handler = new LifecycleHttpHandler();
            using var client = handler.CreateClient();
            await Assert.ThrowsAsync<ArgumentException>(() => FactoryTestConnection.CreateAsync(handler.Configuration(), () => new LifecycleWorkspaceServices(),
                new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), null, new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client)));
            Assert.Empty(handler.Reads);
            Assert.Equal(0, handler.Pushes + handler.Invokes);
            Assert.True((await client.GetAsync("borrowed")).IsSuccessStatusCode);
            await using var replacement = await handler.CreateAsync(client, false);
            var created = replacement.CreateWorkspace().CreateSession().Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "editable";
            Assert.Equal("editable", created.C1AllorsString);
        }

        [Fact]
        public async Task ScopedPushReadAcknowledgesCompleteReceiptEvenAfterCancellation()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            using var cancellation = new System.Threading.CancellationTokenSource();
            var transport = new PushReadClientTransport(request =>
            {
                cancellation.Cancel();
                var raw = JsonSerializer.Serialize(new PushReadResponse { i = request.i, sc = request.sc,
                    push = new PushReadOutcome { k = "committed", n = request.push.n.Select(v => new PushResponseNewObject { w = v.w, d = 500 }).ToArray() },
                    changed = new PushReadChanged { i = request.i, sc = request.sc, k = "changed", p = Array.Empty<ReadRecord>(), s = Array.Empty<ReadSecurity>(), pm = Array.Empty<ReadPermission>(), u = Array.Empty<long>() } });
                return Task.FromResult(raw);
            });
            await using var connection = await PushReadTestConnection.CreateAsync(meta, client, transport);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            await session.PullAsync(new Pull { ObjectId = 101 });
            var created = session.Create<Allors.Workspace.Domain.C1>();
            created.C1AllorsString = "submitted";
            var saved = await session.PushAsync(new PushOptions { Refresh = true }, cancellation.Token);
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(500, created.Id);
            Assert.True(saved.Refresh.Status == MutationReadStatus.Succeeded, string.Join(" | ", saved.Refresh.Errors.Select(v => v.Cause?.ToString() ?? v.Message)));
            Assert.Equal(MutationRefreshApplicationStatus.Skipped, saved.Application.Refresh);
            Assert.IsType<OperationCanceledException>(saved.Application.Errors.Single().Cause);
            Assert.False(saved.ReconciliationRequired);
            Assert.False(created.Strategy.CanWrite(role));
            Assert.NotNull(session.Create<Allors.Workspace.Domain.C1>());
        }

        private sealed class InvokeReadTestConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection
        {
            private InvokeReadTestConnection(Allors.Workspace.Meta.IMetaPopulation meta, ScopedFactory factory)
                : base(new ScopedConfiguration(meta, factory), new Allors.Workspace.Adapters.IdGenerator(), () => new Allors.Workspace.WorkspaceServices(), new Allors.Ranges.DefaultStructRanges<long>()) { }
            internal static async Task<InvokeReadTestConnection> CreateAsync(Allors.Workspace.Meta.IMetaPopulation meta, System.Net.Http.HttpClient client, ScopedFactory factory,
                bool newtonsoft, Allors.Workspace.Adapters.Remote.IInvokeReadTransport capability)
            {
                var connection = new InvokeReadTestConnection(meta, factory);
                var read = newtonsoft ? (Allors.Workspace.Adapters.Remote.IReadTransport)new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(client) : new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client);
                await connection.InitializeAsync(new BootstrapReadTransport(read), "v1", pushReadTransport: new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client), invokeReadTransport: capability);
                return connection;
            }
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
        }

        private sealed class InvokeReadClientTransport : Allors.Workspace.Adapters.Remote.IInvokeReadTransport
        {
            private readonly Func<InvokeReadRequest, Task<string>> send;
            private readonly bool newtonsoft;
            internal InvokeReadClientTransport(Func<InvokeReadRequest, Task<string>> send, bool newtonsoft = false) { this.send = send; this.newtonsoft = newtonsoft; }
            public InvokeReadRequest CaptureRequest(InvokeReadRequest request) => this.newtonsoft ? Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeRequest(JsonConvert.SerializeObject(request)) : InvokeReadJson.DeserializeRequest(JsonSerializer.Serialize(request));
            public InvokeReadReceipt CaptureReceipt(string response, InvokeReadRequest request) => this.newtonsoft ? Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeReceipt(response, request) : InvokeReadJson.DeserializeReceipt(response, request);
            public object CapturePart(string response, InvokeReadRequest request) => this.newtonsoft ? Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeChanged(response, request) : InvokeReadJson.DeserializeChanged(response, request);
            public Task<string> Send(InvokeReadRequest request, System.Threading.CancellationToken cancellationToken = default) => this.send(request);
        }

        private sealed class PushReadClientTransport : Allors.Workspace.Adapters.Remote.IPushReadTransport
        {
            private readonly Func<PushReadRequest, Task<string>> send;
            internal PushReadClientTransport(Func<PushReadRequest, Task<string>> send) => this.send = send;
            public PushReadRequest CaptureRequest(PushReadRequest request) => Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(JsonSerializer.Serialize(request));
            public Task<string> Send(PushReadRequest request, System.Threading.CancellationToken cancellationToken = default) => this.send(request);
            public PushReadReceipt CaptureReceipt(string response, PushReadRequest request) => Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeReceipt(response, request);
            public object CapturePart(string response, PushReadRequest request, bool changed) => Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializePart(response, request, changed);
        }

        private sealed class PushReadTestConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection
        {
            private PushReadTestConnection(Allors.Workspace.Meta.IMetaPopulation meta)
                : base(new ScopedConfiguration(meta, new ScopedFactory(meta)), new Allors.Workspace.Adapters.IdGenerator(), () => new Allors.Workspace.WorkspaceServices(), new Allors.Ranges.DefaultStructRanges<long>()) { }
            internal static async Task<PushReadTestConnection> CreateAsync(Allors.Workspace.Meta.IMetaPopulation meta, System.Net.Http.HttpClient client, Allors.Workspace.Adapters.Remote.IPushReadTransport capability)
            {
                var connection = new PushReadTestConnection(meta);
                await connection.InitializeAsync(new BootstrapReadTransport(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client)), "v1",
                    pushReadTransport: capability, invokeReadTransport: new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client));
                return connection;
            }
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
        }

        [Fact]
        public async Task ScopedPushReadPendingUiGapDoesNotHoldConnectionGate()
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { fixture.Query } });
            using (var other = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession())
            {
                Assert.False((await other.PullAsync(fixture.Query)).HasErrors);
            }
            fixture.Session.Dispose();
            await fixture.Connection.DisposeAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => saved.PendingQuery.CompleteAsync());
        }

        [Theory]
        [InlineData("scope")]
        [InlineData("discovery-class")]
        [InlineData("changed-class")]
        public async Task ScopedPushReadRejectsUnusableReadMetadataWithoutLosingReceipt(string invalid)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            fixture.Item.Strategy.SetUnitRole(fixture.Role, "submitted");
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                if (invalid == "scope") response.changed = new ReadFailure { i = request.i, k = "error",
                    sc = new ReadScope { p = "other", u = request.sc.u, w = request.sc.w, r = request.sc.r },
                    e = new[] { new ReadError { c = "scope_changed", m = "Establish the current identity before reading again.", o = new long[] { 101 } } } };
                if (invalid == "discovery-class") ((DiscoverySuccess)response.discovery).p[0].c = "unknown-class";
                if (invalid == "changed-class") ((PushReadChanged)response.changed).p[0].c = fixture.Meta.FindByName("C2").Tag;
                return Task.FromResult<object>(response);
            };
            var saved = await fixture.Session.PushAsync(new PushOptions { Refresh = true, Discover = new[] { fixture.Query } });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(invalid == "discovery-class" ? MutationReadStatus.Succeeded : MutationReadStatus.Failed, saved.Refresh.Status);
            Assert.Equal(invalid == "discovery-class" ? MutationReadStatus.Failed : MutationReadStatus.Succeeded, saved.Discovery.Status);
            if (invalid == "scope")
            {
                var error = Assert.IsType<Allors.Workspace.Adapters.Remote.ReadOperationException>(saved.Refresh.Errors.Single().Cause);
                Assert.Equal("scope_changed", Assert.Single(error.Errors).Code);
                Assert.Equal(101, Assert.Single(error.Errors.Single().ObjectIds));
                await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Session.PullAsync());
            }
            if (invalid == "changed-class") Assert.Equal(fixture.Type.Tag, ScopedStore(fixture.Connection).GetCurrentRecord(101).ClassTag);
        }

        [Fact]
        public async Task ScopedPushReadCannotMapNewIdsOntoAnotherSessionsCachedObject()
        {
            await using var fixture = await PushReadClientFixture.CreateAsync();
            await fixture.Bind();
            using var saving = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
            var created = saving.Create<Allors.Workspace.Domain.C1>();
            var original = created.Id;
            fixture.OnPush = request =>
            {
                var response = fixture.Response(request);
                response.push.n[0].d = 101;
                return Task.FromResult<object>(response);
            };
            var saved = await saving.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.True(saved.ReconciliationRequired);
            Assert.Equal(original, created.Id);
            Assert.Null(saving.Instantiate<Allors.Workspace.IObject>(101));
            Assert.NotNull(fixture.Item);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedPushReadTransportOwnsCapturedRequestAcrossHostMutation(bool newtonsoft)
        {
            await using var fixture = await PushReadClientFixture.CreateAsync(newtonsoft);
            await fixture.Bind();
            var bytesRole = fixture.Type.RoleTypes.Single(v => v.Name == "C1AllorsBinary");
            var created = fixture.Session.Create<Allors.Workspace.Domain.C1>();
            var buffer = new byte[] { 1, 2, 3 };
            created.Strategy.SetUnitRole(bytesRole, buffer);
            fixture.OnPush = request =>
            {
                buffer[0] = 9;
                var value = request.push.n.Single().r.Single(v => v.t == bytesRole.RelationType.Tag).u;
                Assert.Equal("AQID", value.ToString());
                return Task.FromResult<object>(fixture.Response(request));
            };
            Assert.Equal(MutationOutcome.Committed, (await fixture.Session.PushAsync(new PushOptions { Refresh = false })).Outcome);
        }

        private sealed class PushReadClientFixture : IAsyncDisposable
        {
            private readonly PushReadClientHandler handler;
            private readonly System.Net.Http.HttpClient client;
            internal readonly Allors.Workspace.Meta.IMetaPopulation Meta;
            internal readonly Allors.Workspace.Meta.IClass Type;
            internal readonly ScopedFactory Factory;
            internal readonly Allors.Workspace.Meta.IRoleType Role;
            internal Allors.Workspace.Adapters.Remote.DatabaseConnection Connection;
            internal Allors.Workspace.Adapters.Remote.Session Session;
            internal readonly Pull Query = new Pull { ObjectId = 101 };
            internal Func<PushReadRequest, Task<object>> OnPush;
            internal Func<InvokeReadRequest, Task<object>> OnInvoke;
            internal int Invokes;
            internal Allors.Workspace.Method Method => new Allors.Workspace.Method(this.Item, this.Type.MethodTypes.First());
            internal Func<ReadRequest, Task<object>> OnRead;
            internal int Pushes;
            internal int Requests;
            internal Allors.Workspace.IObject Item => this.Session.Instantiate<Allors.Workspace.IObject>(101);
            private PushReadClientFixture()
            {
                this.Meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
                this.Type = (Allors.Workspace.Meta.IClass)this.Meta.FindByName("C1");
                this.Role = this.Type.RoleTypes.Single(v => v.Name == "C1AllorsString");
                this.handler = new PushReadClientHandler(async message =>
                {
                    ++this.Requests;
                    var raw = await message.Content.ReadAsStringAsync();
                    if (message.RequestUri.AbsolutePath.EndsWith("push-read", StringComparison.Ordinal))
                    {
                        ++this.Pushes;
                        var request = Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(raw);
                        return this.OnPush == null ? this.Response(request) : await this.OnPush(request);
                    }
                    if (message.RequestUri.AbsolutePath.EndsWith("invoke-read", StringComparison.Ordinal))
                    {
                        ++this.Invokes;
                        var request = InvokeReadJson.DeserializeRequest(raw);
                        return this.OnInvoke == null ? this.InvokeResponse(request) : await this.OnInvoke(request);
                    }
                    var input = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(raw);
                    if (this.OnRead != null) return await this.OnRead(input);
                    var read = this.Read(input, this.Pushes > 0 ? 2 : 1, this.Pushes > 0 ? "saved" : "value");
                    return input.k == "discover" ? DiscoveryResponse(read) : (object)read;
                });
                this.client = new System.Net.Http.HttpClient(this.handler) { BaseAddress = new Uri("http://localhost/allors/") };
                this.Factory = new ScopedFactory(this.Meta);
            }
            internal static async Task<PushReadClientFixture> CreateAsync(bool newtonsoft = false, bool directInvoke = false, Func<Allors.Workspace.IWorkspaceServices> services = null)
            {
                var fixture = new PushReadClientFixture();
                try
                {
                fixture.Connection = directInvoke
                    ? await InvokeReadTestConnection.CreateAsync(fixture.Meta, fixture.client, fixture.Factory, newtonsoft, new InvokeReadClientTransport(async request =>
                    {
                        ++fixture.Invokes;
                        var response = fixture.OnInvoke == null ? fixture.InvokeResponse(request) : await fixture.OnInvoke(request);
                        return response is string text ? text : JsonSerializer.Serialize(response);
                    }, newtonsoft))
                    : await PushReadClientConnectionAsync(fixture.Meta, fixture.client, newtonsoft, fixture.Factory, services);
                fixture.Session = (Allors.Workspace.Adapters.Remote.Session)fixture.Connection.CreateWorkspace().CreateSession();
                    return fixture;
                }
                catch { fixture.client.Dispose(); fixture.handler.Dispose(); throw; }
            }
            internal Task<Allors.Workspace.IPullResult> Bind() => this.Session.PullAsync(this.Query);
            internal ReadSuccess Read(ReadRequest request, long version = 1, string value = "value")
            {
                var response = SessionReadResponse(request, this.Type.Tag, this.Role.RelationType.Tag);
                response.p[0].v = version;
                response.p[0].p.ro[0].v = value;
                return response;
            }
            internal PushReadResponse Response(PushReadRequest request)
            {
                var read = this.Read(new ReadRequest { i = request.i }, 2, "saved");
                return new PushReadResponse { i = request.i, sc = request.sc,
                    push = new PushReadOutcome { k = "committed", n = (request.push.n ?? Array.Empty<PushRequestNewObject>()).Select((v, i) => new PushResponseNewObject { w = v.w, d = 500 + i }).ToArray() },
                    changed = request.returnChanged == true ? new PushReadChanged { i = request.i, sc = request.sc, k = "changed", p = read.p, s = read.s, pm = read.pm, u = Array.Empty<long>() } : null,
                    discovery = request.discover == null ? null : DiscoveryResponse(read) };
            }
            internal InvokeReadResponse InvokeResponse(InvokeReadRequest request, string value = "value")
            {
                var read = this.Read(new ReadRequest { i = request.i }, 2, value);
                return new InvokeReadResponse { i = request.i, sc = request.sc, invoke = new InvokeReadOutcome { k = "committed" },
                    changed = request.refresh ? new PushReadChanged { i = request.i, sc = request.sc, k = "changed", p = read.p, s = read.s, pm = read.pm, u = Array.Empty<long>() } : null };
            }
            public async ValueTask DisposeAsync() { try { await this.Connection.DisposeAsync(); } finally { this.client.Dispose(); this.handler.Dispose(); } }
        }

        private static Task<Allors.Workspace.Adapters.Remote.DatabaseConnection> PushReadClientConnectionAsync(Allors.Workspace.Meta.IMetaPopulation meta, System.Net.Http.HttpClient client, bool newtonsoft, Allors.Workspace.Adapters.ReflectionObjectFactory factory = null, Func<Allors.Workspace.IWorkspaceServices> services = null)
        {
            var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", meta,
                factory ?? new Allors.Workspace.Adapters.ReflectionObjectFactory(meta, typeof(Allors.Workspace.Domain.Person)), Array.Empty<Allors.Workspace.Derivations.IRule>());
            Allors.Workspace.Adapters.Remote.IReadTransport read = newtonsoft ? new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(client) : new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client);
            Allors.Workspace.Adapters.Remote.IPushReadTransport push = newtonsoft ? new Allors.Workspace.Adapters.Remote.Newtonsoft.PushReadTransport(client) : new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client);
            Allors.Workspace.Adapters.Remote.IInvokeReadTransport invoke = newtonsoft ? new Allors.Workspace.Adapters.Remote.Newtonsoft.InvokeReadTransport(client) : new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client);
            return FactoryTestConnection.CreateAsync(configuration, services ?? (() => new Allors.Workspace.WorkspaceServices()), new BootstrapReadTransport(read), push, invoke, newtonsoft);
        }

        private sealed class FactoryTestConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection
        {
            private FactoryTestConnection(Allors.Workspace.Adapters.Configuration configuration, Func<Allors.Workspace.IWorkspaceServices> services, bool newtonsoft)
                : base(configuration, new Allors.Workspace.Adapters.IdGenerator(), services, new Allors.Ranges.DefaultStructRanges<long>()) =>
                this.UnitConvert = newtonsoft ? new Allors.Protocol.Json.Newtonsoft.UnitConvert() : new Allors.Protocol.Json.SystemTextJson.UnitConvert();
            internal static async Task<Allors.Workspace.Adapters.Remote.DatabaseConnection> CreateAsync(Allors.Workspace.Adapters.Configuration configuration, Func<Allors.Workspace.IWorkspaceServices> services,
                Allors.Workspace.Adapters.Remote.IReadTransport read, Allors.Workspace.Adapters.Remote.IPushReadTransport push, Allors.Workspace.Adapters.Remote.IInvokeReadTransport invoke,
                bool newtonsoft = false, string context = null, System.Threading.CancellationToken cancellationToken = default, Action<FactoryTestConnection> capture = null)
            {
                var connection = new FactoryTestConnection(configuration, services, newtonsoft);
                capture?.Invoke(connection);
                await connection.InitializeAsync(read, "v1", context, push, invoke, cancellationToken);
                return connection;
            }
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; }
        }

        private sealed class BootstrapReadTransport : Allors.Workspace.Adapters.Remote.IReadTransport
        {
            private readonly Allors.Workspace.Adapters.Remote.IReadTransport inner;
            private readonly string context;
            internal int Calls;
            internal BootstrapReadTransport(Allors.Workspace.Adapters.Remote.IReadTransport inner, string context = null) { this.inner = inner; this.context = context; }
            public ReadRequest CaptureRequest(ReadRequest request) => this.inner.CaptureRequest(request);
            public object CaptureResponse(object response, ReadRequest request) => this.inner.CaptureResponse(response, request);
            public int RequestByteCount(ReadRequest request) => this.inner.RequestByteCount(request);
            public Task<object> Read(ReadRequest request, System.Threading.CancellationToken cancellationToken = default)
            {
                if (this.Calls >= 2) return this.inner.Read(request, cancellationToken);
                Assert.Empty(request.p.l);
                Assert.Empty(request.p.d);
                Assert.Equal(this.context, request.x);
                Assert.Equal(this.Calls++ == 0 ? "discover" : "read", request.k);
                return Task.FromResult(EmptyBootstrapResponse(request));
            }
        }

        private static object EmptyBootstrapResponse(ReadRequest request)
        {
            var complete = new ReadSuccess { i = request.i, k = "complete", sc = new ReadScope { p = "population-1", u = 1, w = "Default", r = "v1", x = request.x },
                l = Array.Empty<ReadInput>(), o = new Dictionary<string, long?>(), c = new Dictionary<string, long[]>(),
                p = Array.Empty<ReadRecord>(), s = Array.Empty<ReadSecurity>(), pm = Array.Empty<ReadPermission>() };
            return request.k == "discover" ? DiscoveryResponse(complete) : (object)complete;
        }

        private sealed class PushReadClientHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly Func<System.Net.Http.HttpRequestMessage, Task<object>> response;
            internal PushReadClientHandler(Func<System.Net.Http.HttpRequestMessage, Task<object>> response) => this.response = response;
            protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var result = await this.response(request);
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                { Content = new System.Net.Http.StringContent(result is string raw ? raw : JsonSerializer.Serialize(result)) };
            }
        }

        public override IProfile Profile { get; }

        [Fact]
        public async Task ScopedSessionOwnsBaselinesAndRejectsCrossSessionInstantiation()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var calls = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                Assert.EndsWith("/read", message.RequestUri.AbsolutePath);
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = CoordinatorResponse(request, type.Tag, role.RelationType.Tag);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                response.p[0].v = ++calls;
                response.p[0].p.ro[0].v = "value" + calls;
                response.pm = new[]
                {
                    new ReadPermission { i = 201, p = new ReadPermissionPayload { c = type.Tag, o = (long)Operations.Read, t = role.RelationType.Tag } },
                    new ReadPermission { i = 202, p = new ReadPermissionPayload { c = type.Tag, o = (long)Operations.Write, t = role.RelationType.Tag } },
                };
                response.s[0].p.p = new long[] { 201, 202 };
                response.o["item"] = 101;
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            var workspace = connection.CreateWorkspace();
            using var first = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
            using var second = (Allors.Workspace.Adapters.Remote.Session)workspace.CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            var initial = (Allors.Workspace.Adapters.Remote.ScopedPullResult)await first.PullAsync(pull);
            var obj = initial.GetObject<Allors.Workspace.Domain.C1>("ITEM");
            Assert.Equal("value1", obj.C1AllorsString);
            Assert.Null(second.Instantiate<Allors.Workspace.Domain.C1>(101));
            obj.C1AllorsString = "edit";
            await second.PullAsync(pull);
            Assert.Equal("edit", obj.C1AllorsString);
            Assert.Equal("value1", ((Allors.Workspace.IUnitDiff)obj.Strategy.Diff().Single()).OriginalRole);
            var conflict = await first.PullAsync(pull);
            Assert.Single(conflict.MergeErrors);
            Assert.Equal(1, obj.Strategy.Version);
            first.Reset();
            Assert.Equal("value1", obj.C1AllorsString);
            await first.PullAsync(pull);
            Assert.Equal("value4", obj.C1AllorsString);
            Assert.Equal("resolved", initial.Inputs.Single().Status);
            await connection.DisposeAsync();
            Assert.Throws<InvalidOperationException>(() => obj.Strategy.CanRead(role));
            Assert.Equal("value4", ((Allors.Workspace.Adapters.Strategy)obj.Strategy).DatabaseOriginState.GetUnitRole(role));
        }

        [Fact]
        public async Task ScopedSessionReportsRawFailureAndRejectsUnsupportedCallsWithoutDispatch()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var calls = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                ++calls;
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                return new ReadFailure { i = request.i, k = "error", e = new[] { new ReadError { c = "read-failed", m = "Failure", o = new long[] { 999 } } } };
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            Assert.Equal(0, calls);
            var result = (Allors.Workspace.Adapters.Remote.ScopedPullResult)await session.PullAsync();
            Assert.True(result.HasErrors);
            Assert.Equal(999, result.ReadErrors.Single().ObjectIds.Single());
            Assert.Empty(result.MissingErrors);
            Assert.Null(session.Instantiate<Allors.Workspace.Domain.C1>(999));
            Assert.Equal(1, calls);
            session.Dispose();
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync());
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync());
        }

        [Fact]
        public async Task ScopedSessionCheckpointsSecurityOnlyChangesAndKeepsUnavailableEdits()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var count = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = SessionReadResponse(request, type.Tag, role.RelationType.Tag);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                ++count;
                response.s[0].v = count;
                if (count == 2)
                {
                    response.p[0].p.ro = Array.Empty<SyncResponseRole>();
                    response.s[0].p.p = Array.Empty<long>();
                    response.pm = Array.Empty<ReadPermission>();
                }
                else if (count == 4)
                {
                    response.l[0].s = "unavailable";
                    response.p = Array.Empty<ReadRecord>();
                    response.s = Array.Empty<ReadSecurity>();
                    response.pm = Array.Empty<ReadPermission>();
                    response.o["item"] = null;
                }
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            var obj = (await session.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("item");
            session.Checkpoint();
            await session.PullAsync(pull);
            Assert.False(obj.Strategy.CanRead(role));
            Assert.Contains(obj.Strategy, session.Checkpoint().AssociationsByRoleType[role]);
            await session.PullAsync(pull);
            Assert.True(obj.Strategy.CanWrite(role));
            obj.C1AllorsString = "edit";
            var unavailable = (Allors.Workspace.Adapters.Remote.ScopedPullResult)await session.PullAsync(pull);
            Assert.False(unavailable.HasErrors);
            Assert.Equal("unavailable", unavailable.Inputs.Single().Status);
            Assert.False(obj.Strategy.CanRead(role));
            Assert.Equal("edit", ((Allors.Workspace.IUnitDiff)obj.Strategy.Diff().Single()).ChangedRole);
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync());
            Assert.Equal(4, count);
            await session.PullAsync(pull);
            Assert.True(obj.Strategy.CanWrite(role));
            Assert.Equal("edit", obj.C1AllorsString);
        }

        [Fact]
        public async Task ScopedSessionCapturesDeclarationsBeforeQueueingAndRejectsExistsParameters()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var requests = new List<ReadRequest>();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                requests.Add(request);
                if (requests.Count == 1) { entered.SetResult(true); await release.Task; }
                var response = SessionReadResponse(request, type.Tag, role.RelationType.Tag);
                response.l = request.p.l.Select((v, i) => new ReadInput { i = i, o = v.o, s = "resolved" }).ToArray();
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var first = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var second = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var blocking = first.PullAsync(new Pull { ObjectType = type, ObjectId = 101 });
            await entered.Task;
            var arguments = new Dictionary<string, object> { ["kind"] = type.Tag };
            var selected = new Pull { Extent = new Filter(type) { Predicate = new Instanceof { Parameter = "kind" } },
                Arguments = arguments, Results = Array.Empty<Allors.Workspace.Data.Result>() };
            var queued = second.PullAsync(selected);
            arguments["kind"] = "changed";
            selected.Results = null;
            release.SetResult(true);
            await blocking;
            await queued;
            Assert.Equal(type.Tag, requests[3].p.l[0].a["kind"].ToString());
            Assert.Empty(requests[3].p.l[0].r);
            Assert.Equal("kind", requests[3].p.l[0].e.p.p);
            await Assert.ThrowsAsync<NotSupportedException>(() => first.PullAsync(new Pull
            { Extent = new Filter(type) { Predicate = new Exists(role) { Parameter = "present" } } }));
            Assert.Equal(new[] { "discover", "read", "discover", "read" }, requests.Select(v => v.k));
        }

        [Fact]
        public async Task ScopedSessionCloseRejectsLocalOperationsAndPermissions()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            using var handler = new CoordinatorHandler(_ => throw new InvalidOperationException("No I/O expected."));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var person = session.Create<Allors.Workspace.Domain.Person>();
            person.FirstName = "before";
            session.Checkpoint();
            session.Dispose();
            Assert.Throws<InvalidOperationException>(() => session.Reset());
            Assert.Throws<InvalidOperationException>(() => session.Checkpoint());
            Assert.Throws<InvalidOperationException>(() => person.FirstName = "after");
            Assert.Equal("before", ((Allors.Workspace.IUnitDiff)person.Strategy.Diff().Single()).ChangedRole);
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedSessionRetirementOrCloseDuringFactoryPreventsApplication(bool close)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            var factory = new ScopedFactory(meta);
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, factory);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            factory.OnCreate = () => { if (close) { session.Dispose(); } else { _ = connection.DisposeAsync(); } };
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync(new Pull { ObjectType = type, ObjectId = 101 }));
            Assert.Null(session.Instantiate<Allors.Workspace.Domain.C1>(101));
            await connection.DisposeAsync();
        }

        [Fact]
        public async Task ScopedSessionKeepsAcceptedAndPreviousCheckpointRecordsIndependently()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var count = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = SessionReadResponse(request, type.Tag, role.RelationType.Tag);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                response.s[0].v = ++count;
                response.p[0].p.ro[0].v = "value" + count;
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            await session.PullAsync(pull);
            session.Checkpoint();
            var store = ScopedStore(connection);
            var first = store.GetCurrentRecord(101);
            await session.PullAsync(pull);
            var second = store.GetCurrentRecord(101);
            Assert.NotSame(first, second);
            Assert.Same(first, store.GetRecord(first));
            Assert.Contains(role, session.Checkpoint().AssociationsByRoleType.Keys);
            Assert.Same(first, store.GetRecord(first));
            await connection.DisposeAsync();
            Assert.Null(store.GetRecord(first));
            Assert.Null(store.GetRecord(second));
            Assert.Equal("value1", first.Roles[role.RelationType.Tag]);
            Assert.Equal("value2", second.Roles[role.RelationType.Tag]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedSessionCheckpointHandlesMixedRecordAndLocalRelations(bool checkpointLocal)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var one = type.RoleTypes.Single(v => v.Name == "C1C1Many2One");
            var many = type.RoleTypes.Single(v => v.Name.StartsWith("C1C1Many2Man", StringComparison.Ordinal));
            var binary = type.RoleTypes.Single(v => v.Name == "C1AllorsBinary");
            var count = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = CoordinatorResponse(request, type.Tag, one.RelationType.Tag);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                response.o["item"] = 101;
                response.s[0].v = ++count;
                response.pm = new[] { one, many, binary }.SelectMany((role, index) => new[]
                {
                    new ReadPermission { i = 201 + index * 2, p = new ReadPermissionPayload { c = type.Tag, o = (long)Operations.Read, t = role.RelationType.Tag } },
                    new ReadPermission { i = 202 + index * 2, p = new ReadPermissionPayload { c = type.Tag, o = (long)Operations.Write, t = role.RelationType.Tag } },
                }).ToArray();
                response.s[0].p.p = response.pm.Select(v => v.i).ToArray();
                response.p[0].p.ro = new[] { new SyncResponseRole { t = one.RelationType.Tag, o = 102 },
                    new SyncResponseRole { t = many.RelationType.Tag, c = new long[] { 102 } }, new SyncResponseRole { t = binary.RelationType.Tag, v = "AQID" } };
                response.p = response.p.Concat(new long[] { 102, 103 }.Select(id => new ReadRecord
                { i = id, c = type.Tag, v = 1, g = new[] { 0 }, r = Array.Empty<int>(), p = new ReadRecordPayload { ro = Array.Empty<SyncResponseRole>() } })).ToArray();
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            var obj = (await session.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("item");
            var bytes = obj.C1AllorsBinary;
            bytes[0] = 99;
            Assert.Equal(new byte[] { 1, 2, 3 }, obj.C1AllorsBinary);
            session.Checkpoint();
            obj.C1C1Many2One = session.Instantiate<Allors.Workspace.Domain.C1>(103);
            obj.AddC1C1Many2Many(session.Instantiate<Allors.Workspace.Domain.C1>(103));
            if (checkpointLocal) { session.Checkpoint(); obj.Strategy.Reset(); }
            Assert.False((await session.PullAsync(pull)).HasErrors);
            var changes = session.Checkpoint();
            Assert.Contains(obj.Strategy, changes.AssociationsByRoleType[one]);
            Assert.Contains(obj.Strategy, changes.AssociationsByRoleType[many]);
            Assert.DoesNotContain(binary, changes.AssociationsByRoleType.Keys);
        }

        [Fact]
        public async Task LocalConnectionRejectsMemoryBeforeCreatingWorkspace()
        {
            using var fixture = new Fixture();
            var database = new Allors.Database.Adapters.Memory.Database(
                new Allors.Database.Configuration.DefaultDatabaseServices(fixture.Engine),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(fixture.M, typeof(Allors.Database.Domain.Person)),
                });
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", meta,
                new Allors.Workspace.Adapters.ReflectionObjectFactory(meta, typeof(Allors.Workspace.Domain.Person)), Array.Empty<Allors.Workspace.Derivations.IRule>());
            var exception = await Assert.ThrowsAsync<NotSupportedException>(() => Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(configuration,
                () => new Allors.Workspace.WorkspaceServices(), database, new LocalReadIdentity(Guid.NewGuid(), 1),
                new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "v1"));
            Assert.Contains("PostgreSQL", exception.Message);
            Assert.Contains("SQL Server", exception.Message);
        }

        [Fact]
        public async Task ScopedSessionLocalFactoryRejectsMemory()
        {
            using var fixture = new Fixture();
            var database = new Allors.Database.Adapters.Memory.Database(
                new Allors.Database.Configuration.DefaultDatabaseServices(fixture.Engine),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(fixture.M, typeof(Allors.Database.Domain.Person)),
                });
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", meta,
                new Allors.Workspace.Adapters.ReflectionObjectFactory(meta, typeof(Allors.Workspace.Domain.Person)), Array.Empty<Allors.Workspace.Derivations.IRule>());
            var exception = await Assert.ThrowsAsync<NotSupportedException>(() => Allors.Workspace.Adapters.Local.DatabaseConnection.CreateAsync(configuration,
                () => new Allors.Workspace.WorkspaceServices(), database,
                new LocalReadIdentity(Guid.NewGuid(), 1), new Allors.Workspace.Adapters.IdGenerator(), new Allors.Ranges.DefaultStructRanges<long>(), "v1"));
            Assert.Contains("SQL", exception.Message);
        }

        // White-box ownership assertion: the scoped public surface deliberately exposes no mutable store.
        private static ReadStore ScopedStore(Allors.Workspace.Adapters.Remote.DatabaseConnection connection)
        {
            var owner = typeof(Allors.Workspace.Adapters.Remote.DatabaseConnection).GetProperty("ScopedOwner", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(connection);
            return (ReadStore)owner.GetType().GetProperty("Store", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(owner);
        }

        private sealed class ScopedFactory : Allors.Workspace.Adapters.ReflectionObjectFactory, Allors.Workspace.IObjectFactory
        {
            internal ScopedFactory(Allors.Workspace.Meta.IMetaPopulation meta) : base(meta, typeof(Allors.Workspace.Domain.Person)) { }
            internal Action OnCreate { get; set; }
            Allors.Workspace.IObject Allors.Workspace.IObjectFactory.Create(Allors.Workspace.IStrategy strategy)
            {
                this.OnCreate?.Invoke();
                return base.Create(strategy);
            }
        }

        private sealed class ScopedConfiguration : Allors.Workspace.Adapters.Configuration
        {
            internal ScopedConfiguration(Allors.Workspace.Meta.IMetaPopulation meta, Allors.Workspace.IObjectFactory factory)
                : base("Default", meta, factory, Array.Empty<Allors.Workspace.Derivations.IRule>()) { }
        }

        private sealed class ScopedTestConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection
        {
            private ScopedTestConnection(Allors.Workspace.Meta.IMetaPopulation meta, Allors.Workspace.IObjectFactory factory)
                : base(new ScopedConfiguration(meta, factory), new Allors.Workspace.Adapters.IdGenerator(), () => new Allors.Workspace.WorkspaceServices(), new Allors.Ranges.DefaultStructRanges<long>()) { }
            internal static async Task<ScopedTestConnection> CreateAsync(Allors.Workspace.Meta.IMetaPopulation meta, System.Net.Http.HttpClient client, Allors.Workspace.IObjectFactory factory, string context = null, bool protectedPush = false, bool protectedInvoke = false)
            {
                var connection = new ScopedTestConnection(meta, factory);
                await connection.InitializeAsync(new BootstrapReadTransport(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), context), "v1", context,
                    protectedPush ? new PushReadClientTransport(async request =>
                    {
                        var response = await connection.PushHandler(new Allors.Protocol.Json.Api.Push.PushRequest { x = request.x, n = request.push.n, o = request.push.o });
                        return response == null ? "null" : JsonSerializer.Serialize(new PushReadResponse
                        {
                            i = request.i, sc = request.sc,
                            push = new PushReadOutcome { k = response.HasErrors ? "rejected" : "committed", n = response.HasErrors ? null : response.n ?? Array.Empty<PushResponseNewObject>(),
                                _e = response._e, _v = response._v, _a = response._a, _m = response._m, _d = response._d },
                        });
                    }) : new Allors.Workspace.Adapters.Remote.SystemText.PushReadTransport(client), protectedInvoke ? new InvokeReadClientTransport(async request => JsonSerializer.Serialize(await connection.InvokeHandler(request))) : new Allors.Workspace.Adapters.Remote.SystemText.InvokeReadTransport(client));
                return connection;
            }
            internal Func<InvokeReadRequest, Task<InvokeReadResponse>> InvokeHandler { get; set; }
            internal Func<Allors.Protocol.Json.Api.Push.PushRequest, Task<Allors.Protocol.Json.Api.Push.PushResponse>> PushHandler { get; set; }
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
        }

        [Fact]
        public async Task ScopedSessionGateOrdersMutationAndRetirementWithoutReplay()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var binary = type.RoleTypes.Single(v => v.Name == "C1AllorsBinary");
            var readEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var readRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pushEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pushRelease = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reads = 0;
            var pushes = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                if (request.k == "discover") { return DiscoveryResponse(SessionReadResponse(request, type.Tag, binary.RelationType.Tag)); }
                if (++reads == 2) { readEntered.SetResult(true); await readRelease.Task; }
                var response = SessionReadResponse(request, type.Tag, binary.RelationType.Tag);
                response.p[0].p.ro[0].v = "AQID";
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), protectedPush: true);
            connection.PushHandler = async request =>
            {
                ++pushes;
                Assert.Equal("BAUG", request.o.Single().r.Single().u?.ToString());
                pushEntered.SetResult(true);
                await pushRelease.Task;
                return new Allors.Protocol.Json.Api.Push.PushResponse();
            };
            using var editing = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var reading = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            var obj = (await editing.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("item");
            var mutable = new byte[] { 4, 5, 6 };
            obj.C1AllorsBinary = mutable;
            var blockedRead = reading.PullAsync(pull);
            await readEntered.Task;
            var queuedPush = editing.PushAsync(new PushOptions { Refresh = false });
            mutable[0] = 99;
            Assert.Equal(0, pushes);
            readRelease.SetResult(true);
            await blockedRead;
            await pushEntered.Task;
            var retiredRead = reading.PullAsync(pull);
            var quiesce = connection.DisposeAsync();
            Assert.False(quiesce.IsCompleted);
            pushRelease.SetResult(true);
            var uncertain = await queuedPush;
            Assert.Equal(MutationOutcome.Committed, uncertain.Outcome);
            Assert.Equal(MutationAcknowledgementStatus.Failed, uncertain.Application.Acknowledgement);
            Assert.True(uncertain.ReconciliationRequired);
            await Assert.ThrowsAsync<InvalidOperationException>(() => retiredRead);
            await quiesce;
            Assert.Equal(2, reads);
            Assert.Equal(1, pushes);
            Assert.Single(obj.Strategy.Diff());
        }

        [Fact]
        public async Task ScopedSessionCancelledQueuedMutationDoesNotDispatchAndGateRecovers()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var reads = 0;
            var pushes = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                if (request.k == "discover") { return DiscoveryResponse(SessionReadResponse(request, type.Tag, role.RelationType.Tag)); }
                if (++reads == 2) { entered.SetResult(true); await release.Task; }
                return SessionReadResponse(request, type.Tag, role.RelationType.Tag);
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), protectedPush: true);
            connection.PushHandler = _ => { ++pushes; return Task.FromResult(new Allors.Protocol.Json.Api.Push.PushResponse()); };
            using var first = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var second = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            await first.PullAsync(pull);
            var pending = second.PullAsync(pull);
            await entered.Task;
            using var cancellation = new System.Threading.CancellationTokenSource();
            var cancelled = first.PushAsync(new PushOptions { Refresh = false }, cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);
            Assert.Equal(0, pushes);
            release.SetResult(true);
            await pending;
            Assert.False((await first.PushAsync(new PushOptions { Refresh = false })).HasErrors);
            Assert.Equal(1, pushes);
        }

        [Fact]
        public async Task ScopedSessionRetiredFailureKeepsStructuredDiagnostic()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                return new ReadFailure { i = request.i, k = "error", sc = new ReadScope { p = "other", u = 1, w = "Default", r = "other" },
                    e = new[] { new ReadError { c = "scope-mismatch", m = "Scope changed", o = new long[] { 999 } } } };
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var retired = await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync());
            var failure = Assert.IsType<Allors.Workspace.Adapters.Remote.ReadOperationException>(retired.InnerException);
            Assert.Equal("scope-mismatch", failure.Errors.Single().Code);
            Assert.Equal(999, failure.Errors.Single().ObjectIds.Single());
            Assert.Equal("other", failure.Scope.Revision);
        }

        [Fact]
        public async Task ScopedSessionRejectsMethodFromAnotherSessionBeforeDispatch()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), protectedInvoke: true);
            var invoked = 0;
            connection.InvokeHandler = request => { ++invoked; return Task.FromResult(new InvokeReadResponse { i = request.i, sc = request.sc, invoke = new InvokeReadOutcome { k = "committed" } }); };
            using var owner = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var foreign = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var pull = new Pull { ObjectType = type, ObjectId = 101 };
            await owner.PullAsync(pull);
            var obj = (await foreign.PullAsync(pull)).GetObject<Allors.Workspace.Domain.C1>("item");
            foreign.Dispose();
            await Assert.ThrowsAsync<ArgumentException>(() => owner.InvokeAsync(new Allors.Workspace.Method(obj, type.MethodTypes.First())));
            Assert.Equal(0, invoked);
        }

        [Fact]
        public async Task ScopedSessionUsesCurrentMembershipAndSharedBodiesDuringConflictsAndQueryOmission()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            var count = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = SessionReadResponse(request, type.Tag, role.RelationType.Tag);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                ++count;
                var other = count == 3 || count == 5;
                response.p[0].i = other ? 102 : 101;
                response.p[0].v = count;
                response.p[0].p.ro[0].v = count == 1 ? "baseline" : "remote";
                response.o["item"] = response.p[0].i;
                response.l[0].o = request.p.l[0].o;
                if (count > 1)
                {
                    response.s[0].i = 12;
                    response.s[0].v = count == 2 ? 1 : 2;
                    response.s[0].p.p = count == 2 ? new long[] { 202 } : new long[] { 201, 202 };
                    response.pm = response.pm.Where(v => response.s[0].p.p.Contains(v.i)).ToArray();
                }
                if (count >= 3)
                {
                    response.p[0].r = new[] { 1 };
                    response.s = response.s.Concat(new[] { new ReadSecurity { k = "revocation", i = 13, v = count == 5 ? 2 : 1,
                        p = new ReadSecurityPayload { p = count == 5 ? Array.Empty<long>() : new long[] { 202 } } } }).ToArray();
                }
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var target = new Pull { ObjectType = type, ObjectId = 101 };
            var query = new Pull { Extent = new Filter(type) };
            var obj = (await session.PullAsync(target)).GetObject<Allors.Workspace.Domain.C1>("item");
            obj.C1AllorsString = "edit";
            Assert.Single((await session.PullAsync(target)).MergeErrors);
            Assert.False(obj.Strategy.CanRead(role));
            Assert.True(obj.Strategy.CanWrite(role));
            var omitted = await session.PullAsync(query);
            Assert.Equal(102, omitted.GetObject<Allors.Workspace.Domain.C1>("item").Id);
            Assert.True(obj.Strategy.CanRead(role));
            Assert.True(obj.Strategy.CanWrite(role));
            Assert.Equal("edit", obj.C1AllorsString);
            Assert.Single((await session.PullAsync(target)).MergeErrors);
            Assert.False(obj.Strategy.CanWrite(role));
            await session.PullAsync(query);
            Assert.True(obj.Strategy.CanWrite(role));
            Assert.Equal(1, obj.Strategy.Version);
            Assert.Equal("baseline", ((Allors.Workspace.IUnitDiff)obj.Strategy.Diff().Single()).OriginalRole);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedSessionPostPublicationCancellationOrFactoryFailurePreservesBaselines(bool fail)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            var factory = new ScopedFactory(meta);
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, factory);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var cancellation = new System.Threading.CancellationTokenSource();
            factory.OnCreate = () => { if (fail) { throw new ApplicationException("Factory failed"); } cancellation.Cancel(); };
            var pending = session.PullAsync(cancellation.Token, new Pull { ObjectType = type, ObjectId = 101 });
            if (fail)
            {
                await Assert.ThrowsAsync<ApplicationException>(() => pending);
                Assert.Null(session.Instantiate<Allors.Workspace.Domain.C1>(101));
                await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync());
            }
            else
            {
                Assert.False((await pending).HasErrors);
                Assert.Equal("value", session.Instantiate<Allors.Workspace.Domain.C1>(101).C1AllorsString);
            }
            Assert.NotNull(ScopedStore(connection).GetCurrentRecord(101));
            await connection.DisposeAsync();
            session.Dispose();
        }

        [Fact]
        public async Task ScopedSessionUnloadedRelationRequiresItsOwnAcceptedTarget()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1C1Many2One");
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var id = request.p.l[0].o.Value;
                var response = SessionReadResponse(request, type.Tag, role.RelationType.Tag);
                response.p[0].i = id;
                response.l[0].o = id;
                response.o["item"] = id;
                response.p[0].p.ro = id == 101 ? new[] { new SyncResponseRole { t = role.RelationType.Tag, o = 102 } } : Array.Empty<SyncResponseRole>();
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var first = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            using var second = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var obj = (await first.PullAsync(new Pull { ObjectType = type, ObjectId = 101 })).GetObject<Allors.Workspace.Domain.C1>("item");
            await second.PullAsync(new Pull { ObjectType = type, ObjectId = 102 });
            Assert.Null(first.Instantiate<Allors.Workspace.Domain.C1>(102));
            var missing = Assert.Throws<Exception>(() => obj.C1C1Many2One);
            Assert.Contains("Pull", missing.Message);
            var target = (await first.PullAsync(new Pull { ObjectType = type, ObjectId = 102 })).GetObject<Allors.Workspace.Domain.C1>("item");
            Assert.Same(target, obj.C1C1Many2One);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ScopedSessionClosedRejectsSessionOriginCollectionMutations(bool remove)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            using var handler = new CoordinatorHandler(_ => throw new InvalidOperationException("No I/O expected."));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var source = session.Create<Allors.Workspace.Domain.C1>();
            var target = session.Create<Allors.Workspace.Domain.C1>();
            if (remove) { source.AddSessionC1Many2Many(target); }
            session.Dispose();
            Assert.Throws<InvalidOperationException>(() =>
            {
                if (remove) { source.RemoveSessionC1Many2Many(target); }
                else { source.AddSessionC1Many2Many(target); }
            });
        }

        [Theory]
        [InlineData("null")]
        [InlineData("missing")]
        [InlineData("null-row")]
        [InlineData("unknown-workspace")]
        [InlineData("duplicate-workspace")]
        [InlineData("zero-database")]
        [InlineData("duplicate-database")]
        [InlineData("existing-database")]
        [InlineData("null-derivation")]
        public async Task ScopedSessionMalformedPushCannotPartiallyRemapOrReplay(string invalid)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), protectedPush: true);
            var attempts = 0;
            connection.PushHandler = request =>
            {
                ++attempts;
                var response = new Allors.Protocol.Json.Api.Push.PushResponse
                { n = request.n.Select((v, i) => new Allors.Protocol.Json.Api.Push.PushResponseNewObject { w = v.w, d = 301 + i }).ToArray() };
                switch (invalid)
                {
                    case "null": response = null; break;
                    case "missing": response.n = null; break;
                    case "null-row": response.n[1] = null; break;
                    case "unknown-workspace": response.n[1].w = -999; break;
                    case "duplicate-workspace": response.n[1].w = response.n[0].w; break;
                    case "zero-database": response.n[1].d = 0; break;
                    case "duplicate-database": response.n[1].d = response.n[0].d; break;
                    case "existing-database": response.n[1].d = 101; break;
                    case "null-derivation": response._d = new Allors.Protocol.Json.Api.ResponseDerivationError[] { null }; break;
                }
                return Task.FromResult(response);
            };
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            await session.PullAsync(new Pull { ObjectType = type, ObjectId = 101 });
            var first = session.Create<Allors.Workspace.Domain.C1>();
            var second = session.Create<Allors.Workspace.Domain.C1>();
            first.C1AllorsString = "local";
            var firstId = first.Id;
            var secondId = second.Id;
            var failure = await session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(invalid == "existing-database" ? MutationOutcome.Committed : MutationOutcome.Unknown, failure.Outcome);
            Assert.True(failure.ReconciliationRequired);
            Assert.NotNull(invalid == "existing-database" ? failure.Application.Errors.Single().Cause : failure.Diagnostics.Errors.Single().Cause);
            Assert.Equal(firstId, first.Id);
            Assert.Equal(secondId, second.Id);
            Assert.Equal(2, session.PushToDatabaseTracker.Created.Count);
            Assert.Equal("local", ((Allors.Workspace.IUnitDiff)first.Strategy.Diff().Single()).ChangedRole);
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.PushAsync(new PushOptions { Refresh = false }));
            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task ScopedSessionNullInvokeReportsUncertainOutcomeBeforeReturningResult()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), protectedInvoke: true);
            var attempts = 0;
            connection.InvokeHandler = _ => { ++attempts; return Task.FromResult<InvokeReadResponse>(null); };
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var obj = (await session.PullAsync(new Pull { ObjectType = type, ObjectId = 101 })).GetObject<Allors.Workspace.Domain.C1>("item");
            var failure = await session.InvokeAsync(new Allors.Workspace.Method(obj, type.MethodTypes.First()));
            Assert.Equal(MutationOutcome.Unknown, failure.Outcome);
            Assert.Contains("may have committed", failure.ErrorMessage);
            Assert.NotNull(Assert.Single(failure.Diagnostics.Errors).Cause);
            Assert.True(failure.ReconciliationRequired);
            await Assert.ThrowsAsync<InvalidOperationException>(() => session.InvokeAsync(new Allors.Workspace.Method(obj, type.MethodTypes.First())));
            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task ScopedSessionPushPreservesNegativeAccessErrorsAndAppliesWithoutTrackerCallbacks()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), protectedPush: true);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            await session.PullAsync(new Pull { ObjectType = type, ObjectId = 101 });
            var created = session.Create<Allors.Workspace.Domain.C1>();
            connection.PushHandler = request => Task.FromResult(new Allors.Protocol.Json.Api.Push.PushResponse { _a = new[] { request.n.Single().w } });
            var denied = await session.PushAsync(new PushOptions { Refresh = false });
            Assert.True(denied.HasErrors);
            Assert.Same(created, Assert.Single(denied.AccessErrors));
            Assert.True(created.Id < 0);
            connection.PushHandler = request => Task.FromResult(new Allors.Protocol.Json.Api.Push.PushResponse
            { n = new[] { new Allors.Protocol.Json.Api.Push.PushResponseNewObject { w = request.n.Single().w, d = 301 } } });
            session.PushToDatabaseTracker.Created = new NoRemovalCallbacks(session.PushToDatabaseTracker.Created);
            Assert.False((await session.PushAsync(new PushOptions { Refresh = false })).HasErrors);
            Assert.Equal(301, created.Id);
            Assert.Null(session.PushToDatabaseTracker.Created);
        }

        private sealed class NoRemovalCallbacks : HashSet<Allors.Workspace.Adapters.Strategy>, ISet<Allors.Workspace.Adapters.Strategy>
        {
            internal NoRemovalCallbacks(IEnumerable<Allors.Workspace.Adapters.Strategy> values) : base(values) { }
            bool ICollection<Allors.Workspace.Adapters.Strategy>.Remove(Allors.Workspace.Adapters.Strategy value) =>
                throw new InvalidOperationException("Host collection callbacks must not run during scoped response application.");
        }

        [Fact]
        public async Task ScopedSessionPullUsesStagedBuiltInTrackersDuringApplication()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message => SessionReadResponse(
                Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync()), type.Tag, role.RelationType.Tag));
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ReadyConnectionAsync(meta, client);
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            session.ChangeSetTracker.Instantiated = new NoAddCallbacks<Allors.Workspace.IStrategy>();
            session.ChangeSetTracker.DatabaseOriginStates = new NoAddCallbacks<Allors.Workspace.Adapters.DatabaseOriginState>();
            Assert.False((await session.PullAsync(new Pull { ObjectType = type, ObjectId = 101 })).HasErrors);
            Assert.Single(session.ChangeSetTracker.Instantiated);
            Assert.Single(session.ChangeSetTracker.DatabaseOriginStates);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("configured-context")]
        [InlineData("")]
        public async Task ScopedSessionMutationsCaptureConfiguredContext(string context)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString");
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                Assert.Equal(context, request.x);
                var response = SessionReadResponse(request, type.Tag, role.RelationType.Tag);
                response.sc.x = context;
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var connection = await ScopedTestConnection.CreateAsync(meta, client, new ScopedFactory(meta), context, protectedPush: true, protectedInvoke: true);
            string pushedContext = null;
            string invokedContext = null;
            connection.PushHandler = request =>
            {
                pushedContext = request.x;
                return Task.FromResult(new Allors.Protocol.Json.Api.Push.PushResponse());
            };
            connection.InvokeHandler = request =>
            {
                invokedContext = request.x;
                Assert.Equal(context, request.sc.x);
                return Task.FromResult(new InvokeReadResponse { i = request.i, sc = request.sc, invoke = new InvokeReadOutcome { k = "committed" } });
            };
            using var session = (Allors.Workspace.Adapters.Remote.Session)connection.CreateWorkspace().CreateSession();
            var obj = (await session.PullAsync(new Pull { ObjectType = type, ObjectId = 101 })).GetObject<Allors.Workspace.Domain.C1>("item");
            Assert.False((await session.PushAsync(new PushOptions { Refresh = false })).HasErrors);
            Assert.False((await session.InvokeAsync(new Allors.Workspace.Method(obj, type.MethodTypes.First()), new Allors.Workspace.InvokeOptions { Refresh = false })).HasErrors);
            Assert.Equal(context, pushedContext);
            Assert.Equal(context, invokedContext);
        }

        private sealed class NoAddCallbacks<T> : HashSet<T>, ISet<T>
        {
            bool ISet<T>.Add(T value) => throw new InvalidOperationException("Host collection callbacks must not run during scoped response application.");
        }

        private static ReadSuccess SessionReadResponse(ReadRequest request, string classTag, string roleTag)
        {
            var response = CoordinatorResponse(request, classTag, roleTag);
            response.o["item"] = 101;
            response.s[0].p.p = new long[] { 201, 202, 203 };
            response.pm = new[]
            {
                new ReadPermission { i = 201, p = new ReadPermissionPayload { c = classTag, o = (long)Operations.Read, t = roleTag } },
                new ReadPermission { i = 202, p = new ReadPermissionPayload { c = classTag, o = (long)Operations.Write, t = roleTag } },
                new ReadPermission { i = 203, p = new ReadPermissionPayload { c = classTag, o = (long)Operations.Create } },
            };
            return response;
        }

        private static Task<Allors.Workspace.Adapters.Remote.DatabaseConnection> ReadyConnectionAsync(Allors.Workspace.Meta.IMetaPopulation meta, System.Net.Http.HttpClient client) =>
            PushReadClientConnectionAsync(meta, client, false);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ReadCoordinatorAssemblesExactReuseAndKeepsCompletedValuesAfterClose(bool newtonsoft)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var requests = new List<ReadRequest>();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                requests.Add(request);
                var response = CoordinatorResponse(request, type.Tag, role);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                if (requests.Count == 4)
                {
                    Assert.Single(request.h.r);
                    Assert.True(request.h.s.Single().h);
                    Assert.Single(request.h.p);
                    response.p[0].p = new ReadRecordPayload { r = 0 };
                    response.s[0].p = new ReadSecurityPayload { r = 0 };
                    response.pm[0].p = new ReadPermissionPayload { r = 0 };
                }

                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            Allors.Workspace.Adapters.Remote.IReadTransport transport = newtonsoft
                ? new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(client)
                : new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client);
            await using var coordinator = await ReadTestConnection.CreateAsync(transport, meta, "Default", "v1");
            var first = await coordinator.Read(CoordinatorRequest(type.Tag));
            var second = await coordinator.Read(CoordinatorRequest(type.Tag));
            Assert.Equal(new[] { "discover", "read", "discover", "read" }, requests.Select(v => v.k));
            Assert.Same(first.Records.Single(), second.Records.Single());
            Assert.Equal("value", second.Records.Single().Roles[role]);
            Assert.Empty(second.Collections["empty"]);
            Assert.Null(second.Objects["missing"]);
            await coordinator.DisposeAsync();
            Assert.Null(coordinator.Store.GetRecord(first.Records.Single()));
            Assert.Equal("value", second.Records.Single().Roles[role]);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("role")]
        [InlineData("unit")]
        [InlineData("permission")]
        [InlineData("closure")]
        [InlineData("conflict")]
        [InlineData("scope")]
        public async Task ReadCoordinatorRejectsInvalidCompletionWithoutPublishing(string invalid)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var count = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = CoordinatorResponse(request, type.Tag, role);
                if (request.k == "discover") { return DiscoveryResponse(response); }
                if (++count == 2)
                {
                    response.p[0].v = 2;
                    switch (invalid)
                    {
                        case "class": response.p[0].c = "unknown"; break;
                        case "role": response.p[0].p.ro[0].t = "unknown"; break;
                        case "unit": response.p[0].p.ro[0].v = true; break;
                        case "permission": response.pm[0].p.c = "unknown"; break;
                        case "closure": response.s[0].p = new ReadSecurityPayload { r = 0 }; response.pm = Array.Empty<ReadPermission>(); break;
                        case "scope": response.sc.r = "v2"; break;
                        case "conflict": response.pm[0].p.o = (long)Operations.Read; response.pm[0].p.t = role; break;
                    }
                }

                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var first = await coordinator.Read(CoordinatorRequest(type.Tag));
            await Assert.ThrowsAnyAsync<Exception>(() => coordinator.Read(CoordinatorRequest(type.Tag)));
            Assert.Same(first.Records.Single(), coordinator.Store.GetCurrentRecord(101));
            Assert.Null(coordinator.Store.GetRecord(101, type.Tag, 2, new[] { new ReadSecurityKey("grant", 11, 1) }, Array.Empty<ReadSecurityKey>()));
        }

        [Theory]
        [InlineData(false, "discover")]
        [InlineData(true, "discover")]
        [InlineData(false, "read")]
        [InlineData(true, "read")]
        public async Task ReadCoordinatorCapturesQueuedInputsAndRejectsCancelledOrRetiredAttempts(bool retire, string phase)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var attempts = 0;
            using var cancellation = new System.Threading.CancellationTokenSource();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                ++attempts;
                if (request.k == phase) { entered.TrySetResult(true); await release.Task; }
                Assert.Equal(101, request.p.l[0].o);
                return CoordinatorResponse(request, type.Tag, role);
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var request = CoordinatorRequest(type.Tag);
            var first = coordinator.Read(request, cancellationToken: cancellation.Token);
            await entered.Task;
            var second = coordinator.Read(request, cancellationToken: cancellation.Token);
            request.p.l[0].o = 999;
            Assert.Equal(phase == "discover" ? 1 : 2, attempts);
            if (retire) { _ = coordinator.DisposeAsync(); } else { cancellation.Cancel(); }
            release.SetResult(true);
            await Assert.ThrowsAnyAsync<Exception>(async () => await first);
            await Assert.ThrowsAnyAsync<Exception>(async () => await second);
            Assert.Null(coordinator.Store.GetCurrentRecord(101));
            Assert.Equal(phase == "discover" ? 1 : 2, attempts);

            var dispatches = new List<int>();
            foreach (var callback in new[] { "size", "correlation-capture", "outgoing-capture" })
            {
                using var beforeDispatch = new System.Threading.CancellationTokenSource();
                var controlled = new CallbackReadTransport(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client));
                await using var owner = await ReadTestConnection.CreateAsync(controlled, meta, "Default", "v1");
                var sizes = 0;
                var captures = 0;
                var armed = false;
                var invalidated = false;
                void Invalidate()
                {
                    invalidated = true;
                    if (retire) { _ = owner.DisposeAsync(); }
                    else { beforeDispatch.Cancel(); }
                }
                controlled.AfterRequestSize = value =>
                {
                    if (value.k == phase && value.sc != null && ++sizes == (phase == "read" ? 2 : 1))
                    {
                        armed = true;
                        if (callback == "size") { Invalidate(); }
                    }
                };
                controlled.AfterCapture = value =>
                {
                    if (armed && value.k == phase && ++captures == (callback == "outgoing-capture" ? 2 : 1) && callback != "size")
                        Invalidate();
                };
                await Assert.ThrowsAnyAsync<Exception>(() => owner.Read(CoordinatorRequest(type.Tag), beforeDispatch.Token));
                Assert.True(invalidated);
                dispatches.Add(controlled.Dispatches);
                Assert.Null(owner.Store.GetCurrentRecord(101));
            }
            Assert.Equal(Enumerable.Repeat(phase == "discover" ? 0 : 1, 3), dispatches);
        }

        [Theory]
        [InlineData("cold", false)]
        [InlineData("empty", false)]
        [InlineData("cached", false)]
        [InlineData("cold", true)]
        [InlineData("empty", true)]
        [InlineData("cached", true)]
        public async Task ReadCoordinatorAlwaysDiscoversBeforeCompleting(string mode, bool newtonsoft)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var store = new ReadStore(CreateStoreScope());
            var key = new ReadSecurityKey("grant", 11, 1);
            ReadStoredRecord held = null;
            Allors.Workspace.Adapters.Remote.ReadSnapshot initial = null;
            if (mode == "cached")
            {
                held = new ReadStoredRecord(store.Scope, 101, type.Tag, 1, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { [role] = "value" });
                initial = store.ImportAndPublish(new[] { held }, new[] { new ReadStoredSecurity(store.Scope, key, new long[] { 201 }) },
                    new[] { new ReadStoredPermission(store.Scope, 201, type.Tag, Operations.Create) });
            }
            var requests = new List<ReadRequest>();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                requests.Add(request);
                var response = CoordinatorResponse(request, type.Tag, role);
                if (mode == "empty")
                {
                    response.l = Array.Empty<ReadInput>();
                    response.p = Array.Empty<ReadRecord>();
                    response.s = Array.Empty<ReadSecurity>();
                    response.pm = Array.Empty<ReadPermission>();
                    response.c.Clear(); response.o.Clear();
                }
                if (request.k == "discover")
                {
                    Assert.Null(request.h);
                    return DiscoveryResponse(response);
                }
                if (mode == "cached")
                {
                    Assert.Single(request.h.r);
                    Assert.True(request.h.s.Single().h);
                    Assert.Single(request.h.p);
                    response.p[0].p = new ReadRecordPayload { r = 0 };
                    response.s[0].p = new ReadSecurityPayload { r = 0 };
                    response.pm[0].p = new ReadPermissionPayload { r = 0 };
                }
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            Allors.Workspace.Adapters.Remote.IReadTransport transport = newtonsoft
                ? new Allors.Workspace.Adapters.Remote.Newtonsoft.ReadTransport(client)
                : new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client);
            await using var coordinator = await ReadTestConnection.CreateAsync(transport, meta, "Default", "v1");
            if (initial != null) coordinator.Store.ImportAndPublish(initial.Records, initial.Security, initial.Permissions);
            var original = CoordinatorRequest(type.Tag);
            if (mode == "empty") { original.p.l = Array.Empty<Allors.Protocol.Json.Data.Pull>(); }
            var result = await coordinator.Read(original);
            Assert.Equal(new[] { "discover", "read" }, requests.Select(v => v.k));
            Assert.Equal(JsonSerializer.Serialize(original.p), JsonSerializer.Serialize(requests[0].p));
            Assert.Equal(JsonSerializer.Serialize(original.p), JsonSerializer.Serialize(requests[1].p));
            if (mode == "empty") { Assert.Empty(result.Records); }
            else { Assert.Single(result.Records); }
            if (mode == "cached") { Assert.Same(held, result.Records.Single()); }
        }

        [Theory]
        [InlineData("membership")]
        [InlineData("version")]
        [InlineData("security")]
        public async Task ReadCoordinatorCompletesChangedDiscoveryWithoutRestarting(string change)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var requests = new List<ReadRequest>();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                requests.Add(request);
                var response = CoordinatorResponse(request, type.Tag, role);
                response.l[0].o = null;
                if (request.k == "discover") { return DiscoveryResponse(response); }
                if (change == "membership") { response.p[0].i = 102; }
                if (change == "version") { response.p[0].v = 2; response.p[0].p.ro[0].v = "changed"; }
                if (change == "security") { response.s[0].v = 2; response.s[0].p.p = new long[] { 202 }; response.pm[0].i = 202; }
                response.c["found"] = new[] { response.p[0].i };
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var original = CoordinatorRequest(type.Tag);
            original.p.l[0].o = null;
            var result = await coordinator.Read(original);
            Assert.Equal(new[] { "discover", "read" }, requests.Select(v => v.k));
            Assert.Equal(JsonSerializer.Serialize(requests[0].p), JsonSerializer.Serialize(requests[1].p));
            Assert.Equal(change == "membership" ? 102 : 101, result.Records.Single().Id);
            Assert.Equal(change == "version" ? 2 : 1, result.Records.Single().DataVersion);
            Assert.Equal(change == "security" ? 2 : 1, result.Security.Single().Key.Version);
            Assert.Equal(change == "security" ? 202 : 201, result.Permissions.Single().Id);
            Assert.Equal(result.Records.Single().Id, result.Collections["found"].Single());
        }

        [Fact]
        public async Task ReadCoordinatorDiscoveryRepeatsTheOriginalSelectionAndBoundsHints()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var requests = new List<ReadRequest>();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                requests.Add(request);
                var response = CoordinatorResponse(request, type.Tag, role);
                if (request.k == "discover")
                {
                    return new DiscoverySuccess { i = request.i, k = "discovery", sc = response.sc,
                        p = Enumerable.Range(101, 300).Select(id => new ReadRecordHeader { i = id, c = type.Tag, v = 1, g = new[] { 0 }, r = Array.Empty<int>() }).ToArray(),
                        s = new[] { new ReadSecurityHeader { k = "grant", i = 11, v = 1 } } };
                }

                Assert.Equal(101, request.p.l.Single().o);
                Assert.Equal(256, request.h.r.Length + request.h.s.Length + request.h.p.Length);
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var store = coordinator.Store;
            var key = new ReadSecurityKey("grant", 11, 1);
            store.ImportAndPublish(Enumerable.Range(101, 300).Select(id => new ReadStoredRecord(store.Scope, id, type.Tag, 1,
                    new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { [role] = "value" })),
                new[] { new ReadStoredSecurity(store.Scope, key, new long[] { 201 }) },
                new[] { new ReadStoredPermission(store.Scope, 201, type.Tag, Operations.Create) });
            var result = await coordinator.Read(CoordinatorRequest(type.Tag));
            Assert.Equal(new[] { "discover", "read" }, requests.Select(v => v.k));
            Assert.Single(result.Records);
        }

        [Fact]
        public async Task ReadCoordinatorReusesHistoricalRecordsFromConnectionCache()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                if (request.k == "discover") return DiscoveryResponse(CoordinatorResponse(request, type.Tag, role));
                Assert.Equal(new long[] { 2, 1 }, request.h.r.Select(value => value.v));
                Assert.All(request.h.s, value => Assert.True(value.h));
                var response = CoordinatorResponse(request, type.Tag, role);
                response.p[0].p = new ReadRecordPayload { r = 1 };
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var store = coordinator.Store;
            var oldKey = new ReadSecurityKey("grant", 11, 1);
            var newKey = new ReadSecurityKey("grant", 11, 2);
            var old = new ReadStoredRecord(store.Scope, 101, type.Tag, 1, new[] { oldKey }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { [role] = "old" });
            var recent = new ReadStoredRecord(store.Scope, 101, type.Tag, 2, new[] { newKey }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { [role] = "recent" });
            var permission = new ReadStoredPermission(store.Scope, 201, type.Tag, Operations.Create);
            store.ImportAndPublish(new[] { old }, new[] { new ReadStoredSecurity(store.Scope, oldKey, new long[] { 201 }) }, new[] { permission });
            store.ImportAndPublish(new[] { recent }, new[] { new ReadStoredSecurity(store.Scope, newKey, new long[] { 201 }) }, new[] { permission });
            var result = await coordinator.Read(CoordinatorRequest(type.Tag));
            Assert.Same(old, result.Records.Single());
            Assert.Equal("old", result.Records.Single().Roles[role]);
            Assert.Equal(oldKey, result.Security.Single().Key);
        }

        [Fact]
        public async Task ReadCoordinatorSerializesSuccessfulReadsAndCapturesQueuedDeclarations()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var attempts = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var order = ++attempts;
                entered.TrySetResult(true);
                await release.Task;
                Assert.Equal(101, request.p.l[0].o);
                var response = CoordinatorResponse(request, type.Tag, role);
                response.p[0].v = order;
                response.p[0].p.ro[0].v = order.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var first = coordinator.Read(CoordinatorRequest(type.Tag));
            await entered.Task;
            var request = CoordinatorRequest(type.Tag);
            var second = coordinator.Read(request);
            request.p.l[0].o = 999;
            Assert.Equal(1, attempts);
            release.SetResult(true);
            var firstResult = await first;
            var secondResult = await second;
            Assert.Equal("2", firstResult.Records.Single().Roles[role]);
            Assert.Equal("4", secondResult.Records.Single().Roles[role]);
            Assert.Equal(4, attempts);
            Assert.Same(secondResult.Records.Single(), coordinator.Store.GetCurrentRecord(101));
        }

        [Fact]
        public async Task ReadCoordinatorDropsHintsAtByteBoundAndRejectsOversizedOriginal()
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.RoleTypes.Single(v => v.Name == "C1AllorsString").RelationType.Tag;
            var store = new ReadStore(CreateStoreScope());
            var key = new ReadSecurityKey("grant", 11, 1);
            var record = new ReadStoredRecord(store.Scope, 101, type.Tag, 1, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { [role] = "value" });
            var current = store.ImportAndPublish(new[] { record }, new[] { new ReadStoredSecurity(store.Scope, key, new long[] { 201 }) }, new[] { new ReadStoredPermission(store.Scope, 201, type.Tag, Operations.Create) });
            var count = 0;
            using var handler = new CoordinatorHandler(async message =>
            {
                ++count;
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                Assert.Null(request.h);
                return CoordinatorResponse(request, type.Tag, role);
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            var transport = new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client);
            var request = CoordinatorRequest(type.Tag);
            request.sc = new ReadScope { p = "population-1", u = 1, w = "Default", r = "v1" };
            await using var coordinator = await ReadTestConnection.CreateAsync(transport, meta, "Default", "v1");
            coordinator.Store.ImportAndPublish(current.Records, current.Security, current.Permissions);
            request.i = new string('x', 1024 * 1024 - transport.RequestByteCount(request) - "discover".Length + "read".Length + request.i.Length);
            var result = await coordinator.Read(request);
            request.i = new string('x', 1024 * 1024);
            await Assert.ThrowsAsync<FormatException>(() => coordinator.Read(request));
            Assert.Equal(2, count);
        }

        [Theory]
        [InlineData("discover")]
        [InlineData("read")]
        public async Task ReadCoordinatorPreservesStructuredFailuresWithoutBindingColdScope(string phase)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var requests = new List<ReadRequest>();
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                requests.Add(request);
                if (request.k != phase)
                {
                    return new DiscoverySuccess { i = request.i, k = "discovery", sc = new ReadScope { p = "population-1", u = 1, w = "Default", r = "v1" },
                        p = Array.Empty<ReadRecordHeader>(), s = Array.Empty<ReadSecurityHeader>() };
                }
                return new ReadFailure { i = request.i, k = "error", e = new[] { new ReadError { c = "unavailable", m = "Select another object.", i = request.p.l.Length == 0 ? null : 0, o = new long[] { 101 } } } };
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            ReadTestConnection captured = null;
            var failure = await Assert.ThrowsAsync<Allors.Workspace.Adapters.Remote.ReadOperationException>(() => ReadTestConnection.CreateAsync(
                new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1", bootstrap: true, capture: value => captured = value));
            Assert.Equal(requests.Last().i, failure.RequestId);
            Assert.Null(failure.Scope);
            Assert.Equal("unavailable", failure.Errors.Single().Code);
            Assert.Null(failure.Errors.Single().SelectionIndex);
            Assert.Equal(new long[] { 101 }, failure.Errors.Single().ObjectIds);
            Assert.Null(captured.Store);
            Assert.Throws<ObjectDisposedException>(() => captured.CreateWorkspace());
            Assert.Equal(phase == "discover" ? new[] { "discover" } : new[] { "discover", "read" }, requests.Select(v => v.k));

            requests.Clear();
            await using var ready = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            var selected = await Assert.ThrowsAsync<Allors.Workspace.Adapters.Remote.ReadOperationException>(() => ready.Read(CoordinatorRequest(type.Tag)));
            Assert.Equal("coordinator", selected.RequestId);
            Assert.Equal("unavailable", selected.Errors.Single().Code);
            Assert.Equal("Select another object.", selected.Errors.Single().Message);
            Assert.Equal(0, selected.Errors.Single().SelectionIndex);
            Assert.Equal(new long[] { 101 }, selected.Errors.Single().ObjectIds);
            Assert.Null(ready.Store.GetCurrentRecord(101));
            Assert.Equal(phase == "discover" ? new[] { "discover" } : new[] { "discover", "read" }, requests.Select(v => v.k));
        }

        [Theory]
        [InlineData(UnitTags.Binary, "AA ==")]
        [InlineData(UnitTags.DateTime, "2026-10-12")]
        [InlineData(UnitTags.Decimal, 1)]
        [InlineData(UnitTags.Decimal, "0.12345678901234567890123456789")]
        [InlineData(UnitTags.Decimal, "7922816251426433759354395033.6")]
        public async Task ReadCoordinatorRejectsUnitEncodingsOutsideTheConfiguredRepresentation(string unitTag, object encoded)
        {
            var meta = new Allors.Workspace.Meta.Lazy.MetaBuilder().Build();
            var type = meta.FindByName("C1");
            var role = type.DatabaseOriginRoleTypes.First(v => v.ObjectType.Tag == unitTag).RelationType.Tag;
            using var handler = new CoordinatorHandler(async message =>
            {
                var request = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await message.Content.ReadAsStringAsync());
                var response = CoordinatorResponse(request, type.Tag, role);
                response.p[0].p.ro[0].v = encoded;
                return response;
            });
            using var client = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("http://localhost/allors/") };
            await using var coordinator = await ReadTestConnection.CreateAsync(new Allors.Workspace.Adapters.Remote.SystemText.ReadTransport(client), meta, "Default", "v1");
            await Assert.ThrowsAsync<FormatException>(() => coordinator.Read(CoordinatorRequest(type.Tag)));
            Assert.Null(coordinator.Store.GetCurrentRecord(101));
        }

        private sealed class CallbackReadTransport : Allors.Workspace.Adapters.Remote.IReadTransport
        {
            private readonly Allors.Workspace.Adapters.Remote.IReadTransport inner;
            internal CallbackReadTransport(Allors.Workspace.Adapters.Remote.IReadTransport inner) => this.inner = inner;
            internal Action<ReadRequest> AfterCapture { get; set; }
            internal Action<ReadRequest> AfterRequestSize { get; set; }
            internal int Dispatches { get; private set; }
            public ReadRequest CaptureRequest(ReadRequest request)
            {
                var result = this.inner.CaptureRequest(request);
                this.AfterCapture?.Invoke(request);
                return result;
            }
            public object CaptureResponse(object response, ReadRequest request) => this.inner.CaptureResponse(response, request);
            public int RequestByteCount(ReadRequest request)
            {
                var result = this.inner.RequestByteCount(request);
                this.AfterRequestSize?.Invoke(request);
                return result;
            }
            public Task<object> Read(ReadRequest request, System.Threading.CancellationToken cancellationToken = default)
            {
                ++this.Dispatches;
                return this.inner.Read(request, cancellationToken);
            }
        }

        private sealed class ReadTestConnection : Allors.Workspace.Adapters.Remote.DatabaseConnection
        {
            private ReadTestConnection(Allors.Workspace.Meta.IMetaPopulation meta, string workspace)
                : base(new Allors.Workspace.Adapters.Remote.Configuration(workspace, meta,
                    new Allors.Workspace.Adapters.ReflectionObjectFactory(meta, typeof(Allors.Workspace.Domain.Person)), Array.Empty<Allors.Workspace.Derivations.IRule>()),
                    new Allors.Workspace.Adapters.IdGenerator(), () => new Allors.Workspace.WorkspaceServices(), new Allors.Ranges.DefaultStructRanges<long>()) { }
            internal static async Task<ReadTestConnection> CreateAsync(Allors.Workspace.Adapters.Remote.IReadTransport read, Allors.Workspace.Meta.IMetaPopulation meta,
                string workspace, string revision, string context = null, bool bootstrap = false, Action<ReadTestConnection> capture = null)
            {
                var connection = new ReadTestConnection(meta, workspace);
                capture?.Invoke(connection);
                await connection.InitializeAsync(bootstrap ? read : new BootstrapReadTransport(read, context), revision, context,
                    new PushReadClientTransport(_ => throw new InvalidOperationException("This read-only fixture must not dispatch Push.")),
                    new InvokeReadClientTransport(_ => throw new InvalidOperationException("This read-only fixture must not dispatch Invoke.")));
                return connection;
            }
            internal ReadStore Store => this.ScopedOwner.Store;
            internal Task<Allors.Workspace.Adapters.Remote.ReadCompletedResult> Read(ReadRequest request, System.Threading.CancellationToken cancellationToken = default) => this.ScopedOwner.Read(request, cancellationToken);
            public override Allors.Protocol.Json.IUnitConvert UnitConvert { get; } = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
        }

        private static ReadRequest CoordinatorRequest(string classTag) => new ReadRequest
        {
            v = 1, i = "coordinator", k = "read", p = new ReadPull
            {
                l = new[] { new Allors.Protocol.Json.Data.Pull { t = classTag, o = 101 } },
                d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>(),
            },
        };

        private static ReadSuccess CoordinatorResponse(ReadRequest request, string classTag, string roleTag) => new ReadSuccess
        {
            i = request.i, k = "complete", sc = new ReadScope { p = "population-1", u = 1, w = "Default", r = "v1" },
            l = new[] { new ReadInput { i = 0, o = 101, s = "resolved" } },
            c = new Dictionary<string, long[]> { ["empty"] = Array.Empty<long>() },
            o = new Dictionary<string, long?> { ["missing"] = null },
            p = new[] { new ReadRecord { i = 101, c = classTag, v = 1, g = new[] { 0 }, r = Array.Empty<int>(),
                p = new ReadRecordPayload { ro = new[] { new SyncResponseRole { t = roleTag, v = "value" } } } } },
            s = new[] { new ReadSecurity { k = "grant", i = 11, v = 1, p = new ReadSecurityPayload { p = new long[] { 201 } } } },
            pm = new[] { new ReadPermission { i = 201, p = new ReadPermissionPayload { c = classTag, o = (long)Operations.Create } } },
        };

        private static DiscoverySuccess DiscoveryResponse(ReadSuccess response) => new DiscoverySuccess
        {
            i = response.i, k = "discovery", sc = response.sc,
            p = response.p.Select(v => new ReadRecordHeader { i = v.i, c = v.c, v = v.v, g = v.g, r = v.r }).ToArray(),
            s = response.s.Select(v => new ReadSecurityHeader { k = v.k, i = v.i, v = v.v }).ToArray(),
        };

        private sealed class CoordinatorHandler : System.Net.Http.HttpMessageHandler
        {
            private readonly Func<System.Net.Http.HttpRequestMessage, Task<object>> response;
            internal CoordinatorHandler(Func<System.Net.Http.HttpRequestMessage, Task<object>> response) => this.response = response;
            protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                var input = Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(await request.Content.ReadAsStringAsync(cancellationToken));
                var result = await this.response(request);
                // Existing scenarios describe the current data; discovery exposes only its headers.
                if (input.k == "discover" && result is ReadSuccess complete) { result = DiscoveryResponse(complete); }
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                { Content = new System.Net.Http.StringContent(JsonSerializer.Serialize(result)) };
            }
        }

        [Fact]
        public void ReadStoreCanonicalIdentityIncludesClassAndCompleteSecuritySets()
        {
            var store = new ReadStore(CreateStoreScope());
            var grant1 = new ReadSecurityKey("grant", 11, 3);
            var grant2 = new ReadSecurityKey("grant", 12, 4);
            var revoke = new ReadSecurityKey("revocation", 11, 3);
            ReadStoredRecord Import(long id, string tag, long version, ReadSecurityKey[] grants, ReadSecurityKey[] revocations, Dictionary<string, object> values)
            {
                var candidate = new ReadStoredRecord(store.Scope, id, tag, version, grants, revocations, values);
                var security = candidate.Grants.Concat(candidate.Revocations).Select(key => new ReadStoredSecurity(store.Scope, key, Array.Empty<long>())).ToArray();
                return store.ImportAndPublish(new[] { candidate }, security, Array.Empty<ReadStoredPermission>()).Records.Single();
            }
            var roles = new Dictionary<string, object> { ["name"] = "old", ["links"] = new long[] { 4, 5 } };
            var record = Import(101, "C1", 7, new[] { grant1, grant2 }, new[] { revoke }, roles);
            Assert.Same(record, Import(101, "C1", 7, new[] { grant2, grant1, grant1 }, new[] { revoke },
                new Dictionary<string, object> { ["links"] = new long[] { 4, 5 }, ["name"] = "old" }));
            Assert.NotSame(record, Import(101, "C2", 7, new[] { grant1, grant2 }, new[] { revoke }, roles));
            Assert.NotSame(record, Import(101, "C1", 8, new[] { grant1, grant2 }, new[] { revoke }, roles));
            Assert.NotSame(record, Import(101, "C1", 7, new[] { grant1 }, new[] { revoke }, roles));
            Assert.NotSame(record, Import(101, "C1", 7, new[] { grant1, grant2 }, Array.Empty<ReadSecurityKey>(), roles));
            Assert.Throws<InvalidOperationException>(() => Import(101, "C1", 7, new[] { grant2, grant1 }, new[] { revoke },
                new Dictionary<string, object> { ["name"] = "old", ["links"] = new long[] { 5, 4 } }));
            Assert.Equal(new long[] { 4, 5 }, record.Roles["links"]);
        }

        [Fact]
        public void ReadStoreOwnsNestedRoleValuesOnInputAndEveryRead()
        {
            var store = new ReadStore(CreateStoreScope());
            var bytes = new byte[] { 1, 2 };
            var links = new long[] { 101, 102 };
            var date = new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc);
            var nested = new Dictionary<string, object> { ["bytes"] = bytes, ["date"] = date };
            var rows = new object[] { nested, links };
            var roles = new Dictionary<string, object> { ["rows"] = rows };
            var record = new ReadStoredRecord(store.Scope, 101, "C1", 0, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(), roles);
            bytes[0] = 9;
            links[0] = 999;
            nested["date"] = date.AddDays(1);
            rows[0] = null;
            roles.Clear();
            var first = (object[])record.Roles["rows"];
            var firstNested = (IDictionary<string, object>)first[0];
            Assert.Equal(new byte[] { 1, 2 }, firstNested["bytes"]);
            Assert.Equal(date, firstNested["date"]);
            Assert.Equal(new long[] { 101, 102 }, first[1]);
            ((byte[])firstNested["bytes"])[0] = 8;
            firstNested.Clear();
            ((long[])first[1])[0] = 888;
            var second = (object[])record.Roles["rows"];
            Assert.Equal(new byte[] { 1, 2 }, ((IDictionary<string, object>)second[0])["bytes"]);
            Assert.Equal(new long[] { 101, 102 }, second[1]);
            var cycle = new Dictionary<string, object>();
            cycle["self"] = cycle;
            Assert.Throws<ArgumentException>(() => new ReadStoredRecord(store.Scope, 102, "C1", 0, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(), cycle));
            Assert.Throws<ArgumentException>(() => new ReadStoredRecord(store.Scope, 102, "C1", 0, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(),
                new Dictionary<string, object> { ["unsupported"] = new object() }));
        }

        [Fact]
        public void ReadStorePossessionIsIndependentOfRecordProvenance()
        {
            var store = new ReadStore(CreateStoreScope());
            var key = new ReadSecurityKey("grant", 11, 3);
            var record = new ReadStoredRecord(store.Scope, 101, "C1", 7, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            Assert.Null(store.GetRecord(record));
            Assert.Null(store.GetSecurity(key));
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(new[] { record }, Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>()));
            Assert.Null(store.GetRecord(record));
            Assert.Equal(3, record.Grants.Single().Version);
            var body = new ReadStoredSecurity(store.Scope, key, new long[] { 201 });
            var permission = new ReadStoredPermission(store.Scope, 201, "C1", Operations.Read, "name");
            var snapshot = store.ImportAndPublish(new[] { record }, new[] { body }, new[] { permission });
            store.Clear();
            Assert.Null(store.GetRecord(record));
            Assert.Null(store.GetSecurity(key));
            Assert.Null(store.GetPermission(201));
            Assert.Same(record, snapshot.Records.Single());
            Assert.Equal(3, snapshot.Records.Single().Grants.Single().Version);
        }

        [Fact]
        public void ReadStoreRetainsExactSecurityVersionsKindsAndPermissionDefinitions()
        {
            var store = new ReadStore(CreateStoreScope());
            var permissionIds = new long[] { 202, 201, 201 };
            var key = new ReadSecurityKey("grant", 11, 3);
            var first = new ReadStoredSecurity(store.Scope, key, permissionIds);
            permissionIds[0] = 999;
            Assert.Equal(new long[] { 201, 202 }, first.PermissionIds);
            Allors.Workspace.Adapters.Remote.ReadSnapshot Import(ReadStoredSecurity body)
            {
                var record = new ReadStoredRecord(store.Scope, 101, "C1", body.Key.Version,
                    body.Key.Kind == "grant" ? new[] { body.Key } : Array.Empty<ReadSecurityKey>(),
                    body.Key.Kind == "revocation" ? new[] { body.Key } : Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
                return store.ImportAndPublish(new[] { record }, new[] { body }, body.PermissionIds.Select(id => new ReadStoredPermission(store.Scope, id, "C1", Operations.Read, "name")));
            }
            var firstResult = Import(first);
            Assert.Same(first, Import(new ReadStoredSecurity(store.Scope, key, new long[] { 201, 202 })).Security.Single());
            var next = new ReadStoredSecurity(store.Scope, new ReadSecurityKey("grant", 11, 4), new long[] { 203 });
            var revocation = new ReadStoredSecurity(store.Scope, new ReadSecurityKey("revocation", 11, 3), new long[] { 204 });
            Import(next);
            Import(revocation);
            Assert.Same(first, store.GetSecurity(key));
            Assert.Same(next, store.GetSecurity(next.Key));
            Assert.Same(revocation, store.GetSecurity(revocation.Key));
            Assert.Throws<InvalidOperationException>(() => Import(new ReadStoredSecurity(store.Scope, key, new long[] { 201 })));
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(firstResult.Records, firstResult.Security,
                new[] { new ReadStoredPermission(store.Scope, 201, "C1", Operations.Write, "name"), firstResult.Permissions.Single(value => value.Id == 202) }));
            Assert.Null(new ReadStoredPermission(store.Scope, 205, "C1", Operations.Create).OperandTag);
        }

        [Fact]
        public void ReadStorePublicationAdvancesCurrentIndexesAndKeepsHistoricalSnapshots()
        {
            var store = new ReadStore(CreateStoreScope());
            var key1 = new ReadSecurityKey("grant", 11, 3);
            var key2 = new ReadSecurityKey("grant", 11, 4);
            var first = new ReadStoredRecord(store.Scope, 101, "C1", 7, new[] { key1 }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { ["name"] = "first" });
            var other = new ReadStoredRecord(store.Scope, 102, "C1", 7, new[] { key1 }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var body1 = new ReadStoredSecurity(store.Scope, key1, new long[] { 201 });
            var permission1 = new ReadStoredPermission(store.Scope, 201, "C1", Operations.Read, "name");
            var oldResult = store.ImportAndPublish(new[] { first, other }, new[] { body1 }, new[] { permission1 });
            var second = new ReadStoredRecord(store.Scope, 101, "C1", 7, new[] { key2 }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { ["name"] = "second" });
            var body2 = new ReadStoredSecurity(store.Scope, key2, new long[] { 202 });
            var permission2 = new ReadStoredPermission(store.Scope, 202, "C1", Operations.Write, "name");
            var newResult = store.ImportAndPublish(new[] { second }, new[] { body2 }, new[] { permission2 });
            Assert.Same(second, store.GetCurrentRecord(101));
            Assert.Same(other, store.GetCurrentRecord(102));
            Assert.Same(body2, store.GetCurrentSecurity("grant", 11));
            Assert.Same(permission2, store.GetCurrentPermission(202));
            Assert.Equal("first", oldResult.Records[0].Roles["name"]);
            Assert.Equal(3, other.Grants.Single().Version);
            Assert.Same(first, store.GetRecord(first));
            Assert.Same(body1, store.GetSecurity(key1));
            Assert.Same(permission1, store.GetPermission(201));
            var unavailable = store.ImportAndPublish(Array.Empty<ReadStoredRecord>(), Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>(), new long[] { 101 });
            Assert.Null(store.GetCurrentRecord(101));
            Assert.True(store.IsUnavailable(101));
            Assert.False(store.IsUnavailable(102));
            Assert.Equal("second", newResult.Records.Single().Roles["name"]);
            var available = store.ImportAndPublish(new[] { second }, new[] { body2 }, new[] { permission2 });
            Assert.False(store.IsUnavailable(101));
        }

        [Fact]
        public void ReadStorePublicationFailureChangesNoCurrentIndex()
        {
            var store = new ReadStore(CreateStoreScope());
            var key = new ReadSecurityKey("grant", 11, 0);
            var original = new ReadStoredRecord(store.Scope, 101, "C1", 1, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var first = store.ImportAndPublish(new[] { original }, Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>());
            var next = new ReadStoredRecord(store.Scope, 101, "C1", 2, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var body = new ReadStoredSecurity(store.Scope, key, new long[] { 201 });
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(new[] { next }, Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>(), new long[] { 102 }));
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(new[] { next }, new[] { body }, Array.Empty<ReadStoredPermission>(), new long[] { 102 }));
            Assert.Same(original, store.GetCurrentRecord(101));
            Assert.Null(store.GetCurrentSecurity("grant", 11));
            Assert.False(store.IsUnavailable(102));
            var permission = new ReadStoredPermission(store.Scope, 201, "C1", Operations.Create);
            Assert.Throws<ArgumentException>(() => store.ImportAndPublish(new[] { next }, new[] { body }, new[] { permission }, new long[] { 101 }));
            Assert.Same(original, store.GetCurrentRecord(101));
            var foreign = new ReadStoredPermission(new ReadStoreScope("other", 1, "Default", "v1"), 201, "C1", Operations.Create);
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(new[] { next }, new[] { body }, new[] { foreign }));
            Assert.Same(original, store.GetCurrentRecord(101));
        }

        [Fact]
        public void ReadStoreClearingSeparatesScopesAndPreservesHeldContents()
        {
            var store = new ReadStore(CreateStoreScope());
            var record = new ReadStoredRecord(store.Scope, 101, "C1", 1, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { ["name"] = "old" });
            var retained = store.ImportAndPublish(new[] { record }, Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>());
            store.Clear();
            store.Clear();
            Assert.Null(store.GetCurrentRecord(101));
            Assert.Null(store.GetRecord(record));
            var replacement = new ReadStore(new ReadStoreScope("population-2", 1, "Default", "v2"));
            Assert.Throws<InvalidOperationException>(() => replacement.ImportAndPublish(new[] { record }, Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>()));
            Assert.Equal("old", retained.Records.Single().Roles["name"]);
            Assert.Equal("old", record.Roles["name"]);
        }

        [Theory]
        [InlineData("population-2", 1, "Default", "v1", null)]
        [InlineData("population-1", 2, "Default", "v1", null)]
        [InlineData("population-1", 1, "Other", "v1", null)]
        [InlineData("population-1", 1, "Default", "v2", null)]
        [InlineData("population-1", 1, "Default", "v1", "")]
        public void ReadStoreScopeIdentityIncludesEveryPartition(string population, long userId, string workspace, string revision, string context)
        {
            Assert.Equal(CreateStoreScope(), CreateStoreScope());
            Assert.NotEqual(CreateStoreScope(), new ReadStoreScope(population, userId, workspace, revision, context));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, 0)]
        [InlineData(9007199254740992, 0)]
        [InlineData(1, -1)]
        [InlineData(1, 9007199254740992)]
        public void ReadStoreRejectsInvalidInteroperableIdentities(long id, long version)
        {
            var store = new ReadStore(CreateStoreScope());
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadSecurityKey("grant", id, version));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadStoredRecord(store.Scope, id, "C1", version, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>()));
        }

        [Fact]
        public void ReadStoreImportsShareImmutableValuesAndKeepIndependentLookup()
        {
            var firstStore = new ReadStore(CreateStoreScope());
            var secondStore = new ReadStore(CreateStoreScope());
            var key = new ReadSecurityKey("grant", 11, 0);
            var candidate = new ReadStoredRecord(CreateStoreScope(), 101, "C1|7:grant,", 1, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var body = new ReadStoredSecurity(CreateStoreScope(), key, new long[] { 201 });
            var permission = new ReadStoredPermission(CreateStoreScope(), 201, "C1", Operations.Create);
            var first = firstStore.ImportAndPublish(new[] { candidate }, new[] { body }, new[] { permission });
            var second = secondStore.ImportAndPublish(first.Records, first.Security, first.Permissions);
            Assert.Same(candidate, first.Records.Single());
            Assert.Same(candidate, second.Records.Single());
            Assert.Same(candidate, firstStore.GetRecord(101, "C1|7:grant,", 1, new[] { key }, Array.Empty<ReadSecurityKey>()));
            Assert.Null(firstStore.GetRecord(101, "C1", 1, new[] { key }, Array.Empty<ReadSecurityKey>()));
            firstStore.Clear();
            Assert.Same(candidate, secondStore.GetRecord(candidate));
            var nextRevision = new ReadStore(new ReadStoreScope("population-1", 1, "Default", "v2"));
            Assert.Throws<InvalidOperationException>(() => nextRevision.ImportAndPublish(first.Records, first.Security, first.Permissions));
        }

        [Fact]
        public void ReadStorePublicationRejectsExtraClosureAndAmbiguousCurrentSecurity()
        {
            var store = new ReadStore(CreateStoreScope());
            var key1 = new ReadSecurityKey("grant", 11, 1);
            var key2 = new ReadSecurityKey("grant", 11, 2);
            var first = new ReadStoredRecord(store.Scope, 101, "C1", 1, new[] { key1 }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var second = new ReadStoredRecord(store.Scope, 102, "C1", 1, new[] { key2 }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var body1 = new ReadStoredSecurity(store.Scope, key1, new long[] { 201 });
            var body2 = new ReadStoredSecurity(store.Scope, key2, new long[] { 201 });
            var permission = new ReadStoredPermission(store.Scope, 201, "C1", Operations.Create);
            var extraPermission = new ReadStoredPermission(store.Scope, 202, "C2", Operations.Create);
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(new[] { first }, new[] { body1 }, new[] { permission, extraPermission }));
            Assert.Throws<InvalidOperationException>(() => store.ImportAndPublish(Array.Empty<ReadStoredRecord>(), new[] { body1 }, new[] { permission }));
            Assert.Throws<ArgumentException>(() => store.ImportAndPublish(new[] { first, second }, new[] { body1, body2 }, new[] { permission }));
            Assert.Null(store.GetCurrentRecord(101));
            Assert.Null(store.GetCurrentRecord(102));
            Assert.Null(store.GetCurrentSecurity("grant", 11));
            Assert.Null(store.GetCurrentPermission(201));
            var published = store.ImportAndPublish(new[] { first }, new[] { body1 }, new[] { permission });
            Assert.Single(published.Records);
            Assert.Single(published.Security);
            Assert.Single(published.Permissions);
            Assert.Throws<ArgumentException>(() => new ReadStoredRecord(store.Scope, 103, "C1", 1, new[] { key1, key2 }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>()));
        }

        [Fact]
        public void ReadStoreKeepsPermissionsUsedByOtherCurrentSecurity()
        {
            var store = new ReadStore(CreateStoreScope());
            var firstKey = new ReadSecurityKey("grant", 11, 1);
            var nextKey = new ReadSecurityKey("grant", 11, 2);
            var otherKey = new ReadSecurityKey("revocation", 11, 1);
            var first = new ReadStoredRecord(store.Scope, 101, "C1", 1, new[] { firstKey }, new[] { otherKey }, new Dictionary<string, object>());
            var next = new ReadStoredRecord(store.Scope, 101, "C1", 2, new[] { nextKey }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var body1 = new ReadStoredSecurity(store.Scope, firstKey, new long[] { 201 });
            var body2 = new ReadStoredSecurity(store.Scope, nextKey, new long[] { 202 });
            var otherBody = new ReadStoredSecurity(store.Scope, otherKey, new long[] { 201 });
            var permission1 = new ReadStoredPermission(store.Scope, 201, "C1", Operations.Read, "name");
            var permission2 = new ReadStoredPermission(store.Scope, 202, "C1", Operations.Write, "name");
            var firstResult = store.ImportAndPublish(new[] { first }, new[] { body1, otherBody }, new[] { permission1 });
            var secondResult = store.ImportAndPublish(new[] { next }, new[] { body2 }, new[] { permission2 });
            Assert.Same(permission1, store.GetCurrentPermission(201));
            Assert.Same(otherBody, store.GetCurrentSecurity("revocation", 11));
            Assert.Same(first, store.GetRecord(first));
            Assert.Same(next, store.GetRecord(next));
            store.Clear();
            Assert.Null(store.GetRecord(next));
            Assert.Same(first, firstResult.Records.Single());
            Assert.Same(next, secondResult.Records.Single());
        }

        [Theory]
        [InlineData("record-scope", typeof(InvalidOperationException))]
        [InlineData("security-scope", typeof(InvalidOperationException))]
        [InlineData("permission-scope", typeof(InvalidOperationException))]
        [InlineData("missing-security", typeof(InvalidOperationException))]
        [InlineData("missing-permission", typeof(InvalidOperationException))]
        [InlineData("record-conflict", typeof(InvalidOperationException))]
        [InlineData("security-conflict", typeof(InvalidOperationException))]
        [InlineData("permission-conflict", typeof(InvalidOperationException))]
        [InlineData("duplicate-record", typeof(ArgumentException))]
        [InlineData("duplicate-security", typeof(ArgumentException))]
        [InlineData("duplicate-permission", typeof(ArgumentException))]
        [InlineData("duplicate-record-conflict", typeof(InvalidOperationException))]
        [InlineData("duplicate-security-conflict", typeof(InvalidOperationException))]
        [InlineData("duplicate-permission-conflict", typeof(InvalidOperationException))]
        [InlineData("available-and-unavailable", typeof(ArgumentException))]
        public void ReadStoreImportFailureRetainsNoCandidatesOrCurrentChanges(string defect, Type exceptionType)
        {
            var scope = CreateStoreScope();
            var foreign = new ReadStoreScope("other-population", 1, "Default", "v1");
            var store = new ReadStore(scope);
            var key = new ReadSecurityKey("grant", 11, 1);
            var original = new ReadStoredRecord(store.Scope, 101, "C1", 1, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var originalBody = new ReadStoredSecurity(store.Scope, key, new long[] { 201 });
            var originalPermission = new ReadStoredPermission(store.Scope, 201, "C1", Operations.Create);
            var current = store.ImportAndPublish(new[] { original }, new[] { originalBody }, new[] { originalPermission });
            var nextKey = new ReadSecurityKey("grant", 12, 1);
            var candidate = new ReadStoredRecord(scope, 102, "C1", 1, new[] { nextKey }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            var nextBody = new ReadStoredSecurity(scope, nextKey, new long[] { 202 });
            var nextPermission = new ReadStoredPermission(scope, 202, "C1", Operations.Create);
            var records = new[] { original, candidate };
            var security = new[] { originalBody, nextBody };
            var permissions = new[] { originalPermission, nextPermission };
            var unavailable = new long[] { 103 };

            switch (defect)
            {
                case "record-scope":
                    records[1] = new ReadStoredRecord(foreign, 102, "C1", 1, new[] { nextKey }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
                    break;
                case "security-scope": security[1] = new ReadStoredSecurity(foreign, nextKey, new long[] { 202 }); break;
                case "permission-scope": permissions[1] = new ReadStoredPermission(foreign, 202, "C1", Operations.Create); break;
                case "missing-security": security = new[] { originalBody }; break;
                case "missing-permission": permissions = new[] { originalPermission }; break;
                case "record-conflict":
                    records[0] = new ReadStoredRecord(scope, 101, "C1", 1, new[] { key }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { ["name"] = "changed" });
                    break;
                case "security-conflict": security[0] = new ReadStoredSecurity(scope, key, new long[] { 201, 202 }); break;
                case "permission-conflict": permissions[0] = new ReadStoredPermission(scope, 201, "C1", Operations.Read, "name"); break;
                case "duplicate-record": records = records.Append(candidate).ToArray(); break;
                case "duplicate-security": security = security.Append(nextBody).ToArray(); break;
                case "duplicate-permission": permissions = permissions.Append(nextPermission).ToArray(); break;
                case "duplicate-record-conflict":
                    records = records.Append(new ReadStoredRecord(scope, 102, "C1", 1, new[] { nextKey }, Array.Empty<ReadSecurityKey>(), new Dictionary<string, object> { ["name"] = "changed" })).ToArray();
                    break;
                case "duplicate-security-conflict": security = security.Append(new ReadStoredSecurity(scope, nextKey, new long[] { 201, 202 })).ToArray(); break;
                case "duplicate-permission-conflict": permissions = permissions.Append(new ReadStoredPermission(scope, 202, "C1", Operations.Read, "name")).ToArray(); break;
                case "available-and-unavailable": unavailable = new long[] { 102, 103 }; break;
            }

            Assert.Throws(exceptionType, () => store.ImportAndPublish(records, security, permissions, unavailable));

            Assert.Same(original, store.GetCurrentRecord(101));
            Assert.Same(originalBody, store.GetCurrentSecurity("grant", 11));
            Assert.Same(originalPermission, store.GetCurrentPermission(201));
            Assert.Null(store.GetRecord(candidate));
            Assert.Null(store.GetSecurity(nextKey));
            Assert.Null(store.GetPermission(202));
            Assert.Null(store.GetCurrentRecord(102));
            Assert.Null(store.GetCurrentSecurity("grant", 12));
            Assert.Null(store.GetCurrentPermission(202));
            Assert.False(store.IsUnavailable(103));
        }

        [Fact]
        public void ReadStoreImportRechecksCancellationAfterInputCapture()
        {
            var store = new ReadStore(CreateStoreScope());
            var candidate = new ReadStoredRecord(store.Scope, 101, "C1", 1, Array.Empty<ReadSecurityKey>(), Array.Empty<ReadSecurityKey>(), new Dictionary<string, object>());
            using var cancellation = new System.Threading.CancellationTokenSource();
            IEnumerable<ReadStoredRecord> Capture()
            {
                yield return candidate;
                cancellation.Cancel();
            }
            Assert.Throws<OperationCanceledException>(() => store.ImportAndPublish(Capture(), Array.Empty<ReadStoredSecurity>(), Array.Empty<ReadStoredPermission>(), cancellationToken: cancellation.Token));
            Assert.Null(store.GetRecord(candidate));
        }

        private static ReadStoreScope CreateStoreScope() => new ReadStoreScope("population-1", 1, "Default", "v1", null);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task JsonApiPullDisposesItsTransaction(bool throwAfterPull)
        {
            await this.Login("administrator");
            var profile = (Profile)this.Profile;
            var sink = new PullSink(throwAfterPull);
            profile.Database.Sink = sink;
            var session = this.Workspace.CreateSession();
            var pull = new Pull { Extent = new Filter(this.M.C1) };

            try
            {
                if (throwAfterPull)
                {
                    var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync(pull));
                    Assert.Equal("Test pull failure", exception.Message);
                }
                else
                {
                    var result = await session.PullAsync(pull);
                    Assert.False(result.HasErrors);
                    Assert.NotEmpty(result.GetCollection<Allors.Workspace.Domain.C1>());
                }

                Assert.NotNull(sink.PullEvent);
                Assert.NotNull(sink.PullEvent.PullResponse);
                Assert.Equal(profile.DatabaseConnection.UserId, sink.UserId);
                // The sink leaves a new object in the request transaction. Disposal must roll it
                // back on both success and failure, rather than committing it with the response.
                Assert.True(sink.UncommittedObject.Strategy.IsDeleted);
                using var transaction = profile.Database.CreateTransaction();
                Assert.NotSame(sink.PullEvent.Transaction, transaction);
                Assert.Null(transaction.Instantiate(sink.UncommittedId));
            }
            finally
            {
                profile.Database.Sink = null;
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("SystemTextWeb")]
        [InlineData("Newtonsoft")]
        public void ReadProtocolMatchesSharedFixture(string serializer)
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.ReadContract.json");
            Assert.NotNull(stream);
            using var fixture = JsonDocument.Parse(stream);

            var cold = AssertReadContractFixture(fixture, "coldRequest", new ReadRequest
            {
                v = 1, i = "cold-1", x = "tenant=blue", k = "read", p = CreateReadPull(),
            }, serializer);
            Assert.Null(cold.sc);
            Assert.Null(cold.h);

            var cached = AssertReadContractFixture(fixture, "cachedRequest", new ReadRequest
            {
                v = 1, i = "read-1", sc = CreateReadScope(), x = "tenant=blue", k = "read", p = CreateReadPull(),
                h = new ReadCache
                {
                    s = new[]
                    {
                        new ReadSecurityHint { k = "grant", i = 11, v = 4, h = true },
                        new ReadSecurityHint { k = "grant", i = 11, v = 3, h = true },
                        new ReadSecurityHint { k = "revocation", i = 21, v = 2, h = false },
                    },
                    r = new[]
                    {
                        new ReadRecordHeader { i = 102, c = "C1", v = 4294967298, g = new[] { 0 }, r = new[] { 2 } },
                        new ReadRecordHeader { i = 101, c = "C1", v = 7, g = new[] { 1 }, r = new[] { 2 } },
                    },
                    p = new long[] { 201 },
                },
            }, serializer);
            Assert.Equal(cached.h.s[0].i, cached.h.s[1].i);
            Assert.Equal(new long[] { 4, 3 }, cached.h.s.Take(2).Select(v => v.v));
            Assert.False(cached.h.s[2].h);
            Assert.Equal(4294967298, cached.h.r[0].v);

            AssertReadContractFixture(fixture, "discovery", new DiscoverySuccess
            {
                i = "discover-1", sc = CreateReadScope(), k = "discovery",
                p = new[]
                {
                    new ReadRecordHeader { i = 101, c = "C1", v = 7, g = new[] { 0 }, r = new[] { 1 } },
                    new ReadRecordHeader { i = 102, c = "C1", v = 4294967298, g = new[] { 0 }, r = new[] { 1 } },
                },
                s = new[]
                {
                    new ReadSecurityHeader { k = "grant", i = 11, v = 4 },
                    new ReadSecurityHeader { k = "revocation", i = 21, v = 2 },
                },
            }, serializer);

            var unitValues = CreateReadUnitValues();
            var roles = unitValues.Select(v => new SyncResponseRole { t = v.Key, v = v.Value }).Concat(new[]
            {
                new SyncResponseRole { t = "one", o = 4294967297 },
                new SyncResponseRole { t = "many", c = new long[] { 102, 4294967297 } },
            }).ToArray();
            var complete = AssertReadContractFixture(fixture, "complete", new ReadSuccess
            {
                i = "read-1", sc = CreateReadScope(), k = "complete",
                l = new[]
                {
                    new ReadInput { i = 0, o = 101, s = "resolved" },
                    new ReadInput { i = 1, o = 999, s = "unavailable" },
                    new ReadInput { i = 2, s = "resolved" },
                },
                c = new Dictionary<string, long[]> { ["found"] = new long[] { 101, 102, 103 }, ["empty"] = Array.Empty<long>() },
                o = new Dictionary<string, long?> { ["first"] = 101, ["missing"] = null },
                p = new[]
                {
                    new ReadRecord { i = 101, c = "C1", v = 7, g = new[] { 0, 2 }, r = new[] { 1 }, p = new ReadRecordPayload { ro = roles } },
                    new ReadRecord { i = 102, c = "C1", v = 4294967298, g = new[] { 0 }, r = new[] { 1 }, p = new ReadRecordPayload { r = 0 } },
                    new ReadRecord { i = 103, c = "C1", v = 1, g = Array.Empty<int>(), r = Array.Empty<int>(), p = new ReadRecordPayload { ro = Array.Empty<SyncResponseRole>() } },
                },
                s = new[]
                {
                    new ReadSecurity { k = "grant", i = 11, v = 4, p = new ReadSecurityPayload { r = 0 } },
                    new ReadSecurity { k = "revocation", i = 21, v = 2, p = new ReadSecurityPayload { p = Array.Empty<long>() } },
                    new ReadSecurity { k = "grant", i = 12, v = 1, p = new ReadSecurityPayload { p = new long[] { 201, 202, 203, 204, 205 } } },
                },
                pm = new[]
                {
                    new ReadPermission { i = 201, p = new ReadPermissionPayload { r = 0 } },
                    new ReadPermission { i = 202, p = new ReadPermissionPayload { c = "C1", o = (long)Operations.Create } },
                    new ReadPermission { i = 203, p = new ReadPermissionPayload { c = "C1", o = (long)Operations.Read, t = "string" } },
                    new ReadPermission { i = 204, p = new ReadPermissionPayload { c = "C1", o = (long)Operations.Write, t = "string" } },
                    new ReadPermission { i = 205, p = new ReadPermissionPayload { c = "C1", o = (long)Operations.Execute, t = "DoWork" } },
                },
            }, serializer);
            Assert.Equal(cached.p.l.Length, complete.l.Length);
            Assert.True(complete.o.ContainsKey("missing"));
            Assert.Null(complete.o["missing"]);
            Assert.Empty(complete.c["empty"]);
            var nothing = Assert.Single(complete.p[0].p.ro, role => role.t == "nothing");
            Assert.Null(nothing.v);
            Assert.Equal(0, complete.p[1].p.r);
            Assert.Null(complete.p[1].p.ro);
            Assert.Empty(complete.p[2].p.ro);
            Assert.Null(complete.p[2].p.r);
            Assert.Equal(0, complete.s[0].p.r);
            Assert.Empty(complete.s[1].p.p);
            Assert.Equal(0, complete.pm[0].p.r);
            Assert.Equal(new long?[] { 1, 2, 4, 8 }, complete.pm.Skip(1).Select(v => v.p.o));
            Assert.Null(complete.pm[1].p.t);
            Assert.Equal(4294967297, complete.p[0].p.ro.Single(v => v.t == "one").o);
            Assert.Equal(new long[] { 102, 4294967297 }, complete.p[0].p.ro.Single(v => v.t == "many").c);

            var failure = AssertReadContractFixture(fixture, "failure", new ReadFailure
            {
                i = "read-1", sc = CreateReadScope(), k = "error",
                e = new[] { new ReadError { c = "invalid-cache-reference", m = "Remove the invalid cache reference and retry the read.", o = new long[] { 101 }, i = 0 } },
            }, serializer);
            Assert.Equal(0, failure.e[0].i);

            var unscoped = AssertReadContractFixture(fixture, "unscopedFailure", new ReadFailure
            {
                i = "cold-1", k = "error",
                e = new[] { new ReadError { c = "unsupported-protocol", m = "Use a supported read protocol revision." } },
            }, serializer);
            Assert.Null(unscoped.sc);
            Assert.Null(unscoped.e[0].i);
            Assert.Null(unscoped.e[0].o);

            AssertReadContractFixture(fixture, "emptyComplete", new ReadSuccess
            {
                i = "empty-1", sc = CreateReadScope(), k = "complete", l = Array.Empty<ReadInput>(),
                c = new Dictionary<string, long[]>(), o = new Dictionary<string, long?>(),
                p = Array.Empty<ReadRecord>(), s = Array.Empty<ReadSecurity>(), pm = Array.Empty<ReadPermission>(),
            }, serializer);
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("SystemTextWeb")]
        [InlineData("Newtonsoft")]
        public void ReadProtocolDecodesOmittedOptionalFields(string serializer)
        {
            var request = DeserializeReadContract<ReadRequest>("{\"v\":1,\"i\":\"cold\",\"k\":\"discover\",\"p\":{\"l\":[],\"d\":[]}}", serializer);
            Assert.Equal("discover", request.k);
            Assert.Null(request.sc);
            Assert.Null(request.x);
            Assert.Null(request.h);
            Assert.Empty(request.p.l);
            Assert.Empty(request.p.d);

            var scope = DeserializeReadContract<ReadScope>("{\"p\":\"population-1\",\"u\":7,\"w\":\"Default\",\"r\":\"schema-1\"}", serializer);
            Assert.Null(scope.x);
            var emptyCache = DeserializeReadContract<ReadCache>("{\"s\":[],\"r\":[],\"p\":[]}", serializer);
            Assert.Empty(emptyCache.s);
            Assert.Empty(emptyCache.r);
            Assert.Empty(emptyCache.p);

            var failure = DeserializeReadContract<ReadFailure>("{\"i\":\"failed\",\"k\":\"error\",\"e\":[{\"c\":\"scope-retired\",\"m\":\"Start a new read without cache hints.\"}]}", serializer);
            Assert.Null(failure.sc);
            Assert.Null(failure.e[0].o);
            Assert.Null(failure.e[0].i);

            var recordBody = DeserializeReadContract<ReadRecordPayload>("{\"ro\":[]}", serializer);
            Assert.Empty(recordBody.ro);
            Assert.Null(recordBody.r);
            var recordReuse = DeserializeReadContract<ReadRecordPayload>("{\"r\":0}", serializer);
            Assert.Null(recordReuse.ro);
            Assert.Equal(0, recordReuse.r);

            var securityBody = DeserializeReadContract<ReadSecurityPayload>("{\"p\":[]}", serializer);
            Assert.Empty(securityBody.p);
            Assert.Null(securityBody.r);
            var securityReuse = DeserializeReadContract<ReadSecurityPayload>("{\"r\":0}", serializer);
            Assert.Null(securityReuse.p);
            Assert.Equal(0, securityReuse.r);

            var createPermission = DeserializeReadContract<ReadPermissionPayload>("{\"c\":\"C1\",\"o\":1}", serializer);
            Assert.Equal(1, createPermission.o);
            Assert.Null(createPermission.t);
            Assert.Null(createPermission.r);
            var permissionReuse = DeserializeReadContract<ReadPermissionPayload>("{\"r\":0}", serializer);
            Assert.Null(permissionReuse.c);
            Assert.Null(permissionReuse.o);
            Assert.Null(permissionReuse.t);
            Assert.Equal(0, permissionReuse.r);

            var input = DeserializeReadContract<ReadInput>("{\"i\":0,\"s\":\"resolved\"}", serializer);
            Assert.Equal(0, input.i);
            Assert.Null(input.o);
        }

        [Fact]
        public void ReadProtocolRootsExcludeLegacyResponseMembers()
        {
            foreach (var root in new[] { typeof(ReadRequest), typeof(DiscoverySuccess), typeof(ReadSuccess), typeof(ReadFailure) })
            {
                Assert.False(typeof(Allors.Protocol.Json.Api.Response).IsAssignableFrom(root));
            }

            Assert.Equal(new[] { "d", "l" }, typeof(ReadPull).GetProperties().Select(v => v.Name).OrderBy(v => v).ToArray());
        }

        public static IEnumerable<object[]> ReadValidationCases()
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.ReadValidationCases.json");
            using var cases = JsonDocument.Parse(stream);
            foreach (var item in cases.RootElement.EnumerateArray())
            {
                yield return new object[] { "SystemText", item.GetProperty("name").GetString() };
                yield return new object[] { "Newtonsoft", item.GetProperty("name").GetString() };
            }
        }

        [Theory]
        [MemberData(nameof(ReadValidationCases))]
        public void ReadValidationMatchesSharedCases(string serializer, string name)
        {
            using var fixtureStream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.ReadContract.json");
            using var casesStream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.ReadValidationCases.json");
            using var fixture = JsonDocument.Parse(fixtureStream);
            using var cases = JsonDocument.Parse(casesStream);
            var item = cases.RootElement.EnumerateArray().Single(value => value.GetProperty("name").GetString() == name);
            var request = JsonNode.Parse(fixture.RootElement.GetProperty(item.GetProperty("request").GetString()).GetRawText());
            if (item.TryGetProperty("requestChanges", out var requestChanges))
            {
                ApplyReadValidationChanges(request, requestChanges);
            }

            JsonNode response = null;
            if (item.TryGetProperty("response", out var responseName))
            {
                response = JsonNode.Parse(fixture.RootElement.GetProperty(responseName.GetString()).GetRawText());
            }

            ApplyReadValidationChanges(response ?? request, item.GetProperty("changes"));
            if (item.GetProperty("valid").GetBoolean())
            {
                Decode();
            }
            else
            {
                var exception = Assert.Throws<FormatException>(Decode);
                Assert.Contains("$", exception.Message);
                Assert.Contains(":", exception.Message);
            }

            void Decode()
            {
                var original = DecodeReadRequest(request.ToJsonString(), serializer);
                if (response != null)
                {
                    Assert.NotNull(DecodeReadResponse(response.ToJsonString(), original, serializer));
                }
            }
        }

        private static void ApplyReadValidationChanges(JsonNode root, JsonElement changes)
        {
            foreach (var change in changes.EnumerateArray())
            {
                var path = change.GetProperty("path").EnumerateArray().ToArray();
                var target = root;
                foreach (var segment in path.Take(path.Length - 1))
                {
                    target = segment.ValueKind == JsonValueKind.Number ? target[segment.GetInt32()] : target[segment.GetString()];
                }

                var key = path.Last();
                var remove = change.TryGetProperty("remove", out var deletion) && deletion.GetBoolean();
                var value = remove ? null : JsonNode.Parse(change.GetProperty("value").GetRawText());
                if (key.ValueKind == JsonValueKind.Number)
                {
                    if (remove)
                    {
                        target.AsArray().RemoveAt(key.GetInt32());
                    }
                    else
                    {
                        target[key.GetInt32()] = value;
                    }
                }
                else if (remove)
                {
                    target.AsObject().Remove(key.GetString());
                }
                else
                {
                    target[key.GetString()] = value;
                }
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void ReadValidationAcceptsSharedMessages(string serializer)
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.ReadContract.json");
            using var fixture = JsonDocument.Parse(stream);
            var root = fixture.RootElement;
            var cold = DecodeReadRequest(root.GetProperty("coldRequest").GetRawText(), serializer);
            var cached = DecodeReadRequest(root.GetProperty("cachedRequest").GetRawText(), serializer);
            var complete = Assert.IsType<ReadSuccess>(DecodeReadResponse(root.GetProperty("complete").GetRawText(), cached, serializer));
            Assert.Equal(0, complete.p[1].p.r);
            Assert.Null(complete.o["missing"]);
            Assert.Empty(complete.c["empty"]);
            Assert.IsType<ReadFailure>(DecodeReadResponse(root.GetProperty("failure").GetRawText(), cached, serializer));
            Assert.IsType<ReadFailure>(DecodeReadResponse(root.GetProperty("unscopedFailure").GetRawText(), cold, serializer));
            cold.i = "discover-1";
            cold.k = "discover";
            Assert.IsType<DiscoverySuccess>(DecodeReadResponse(root.GetProperty("discovery").GetRawText(), cold, serializer));
            cold.i = "empty-1";
            cold.k = "read";
            cold.p.l = Array.Empty<Allors.Protocol.Json.Data.Pull>();
            Assert.IsType<ReadSuccess>(DecodeReadResponse(root.GetProperty("emptyComplete").GetRawText(), cold, serializer));
        }

        [Theory]
        [InlineData("{\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1,\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[],}}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}/*comment*/")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}} null")]
        [InlineData("{'v':1,'i':'read','k':'read','p':{'l':[],'d':[]}}")]
        [InlineData("{v:1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":\"1\",\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":01,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":+1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":NaN,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":Infinity,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1e999,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1.0000000000000001,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":9007199254740990.5,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1e-400,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]},\"h\":undefined}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]},\"procedure\":null}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]},\"\\u0076\":1}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[,],\"d\":[]}}")]
        [InlineData("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}\v")]
        [InlineData("{\"v\":1,\"i\":\"read\n\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}")]
        [InlineData("null")]
        [InlineData("[]")]
        public void ReadValidationRejectsMalformedJson(string json)
        {
            foreach (var serializer in new[] { "SystemText", "Newtonsoft" })
            {
                var exception = Assert.Throws<FormatException>(() => DecodeReadRequest(json, serializer));
                Assert.Contains("$", exception.Message);
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void ReadValidationRechecksOriginalRequest(string serializer)
        {
            const string json = "{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";
            var request = DecodeReadRequest(json, serializer);
            request.v = 0;
            var exception = Assert.Throws<FormatException>(() => DecodeReadResponse("{\"i\":\"read\",\"k\":\"error\",\"e\":[{\"c\":\"invalid\",\"m\":\"Retry the read.\"}]}", request, serializer));
            Assert.Contains("$", exception.Message);
        }

        [Theory]
        [InlineData("1.0")]
        [InlineData("1e0")]
        public void ReadValidationAcceptsIntegralNumberSpellings(string revision)
        {
            foreach (var serializer in new[] { "SystemText", "Newtonsoft" })
            {
                var request = DecodeReadRequest("{\"v\":" + revision + ",\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}", serializer);
                Assert.Equal(1, request.v);
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void ReadValidationRejectsDuplicateOutputNames(string serializer)
        {
            var request = DecodeReadRequest("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}", serializer);
            const string response = "{\"i\":\"read\",\"k\":\"complete\",\"sc\":{\"p\":\"population\",\"u\":7,\"w\":\"Default\",\"r\":\"schema\"},\"l\":[],\"p\":[],\"s\":[],\"pm\":[],\"c\":{},\"o\":{\"same\":null,\"same\":null}}";
            var exception = Assert.Throws<FormatException>(() => DecodeReadResponse(response, request, serializer));
            Assert.Contains("$", exception.Message);
            Assert.Contains(serializer == "SystemText" ? "duplicate JSON property" : "unique property names", exception.Message);
        }

        [Theory]
        [InlineData("1.0000000000000001")]
        [InlineData("9007199254740990.5")]
        [InlineData("1e-400")]
        public void ReadValidationRejectsNumbersRoundedToIntegers(string number)
        {
            foreach (var serializer in new[] { "SystemText", "Newtonsoft" })
            {
                var json = "{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[{\"a\":{\"n\":" + number + "}}],\"d\":[]}}";
                Assert.Throws<FormatException>(() => DecodeReadRequest(json, serializer));
            }
        }

        [Theory]
        [InlineData(58, true)]
        [InlineData(59, true)]
        [InlineData(60, false)]
        public void ReadValidationLimitsJsonNesting(int arrays, bool valid)
        {
            var value = new string('[', arrays) + "null" + new string(']', arrays);
            var json = "{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[{\"a\":{\"n\":" + value + "}}],\"d\":[]}}";
            foreach (var serializer in new[] { "SystemText", "Newtonsoft" })
            {
                if (valid)
                {
                    Assert.NotNull(DecodeReadRequest(json, serializer));
                }
                else
                {
                    Assert.Throws<FormatException>(() => DecodeReadRequest(json, serializer));
                }
            }
        }

        [Theory]
        [InlineData("\\uD800", false)]
        [InlineData("\\uDC00", false)]
        [InlineData("\\uD800a", false)]
        [InlineData("\\uD83D\\uDE00", true)]
        [InlineData("é", true)]
        public void ReadValidationChecksUnicodeEscapes(string value, bool valid)
        {
            var json = "{\"v\":1,\"i\":\"" + value + "\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";
            foreach (var serializer in new[] { "SystemText", "Newtonsoft" })
            {
                if (valid)
                {
                    Assert.NotNull(DecodeReadRequest(json, serializer));
                }
                else
                {
                    Assert.Throws<FormatException>(() => DecodeReadRequest(json, serializer));
                }
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void ReadValidationRejectsLiteralUnpairedSurrogates(string serializer)
        {
            foreach (var value in new[] { new string('\ud800', 1), new string('\udc00', 1) })
            {
                var json = "{\"v\":1,\"i\":\"" + value + "\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";
                Assert.Throws<FormatException>(() => DecodeReadRequest(json, serializer));
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void ReadValidationRejectsUnserializableOriginalRequests(string serializer)
        {
            var request = DecodeReadRequest("{\"v\":1,\"i\":\"read\",\"k\":\"read\",\"p\":{\"l\":[{\"a\":{}}],\"d\":[]}}", serializer);
            request.p.l[0].a["self"] = request;
            Assert.Throws<FormatException>(() => DecodeReadResponse("{\"i\":\"read\",\"k\":\"error\",\"e\":[{\"c\":\"invalid\",\"m\":\"Retry the read.\"}]}", request, serializer));
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("SystemTextWeb")]
        [InlineData("Newtonsoft")]
        public void PushReadProtocolSerializesIndependentDtoShape(string serializer)
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.PushReadContract.json");
            using var fixture = JsonDocument.Parse(stream);
            var scope = CreateReadScope();
            var request = new PushReadRequest
            {
                v = 1, i = "save-1", sc = scope, x = "tenant=blue", returnChanged = true,
                push = new PushReadChanges
                {
                    n = new[] { new PushRequestNewObject { w = -1, t = "C1", r = new[] { new PushRequestRole { t = "number", u = -3 } } } },
                    o = new[] { new PushRequestObject { d = 101, v = 7, r = Array.Empty<PushRequestRole>() } },
                },
                discover = new ReadPull { l = new[] { new Allors.Protocol.Json.Data.Pull { o = 101 } }, d = Array.Empty<Allors.Protocol.Json.Api.Pull.PullDependency>() },
            };
            var response = new PushReadResponse
            {
                i = "save-1", sc = scope,
                push = new PushReadOutcome { k = "committed", n = new[] { new PushResponseNewObject { w = -1, d = 102 } } },
                changed = new PushReadChanged
                {
                    i = "save-1", sc = scope, k = "changed",
                    p = new[] { new ReadRecord { i = 101, c = "C1", v = 8, g = new[] { 0 }, r = Array.Empty<int>(), p = new ReadRecordPayload { ro = new[] { new SyncResponseRole { t = "number", v = -3 } } } } },
                    s = new[] { new ReadSecurity { k = "grant", i = 11, v = 4, p = new ReadSecurityPayload { p = new long[] { 201 } } } },
                    pm = new[] { new ReadPermission { i = 201, p = new ReadPermissionPayload { c = "C1", o = 2, t = "number" } } },
                    u = new long[] { 102 },
                },
                discovery = new DiscoverySuccess { i = "save-1", sc = scope, k = "discovery", p = Array.Empty<ReadRecordHeader>(), s = Array.Empty<ReadSecurityHeader>() },
            };
            foreach (var pair in new[] { (Name: "request", Value: (object)request), (Name: "response", Value: (object)response) })
            {
                var actual = serializer == "Newtonsoft"
                    ? JsonConvert.SerializeObject(pair.Value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    : JsonSerializer.Serialize(pair.Value, new JsonSerializerOptions(serializer == "SystemTextWeb" ? JsonSerializerDefaults.Web : JsonSerializerDefaults.General) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(fixture.RootElement.GetProperty(pair.Name).GetRawText()), JsonNode.Parse(actual)), serializer + " " + pair.Name + ": " + actual);
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void PushReadProtocolValidatesSharedFixturesAndIndependentReceipts(string serializer)
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.PushReadContract.json");
            using var fixture = JsonDocument.Parse(stream);
            var requestJson = fixture.RootElement.GetProperty("request").GetRawText();
            var request = serializer == "SystemText"
                ? Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(requestJson)
                : Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeRequest(requestJson);
            using var casesStream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.PushReadValidationCases.json");
            using var cases = JsonDocument.Parse(casesStream);
            foreach (var group in cases.RootElement.EnumerateObject())
            {
                foreach (var entry in group.Value.EnumerateArray())
                {
                    var json = entry.GetProperty("json").GetString();
                    Action action = group.Name.Contains("Request")
                        ? () => { if (serializer == "SystemText") Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeRequest(json); else Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeRequest(json); }
                        : group.Name.Contains("Receipt") || group.Name == "receiptOnlyResponses"
                        ? () => { if (serializer == "SystemText") Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeReceipt(json, request); else Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeReceipt(json, request); }
                        : () => { if (serializer == "SystemText") Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeResponse(json, request); else Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeResponse(json, request); };
                    var error = Record.Exception(action);
                    Assert.True(group.Name.StartsWith("invalid", StringComparison.Ordinal) ? error is FormatException : error == null,
                        group.Name + ": " + entry.GetProperty("name").GetString() + ": " + error);
                    if (group.Name == "receiptOnlyResponses")
                    {
                        Assert.Throws<FormatException>(() => { if (serializer == "SystemText") Allors.Protocol.Json.SystemTextJson.PushReadJson.DeserializeResponse(json, request); else Allors.Protocol.Json.Newtonsoft.PushReadJson.DeserializeResponse(json, request); });
                    }
                }
            }
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("SystemTextWeb")]
        [InlineData("Newtonsoft")]
        public void InvokeReadProtocolSerializesIndependentDtoShape(string serializer)
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.InvokeReadContract.json");
            using var fixture = JsonDocument.Parse(stream);
            var scope = CreateReadScope();
            var request = new InvokeReadRequest { v = 1, i = "invoke-1", sc = scope, x = "tenant=blue", refresh = true, l = new[]
            {
                new Allors.Protocol.Json.Api.Invoke.Invocation { i = 101, v = 7, m = "JustDoIt" },
                new Allors.Protocol.Json.Api.Invoke.Invocation { i = 102, v = 8, m = "JustDoIt" },
            } };
            var response = new InvokeReadResponse { i = "invoke-1", sc = scope, invoke = new InvokeReadOutcome { k = "committed" }, changed = new PushReadChanged
            {
                i = "invoke-1", sc = scope, k = "changed",
                p = new[] { new ReadRecord { i = 101, c = "C1", v = 8, g = new[] { 0 }, r = Array.Empty<int>(), p = new ReadRecordPayload { ro = new[] { new SyncResponseRole { t = "name", v = "after" } } } } },
                s = new[] { new ReadSecurity { k = "grant", i = 11, v = 1, p = new ReadSecurityPayload { p = new long[] { 31 } } } },
                pm = new[] { new ReadPermission { i = 31, p = new ReadPermissionPayload { c = "C1", o = 2, t = "name" } } }, u = new long[] { 102 },
            } };
            foreach (var pair in new[] { (Name: "request", Value: (object)request), (Name: "response", Value: (object)response) })
            {
                var actual = serializer == "Newtonsoft"
                    ? JsonConvert.SerializeObject(pair.Value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
                    : JsonSerializer.Serialize(pair.Value, new JsonSerializerOptions(serializer == "SystemTextWeb" ? JsonSerializerDefaults.Web : JsonSerializerDefaults.General) { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
                Assert.True(JsonNode.DeepEquals(JsonNode.Parse(fixture.RootElement.GetProperty(pair.Name).GetRawText()), JsonNode.Parse(actual)), serializer + " " + pair.Name + ": " + actual);
            }
            Assert.Null(typeof(InvokeReadRequest).GetProperty("o"));
            Assert.Null(typeof(InvokeReadOutcome).GetProperty("n"));
            Assert.Null(typeof(InvokeReadResponse).GetProperty("discovery"));
        }

        [Theory]
        [InlineData("SystemText")]
        [InlineData("Newtonsoft")]
        public void InvokeReadProtocolValidatesSharedFixturesAndIndependentReceipts(string serializer)
        {
            using var stream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.InvokeReadContract.json");
            using var fixture = JsonDocument.Parse(stream);
            using var casesStream = typeof(PullTests).Assembly.GetManifestResourceStream("Allors.InvokeReadValidationCases.json");
            using var cases = JsonDocument.Parse(casesStream);
            var count = 0;
            foreach (var group in cases.RootElement.EnumerateObject())
            foreach (var entry in group.Value.EnumerateArray())
            {
                ++count;
                var json = entry.GetProperty("json").GetString();
                var requestJson = entry.TryGetProperty("request", out var overridden) ? overridden.GetRawText() : fixture.RootElement.GetProperty("request").GetRawText();
                var request = serializer == "SystemText" ? InvokeReadJson.DeserializeRequest(requestJson) : Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeRequest(requestJson);
                Action action = group.Name.Contains("Request")
                    ? () => { if (serializer == "SystemText") InvokeReadJson.DeserializeRequest(json); else Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeRequest(json); }
                    : group.Name == "invalidReceipts"
                    ? () => { if (serializer == "SystemText") InvokeReadJson.DeserializeReceipt(json, request); else Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeReceipt(json, request); }
                    : () => { if (serializer == "SystemText") InvokeReadJson.DeserializeResponse(json, request); else Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeResponse(json, request); };
                var error = Record.Exception(action);
                Assert.True(group.Name.StartsWith("invalid", StringComparison.Ordinal) ? error is FormatException : error == null,
                    group.Name + ": " + entry.GetProperty("name").GetString() + ": " + error);
                if (group.Name == "invalidResponsesWithValidReceipt")
                {
                    var receipt = serializer == "SystemText" ? InvokeReadJson.DeserializeReceipt(json, request) : Allors.Protocol.Json.Newtonsoft.InvokeReadJson.DeserializeReceipt(json, request);
                    using var raw = JsonDocument.Parse(json);
                    Assert.Equal(raw.RootElement.GetProperty("invoke").GetProperty("k").GetString(), receipt.invoke.k);
                }
            }
            Assert.Equal(72, count);
        }

        private static ReadRequest DecodeReadRequest(string json, string serializer) => serializer switch
        {
            "SystemText" => Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeRequest(json),
            "Newtonsoft" => Allors.Protocol.Json.Newtonsoft.ReadJson.DeserializeRequest(json),
            _ => throw new ArgumentException("Select a supported protocol serializer.", nameof(serializer)),
        };

        private static object DecodeReadResponse(string json, ReadRequest request, string serializer) => serializer switch
        {
            "SystemText" => Allors.Protocol.Json.SystemTextJson.ReadJson.DeserializeResponse(json, request),
            "Newtonsoft" => Allors.Protocol.Json.Newtonsoft.ReadJson.DeserializeResponse(json, request),
            _ => throw new ArgumentException("Select a supported protocol serializer.", nameof(serializer)),
        };

        private static T DeserializeReadContract<T>(string json, string serializer) => serializer switch
        {
            "SystemText" => JsonSerializer.Deserialize<T>(json),
            "SystemTextWeb" => JsonSerializer.Deserialize<T>(json, JsonSerializerOptions.Web),
            "Newtonsoft" => JsonConvert.DeserializeObject<T>(json),
            _ => throw new ArgumentException("Select a supported protocol serializer.", nameof(serializer)),
        };

        private static string SerializeReadContract(object value, string serializer) => serializer switch
        {
            "SystemText" => JsonSerializer.Serialize(value),
            "SystemTextWeb" => JsonSerializer.Serialize(value, JsonSerializerOptions.Web),
            "Newtonsoft" => JsonConvert.SerializeObject(value),
            _ => throw new ArgumentException("Select a supported protocol serializer.", nameof(serializer)),
        };

        private static T AssertReadContractFixture<T>(JsonDocument fixture, string name, T value, string serializer)
        {
            var expected = fixture.RootElement.GetProperty(name).GetRawText();
            AssertJsonEqual(expected, SerializeReadContract(value, serializer));
            var decoded = DeserializeReadContract<T>(expected, serializer);
            AssertJsonEqual(expected, SerializeReadContract(decoded, serializer));
            return decoded;

            void AssertJsonEqual(string expectedJson, string actualJson) => Assert.True(
                JsonNode.DeepEquals(JsonNode.Parse(expectedJson), JsonNode.Parse(actualJson)),
                $"{serializer} did not preserve {name}. Expected: {expectedJson}. Actual: {actualJson}");
        }

        private static ReadScope CreateReadScope() => new ReadScope
        {
            p = "population-1", u = 7, w = "Default", r = "schema-1", x = "tenant=blue",
        };

        private static ReadPull CreateReadPull() => new ReadPull
        {
            l = new[]
            {
                new Allors.Protocol.Json.Data.Pull { t = "C1", o = 101 },
                new Allors.Protocol.Json.Data.Pull { t = "C1", o = 999 },
                new Allors.Protocol.Json.Data.Pull
                {
                    e = new Allors.Protocol.Json.Data.Extent { k = Allors.Protocol.Json.Data.ExtentKind.Filter, t = "C1" },
                },
            },
            d = new[] { new Allors.Protocol.Json.Api.Pull.PullDependency { o = "C1", r = "Related" } },
        };

        private static Dictionary<string, object> CreateReadUnitValues()
        {
            var convert = new Allors.Protocol.Json.SystemTextJson.UnitConvert();
            return new Dictionary<string, object>
            {
                ["binary"] = convert.ToJson(new byte[] { 0, 1, 2, 255 }),
                ["boolean"] = convert.ToJson(false),
                ["dateTime"] = convert.ToJson(new DateTime(2026, 10, 10, 12, 34, 56, DateTimeKind.Utc)),
                ["decimal"] = convert.ToJson(123.4500m),
                ["float"] = convert.ToJson(1.5d),
                ["integer"] = convert.ToJson(42),
                ["string"] = convert.ToJson("hello"),
                ["unique"] = convert.ToJson(new Guid("01234567-89ab-cdef-0123-456789abcdef")),
                ["nothing"] = convert.ToJson(null),
            };
        }

        private sealed class PullSink(bool throwAfterPull) : ISink
        {
            public PullEvent PullEvent { get; private set; }

            public long UserId { get; private set; }

            public long UncommittedId { get; private set; }

            public C1 UncommittedObject { get; private set; }

            public void OnBefore(IEvent @event)
            {
                if (@event is PullEvent pullEvent)
                {
                    this.PullEvent = pullEvent;
                    this.UserId = pullEvent.Transaction.Services.Get<IUserService>().User.Id;
                }
            }

            public void OnAfter(IEvent @event)
            {
                if (@event is PullEvent pullEvent)
                {
                    this.UncommittedObject = pullEvent.Transaction.Create<C1>();
                    this.UncommittedId = this.UncommittedObject.Id;
                    if (throwAfterPull)
                    {
                        throw new InvalidOperationException("Test pull failure");
                    }
                }
            }
        }
    }
}
