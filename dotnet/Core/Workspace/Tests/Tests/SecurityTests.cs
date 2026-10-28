// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Workspace;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class SecurityTests : Test
    {
        protected SecurityTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        protected abstract IWorkspace CreateWorkspace(HttpClient client);

        protected abstract IPullResult CreatePullResult(ISession session, PullResponse response);

        protected abstract object GetCachedRecord(IWorkspace workspace, long id);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task HybridSyncKeepsSessionSnapshotsAndUsesExactKnownEntries(bool useRevocation)
        {
            long dataVersion = 1;
            long grantVersion = 1;
            var syncCount = 0;
            using var handler = new ResponseHandler(async request =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/pull"))
                {
                    var manifest = Manifest(dataVersion, useRevocation ? 1 : grantVersion);
                    if (useRevocation)
                    {
                        manifest.p[0].r = new long[] { 21 };
                        manifest.r = new[] { new long[] { 21, grantVersion } };
                    }

                    return Json(manifest);
                }
                if (request.RequestUri.AbsolutePath.EndsWith("/push"))
                {
                    var pushRequest = JsonSerializer.Deserialize<PushRequest>(await request.Content.ReadAsStringAsync());
                    Assert.Empty(pushRequest.o ?? Array.Empty<PushRequestObject>());
                    return Json(new PushResponse());
                }

                Assert.EndsWith("/sync", request.RequestUri.AbsolutePath);
                var hint = JsonSerializer.Deserialize<SyncRequest>(await request.Content.ReadAsStringAsync());
                syncCount++;
                var response = this.Snapshot(dataVersion, useRevocation ? 1 : grantVersion, useRevocation || grantVersion == 1);
                if (useRevocation)
                {
                    response.o[0].r = new long[] { 21 };
                    response.r = new[] { new AccessResponseRevocation { i = 21, v = grantVersion, p = grantVersion == 1 ? Array.Empty<long>() : new long[] { 31, 35 } } };
                    if (grantVersion == 2) response.o[0].ro = response.o[0].ro.Where(role => role.t != this.M.C1.C1AllorsString.RelationType.Tag).ToArray();
                }
                if (dataVersion == 2)
                {
                    Assert.Equal(new[] { new long[] { 11, useRevocation ? 1 : 2 } }, hint.g);
                    response.g[0].p = null;
                    if (useRevocation)
                    {
                        Assert.Equal(new[] { new long[] { 21, 2 } }, hint.r);
                        response.r[0].p = null;
                    }

                    response.p = Array.Empty<PermissionResponsePermission>();
                }
                else if (useRevocation && grantVersion == 2) Assert.Equal(new[] { new long[] { 11, 1 } }, hint.g);
                else Assert.Empty(hint.g);
                return Json(response);
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var workspace = this.CreateWorkspace(client);
            var first = workspace.CreateSession();
            var second = workspace.CreateSession();
            var pull = new Pull { ObjectId = 1 };
            var original = (await first.PullAsync(pull)).GetObject<C1>();
            var binary = original.C1AllorsBinary;
            binary[0] = 99;
            Assert.Equal(new byte[] { 1, 2 }, original.C1AllorsBinary);
            Assert.Equal("value1", original.C1AllorsString);
            original.C1AllorsString = "local";
            grantVersion = 2;
            var refreshed = (await second.PullAsync(pull)).GetObject<C1>();
            Assert.False(refreshed.CanReadC1AllorsString);
            Assert.False(refreshed.CanExecuteClassMethod);
            Assert.True(original.CanReadC1AllorsString);
            Assert.True(original.CanExecuteClassMethod);
            Assert.Equal("local", original.C1AllorsString);
            first.Reset();
            Assert.Equal("value1", original.C1AllorsString);
            original.C1AllorsString = "local";
            Assert.False((await first.PullAsync(pull)).HasErrors);
            Assert.True(original.CanReadC1AllorsString);
            Assert.True(original.CanExecuteClassMethod);
            dataVersion = 2;
            Assert.False((await first.PullAsync(pull)).HasErrors);
            Assert.Equal(2, original.Strategy.Version);
            Assert.Equal("local", original.C1AllorsString);
            Assert.True(original.CanReadC1AllorsString);
            original.Strategy.RestoreRole(this.M.C1.C1AllorsString);
            first.Checkpoint();
            first.Reset();
            Assert.False(original.CanReadC1AllorsString);
            Assert.False(original.CanExecuteClassMethod);
            Assert.Null(original.C1AllorsString);
            Assert.Equal(3, syncCount);
            Assert.Equal(3, original.Strategy.Version);
            Assert.True(original.CanWriteC1AllorsString);
            Assert.False((await first.PushAsync()).HasErrors);
            Assert.True(original.CanWriteC1AllorsString);
            Assert.False((await first.PullAsync(pull)).HasErrors);
            Assert.Equal(3, original.Strategy.Version);
            Assert.Equal(3, syncCount);
        }

        [Fact]
        public async Task HybridSessionsLoadConcurrentlyAndRetainTheirSelectedSnapshots()
        {
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pullCount = 0;
            var syncCount = 0;
            using var handler = new ResponseHandler(async request =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/pull"))
                {
                    var version = Interlocked.Increment(ref pullCount);
                    return Json(Manifest(version, version));
                }

                Assert.EndsWith("/sync", request.RequestUri.AbsolutePath);
                var snapshot = Interlocked.Increment(ref syncCount);
                if (snapshot == 1)
                {
                    entered.SetResult(true);
                    await release.Task;
                }

                return Json(this.Snapshot(snapshot, snapshot, snapshot == 1));
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var workspace = this.CreateWorkspace(client);
            var first = workspace.CreateSession();
            var second = workspace.CreateSession();
            var pending = first.PullAsync(new Pull { ObjectId = 1 });
            C1 newer;
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                newer = (await second.PullAsync(new Pull { ObjectId = 1 }).WaitAsync(TimeSpan.FromSeconds(10))).GetObject<C1>();
                Assert.Equal(3, newer.Strategy.Version);
                Assert.False(newer.CanReadC1AllorsString);
            }
            finally
            {
                release.TrySetResult(true);
            }

            var older = (await pending.WaitAsync(TimeSpan.FromSeconds(10))).GetObject<C1>();
            Assert.Equal(2, older.Strategy.Version);
            Assert.True(older.CanReadC1AllorsString);
            older.C1AllorsString = "local";
            first.Reset();
            second.Reset();
            Assert.Equal("value1", older.C1AllorsString);
            Assert.Equal(1, older.C1AllorsInteger);
            Assert.Equal(3, newer.Strategy.Version);
            Assert.Equal(2, newer.C1AllorsInteger);
            Assert.False(newer.CanReadC1AllorsString);
            Assert.Equal(2, syncCount);
        }

        [Theory]
        [InlineData("data")]
        [InlineData("grant")]
        [InlineData("revocation")]
        public async Task HybridCachedSelectionsDoNotReplaceConcurrentPublishedRecords(string change)
        {
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var pullCount = 0;
            var syncCount = 0;
            var newerDataVersion = change == "data" ? 2 : 1;
            var newerSecurityVersion = change == "data" ? 1 : 2;
            PullResponse Selection(long dataVersion, long securityVersion)
            {
                var selection = Manifest(dataVersion, change == "grant" ? securityVersion : 1);
                if (change == "revocation")
                {
                    selection.p[0].r = new long[] { 21 };
                    selection.r = new[] { new long[] { 21, securityVersion } };
                }

                return selection;
            }

            SyncResponse Complete(long dataVersion, long securityVersion)
            {
                var snapshot = this.Snapshot(dataVersion, change == "grant" ? securityVersion : 1, change != "grant" || securityVersion == 1);
                if (change == "revocation")
                {
                    snapshot.o[0].r = new long[] { 21 };
                    snapshot.r = new[] { new AccessResponseRevocation { i = 21, v = securityVersion, p = securityVersion == 1 ? Array.Empty<long>() : new long[] { 31 } } };
                    if (securityVersion != 1) snapshot.o[0].ro = snapshot.o[0].ro.Where(role => role.t != this.M.C1.C1AllorsString.RelationType.Tag).ToArray();
                }

                return snapshot;
            }

            using var handler = new ResponseHandler(async request =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/pull"))
                {
                    var call = Interlocked.Increment(ref pullCount);
                    var selection = call <= 2 ? Selection(1, 1) : Selection(newerDataVersion, newerSecurityVersion);
                    if (call == 2)
                    {
                        selection.p = selection.p.Append(new PullResponseObject { i = 2, v = 2, g = new long[] { 11 }, r = selection.p[0].r }).ToArray();
                        selection.o.Add("Y", 2);
                    }

                    return Json(selection);
                }

                Assert.EndsWith("/sync", request.RequestUri.AbsolutePath);
                var sync = Interlocked.Increment(ref syncCount);
                var syncRequest = JsonSerializer.Deserialize<SyncRequest>(await request.Content.ReadAsStringAsync());
                if (sync == 2)
                {
                    Assert.Equal(new long[] { 2 }, syncRequest.o);
                    entered.SetResult(true);
                    await release.Task;
                    var snapshot = Complete(1, 1);
                    snapshot.o[0].i = 2;
                    return Json(snapshot);
                }

                Assert.Equal(new long[] { 1 }, syncRequest.o);
                return Json(sync == 1 ? Complete(1, 1) : Complete(newerDataVersion, newerSecurityVersion));
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var workspace = this.CreateWorkspace(client);
            var first = workspace.CreateSession();
            var second = workspace.CreateSession();
            var pull = new Pull { ObjectId = 1 };
            var original = (await first.PullAsync(pull)).GetObject<C1>();
            original.C1AllorsString = "local";
            var pending = first.PullAsync(pull);
            object published = null;
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                var newer = (await second.PullAsync(pull).WaitAsync(TimeSpan.FromSeconds(10))).GetObject<C1>();
                Assert.Equal(newerDataVersion + 1, newer.Strategy.Version);
                Assert.Equal(change == "data", newer.CanReadC1AllorsString);
                published = this.GetCachedRecord(workspace, 1);
            }
            finally
            {
                release.TrySetResult(true);
            }

            var completed = await pending.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Same(published, this.GetCachedRecord(workspace, 1));
            Assert.Same(original, completed.GetObject<C1>());
            Assert.Equal("local", original.C1AllorsString);
            Assert.Equal(2, original.Strategy.Version);
            Assert.True(original.CanReadC1AllorsString);
            Assert.NotNull(this.GetCachedRecord(workspace, 2));
            Assert.Equal(2, completed.GetObject<C1>("Y").Strategy.Version);
            Assert.Equal("value1", completed.GetObject<C1>("Y").C1AllorsString);
            original.Strategy.Reset();
            Assert.Equal("value1", original.C1AllorsString);
            Assert.Equal(2, original.Strategy.Version);
            Assert.True(original.CanReadC1AllorsString);

            var cached = (await workspace.CreateSession().PullAsync(pull)).GetObject<C1>();
            Assert.Equal(newerDataVersion + 1, cached.Strategy.Version);
            Assert.Equal(change == "data", cached.CanReadC1AllorsString);
            Assert.Same(published, this.GetCachedRecord(workspace, 1));
            Assert.Equal(3, syncCount);
        }

        [Theory]
        [InlineData("missing-table")]
        [InlineData("unadvertised-entry")]
        [InlineData("missing-definition")]
        [InlineData("duplicate-entry")]
        [InlineData("missing-object")]
        [InlineData("invalid-role")]
        [InlineData("result-name")]
        [InlineData("pull-version-zero")]
        [InlineData("pull-version-initial")]
        [InlineData("sync-version-zero")]
        [InlineData("sync-version-initial")]
        [InlineData("missing-role-table")]
        [InlineData("unknown-role")]
        [InlineData("duplicate-role")]
        [InlineData("wrong-class-role")]
        [InlineData("object-conflict")]
        [InlineData("class-conflict")]
        [InlineData("grant-known-conflict")]
        [InlineData("grant-publication-conflict")]
        [InlineData("revocation-known-conflict")]
        [InlineData("revocation-publication-conflict")]
        [InlineData("permission-conflict")]
        [InlineData("permission-identity-conflict")]
        [InlineData("pull-grants-order")]
        [InlineData("pull-revocations-order")]
        [InlineData("sync-grants-order")]
        [InlineData("sync-revocations-order")]
        [InlineData("grant-permissions-order")]
        [InlineData("revoked-permissions-order")]
        [InlineData("many-role-order")]
        public async Task HybridSyncRejectsIncompleteResponsesBeforePublishing(string failure)
        {
            var stage = 0;
            var syncCount = 0;
            var withRevocation = failure.StartsWith("revocation-", StringComparison.Ordinal);
            var expectedMessages = new Dictionary<string, string>
            {
                ["unknown-role"] = $"Sync object 1 (class '{this.M.C1.Tag}') references unknown role tag 'unknown-role'. Check that the client and server use matching workspace models; regenerate your workspace if needed.",
                ["duplicate-role"] = $"Sync object 1 (class '{this.M.C1.Tag}') contains duplicate role tag '{this.M.C1.C1AllorsInteger.RelationType.Tag}'. Return each role only once.",
                ["wrong-class-role"] = $"Sync object 1 (class '{this.M.C1.Tag}') contains role tag '{this.M.Denied.DefaultWorkspaceProperty.RelationType.Tag}' that is not a database role of this class.",
                ["object-conflict"] = "Sync changed the contents of object 1 at data version 2 without changing its security versions.",
                ["class-conflict"] = $"Sync changed the class of object 1 from '{this.M.C1.Tag}' to '{this.M.Denied.Tag}'.",
                ["grant-known-conflict"] = "Sync changed the contents of grant 11 without changing version 1.",
                ["grant-publication-conflict"] = "Sync changed the contents of grant 11 without changing version 1.",
                ["revocation-known-conflict"] = "Sync changed the contents of revocation 21 without changing version 1.",
                ["revocation-publication-conflict"] = "Sync changed the contents of revocation 21 without changing version 1.",
                ["permission-conflict"] = "Sync changed the definition of permission 32.",
                ["permission-identity-conflict"] = $"Sync permission 36 conflicts with cached permission 32 for class '{this.M.C1.Tag}', operand '{this.M.C1.C1AllorsString.RelationType.Tag}' and operation {(long)Operations.Write}.",
            };
            using var handler = new ResponseHandler(async request =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/pull"))
                {
                    var response = Manifest(stage == 0 ? 1 : 2, stage == 0 ? 1 : 2);
                    if (stage == 1 && failure == "grant-known-conflict") response.g[0][1] = 1;
                    if (withRevocation)
                    {
                        response.p[0].r = new long[] { 21 };
                        response.r = new[] { new long[] { 21, stage == 0 || stage == 1 && failure == "revocation-known-conflict" ? 1 : 2 } };
                    }
                    if (stage == 1 && failure == "result-name") response.o.Add("c1", 1);
                    if (stage == 1 && failure == "pull-version-zero") response.p[0].v = 0;
                    if (stage == 1 && failure == "pull-version-initial") response.p[0].v = 1;
                    if (stage == 1 && failure == "invalid-role") response.p = new[] { response.p[0], new PullResponseObject { i = 2, v = 3, g = new long[] { 11 } } };
                    if (stage > 0 && (failure == "pull-grants-order" || failure == "sync-grants-order"))
                    {
                        response.g = new[] { new long[] { 12, 2 }, response.g[0] };
                        response.p[0].g = stage == 1 && failure == "pull-grants-order" ? new long[] { 12, 11 } : new long[] { 11, 12 };
                    }
                    if (stage > 0 && (failure == "pull-revocations-order" || failure == "sync-revocations-order"))
                    {
                        response.r = new[] { new long[] { 22, 2 }, new long[] { 21, 2 } };
                        response.p[0].r = stage == 1 && failure == "pull-revocations-order" ? new long[] { 22, 21 } : new long[] { 21, 22 };
                    }
                    if (stage > 0 && failure == "revoked-permissions-order")
                    {
                        response.r = new[] { new long[] { 21, 2 } };
                        response.p[0].r = new long[] { 21 };
                    }
                    if (stage > 0 && failure == "many-role-order")
                    {
                        response.p = response.p.Concat(new[] { new PullResponseObject { i = 2, v = 3, g = new long[] { 11 } }, new PullResponseObject { i = 3, v = 3, g = new long[] { 11 } } }).ToArray();
                        response.c.Add("Ordered", new long[] { 3, 1, 2, 3 });
                        response.o.Add("None", 0);
                    }
                    return Json(response);
                }

                Assert.EndsWith("/sync", request.RequestUri.AbsolutePath);
                ++syncCount;
                var hint = JsonSerializer.Deserialize<SyncRequest>(await request.Content.ReadAsStringAsync());
                Assert.DoesNotContain(hint.g, pair => pair[1] == 2);
                var snapshot = this.Snapshot(stage == 0 ? 1 : 2, stage == 0 ? 1 : 2, stage == 0);
                if (withRevocation)
                {
                    snapshot.o[0].r = new long[] { 21 };
                    snapshot.r = new[] { new AccessResponseRevocation { i = 21, v = stage < 2 ? 1 : 2, p = stage == 1 ? new long[] { 32 } : Array.Empty<long>() } };
                }
                if (stage > 0 && (failure == "pull-grants-order" || failure == "sync-grants-order"))
                {
                    snapshot.g = new[] { new AccessResponseGrant { i = 12, v = 2, p = Array.Empty<long>() }, snapshot.g[0] };
                    snapshot.o[0].g = stage == 1 && failure == "sync-grants-order" ? new long[] { 12, 11 } : new long[] { 11, 12 };
                }
                if (stage > 0 && (failure == "pull-revocations-order" || failure == "sync-revocations-order"))
                {
                    snapshot.r = new[] { new AccessResponseRevocation { i = 22, v = 2, p = Array.Empty<long>() }, new AccessResponseRevocation { i = 21, v = 2, p = Array.Empty<long>() } };
                    snapshot.o[0].r = stage == 1 && failure == "sync-revocations-order" ? new long[] { 22, 21 } : new long[] { 21, 22 };
                }
                if (stage > 0 && failure == "revoked-permissions-order")
                {
                    snapshot.r = new[] { new AccessResponseRevocation { i = 21, v = 2, p = stage == 1 ? new long[] { 33, 32 } : new long[] { 32, 33 } } };
                    snapshot.o[0].r = new long[] { 21 };
                }
                if (stage > 0 && failure == "many-role-order")
                {
                    snapshot.p = snapshot.p.Append(new PermissionResponsePermission { i = 36, c = this.M.C1.Tag, t = this.M.C1.C1C1Many2Manies.RelationType.Tag, o = (long)Operations.Read }).ToArray();
                    snapshot.g[0].p = snapshot.g[0].p.Append(36L).ToArray();
                    snapshot.o[0].ro = snapshot.o[0].ro.Append(new SyncResponseRole { t = this.M.C1.C1C1Many2Manies.RelationType.Tag, c = stage == 1 ? new long[] { 3, 2 } : new long[] { 2, 3 } }).ToArray();
                    snapshot.o = snapshot.o.Concat(new[] { new SyncResponseObject { i = 2, v = 3, c = this.M.C1.Tag, g = new long[] { 11 }, ro = Array.Empty<SyncResponseRole>() }, new SyncResponseObject { i = 3, v = 3, c = this.M.C1.Tag, g = new long[] { 11 }, ro = Array.Empty<SyncResponseRole>() } }).ToArray();
                }
                if (stage == 1)
                {
                    switch (failure)
                    {
                        case "missing-table": snapshot.r = null; break;
                        case "sync-version-zero": snapshot.o[0].v = 0; break;
                        case "sync-version-initial": snapshot.o[0].v = 1; break;
                        case "missing-role-table": snapshot.o[0].ro = null; break;
                        case "unadvertised-entry": snapshot.g[0].p = null; break;
                        case "missing-definition": snapshot.p = Array.Empty<PermissionResponsePermission>(); break;
                        case "duplicate-entry": snapshot.g = new[] { snapshot.g[0], snapshot.g[0] }; break;
                        case "missing-object": snapshot.o = Array.Empty<SyncResponseObject>(); break;
                        case "grant-permissions-order": snapshot.g[0].p = snapshot.g[0].p.Reverse().ToArray(); break;
                        case "unknown-role": snapshot.o[0].ro = snapshot.o[0].ro.Append(new SyncResponseRole { t = "unknown-role", v = "ignored" }).ToArray(); break;
                        case "duplicate-role": snapshot.o[0].ro = snapshot.o[0].ro.Append(snapshot.o[0].ro[0]).ToArray(); break;
                        case "wrong-class-role": snapshot.o[0].ro = snapshot.o[0].ro.Append(new SyncResponseRole { t = this.M.Denied.DefaultWorkspaceProperty.RelationType.Tag, v = "ignored" }).ToArray(); break;
                        case "object-conflict":
                            snapshot = this.Snapshot(1, 1, true);
                            snapshot.o[0].ro[0].v = "changed";
                            break;
                        case "class-conflict":
                            snapshot.o[0].c = this.M.Denied.Tag;
                            snapshot.o[0].ro = Array.Empty<SyncResponseRole>();
                            break;
                        case "grant-known-conflict":
                        case "grant-publication-conflict": snapshot.g[0].v = 1; break;
                        case "permission-conflict":
                            snapshot.p[0].t = this.M.C1.ClassMethod.Tag;
                            snapshot.p[0].o = (long)Operations.Execute;
                            break;
                        case "permission-identity-conflict":
                            snapshot.p[0].i = 36;
                            snapshot.g[0].p = snapshot.g[0].p.Select(id => id == 32 ? 36 : id).OrderBy(id => id).ToArray();
                            break;
                        case "invalid-role":
                            var invalid = this.Snapshot(2, 2, false).o[0];
                            invalid.i = 2;
                            invalid.ro.Last().v = "not-base64";
                            snapshot.o = new[] { snapshot.o[0], invalid };
                            break;
                    }
                }

                return Json(snapshot);
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var session = this.CreateWorkspace(client).CreateSession();
            var pull = new Pull { ObjectId = 1 };
            var value = (await session.PullAsync(pull)).GetObject<C1>();
            value.C1AllorsString = "local";
            stage = 1;
            var error = await Assert.ThrowsAnyAsync<Exception>(() => session.PullAsync(pull));
            Assert.Equal(failure.StartsWith("pull-", StringComparison.Ordinal) || failure.StartsWith("result-", StringComparison.Ordinal) ? 1 : 2, syncCount);
            if (failure.EndsWith("-order", StringComparison.Ordinal)) Assert.Equal("An object or security reference must contain sorted, distinct positive IDs.", error.Message);
            if (expectedMessages.TryGetValue(failure, out var expectedMessage)) Assert.Equal(expectedMessage, error.Message);
            Assert.Equal("local", value.C1AllorsString);
            Assert.True(value.CanReadC1AllorsString);
            session.Reset();
            Assert.Equal("value1", value.C1AllorsString);
            Assert.Equal(2, value.Strategy.Version);
            stage = 2;
            var recovered = await session.PullAsync(pull);
            Assert.False(recovered.HasErrors);
            Assert.Equal(3, value.Strategy.Version);
            Assert.False(value.CanReadC1AllorsString);
            if (failure == "many-role-order")
            {
                Assert.Equal(new long[] { 2, 3 }, value.C1C1Many2Manies.Select(target => target.Id));
                Assert.Equal(new long[] { 3, 1, 2, 3 }, recovered.GetCollection<C1>("Ordered").Select(target => target.Id));
                Assert.Null(recovered.GetObject<C1>("None"));
            }
        }

        [Fact]
        public async Task HybridSessionOperationsKeepOrderAndRecoverAfterFailure()
        {
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var paths = new List<string>();
            using var handler = new ResponseHandler(async request =>
            {
                var path = request.RequestUri.AbsolutePath;
                paths.Add(path);
                if (paths.Count == 1)
                {
                    entered.SetResult(true);
                    await release.Task;
                    return Json(new PullResponse { p = null });
                }

                if (path == "/pull") return Json(Manifest(1, 1));
                if (path == "/sync") return Json(this.Snapshot(1, 1, true));
                Assert.Contains(path, new[] { "/push", "/invoke" });
                return Json(new { });
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var session = this.CreateWorkspace(client).CreateSession();
            var first = session.PullAsync(new Pull { ObjectId = 1 });
            await entered.Task;
            var second = session.PullAsync(new Pull { ObjectId = 1 });
            var push = session.PushAsync();
            var invoke = session.InvokeAsync(Array.Empty<Method>());
            Assert.Single(paths);
            release.SetResult(true);
            await Assert.ThrowsAsync<InvalidOperationException>(() => first);
            Assert.False((await second).HasErrors);
            Assert.False((await push).HasErrors);
            Assert.False((await invoke).HasErrors);
            Assert.Equal(new[] { "/pull", "/pull", "/sync", "/push", "/invoke" }, paths);
        }

        [Fact]
        public async Task HybridPullResultsOwnTheirNamedBuffersBeforeMaterialization()
        {
            using var handler = new ResponseHandler(request => Task.FromResult(Json(
                request.RequestUri.AbsolutePath.EndsWith("/pull") ? (object)Manifest(1, 1) : this.Snapshot(1, 1, true))));
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var workspace = this.CreateWorkspace(client);
            var session = workspace.CreateSession();
            var value = (await session.PullAsync(new Pull { ObjectId = 1 })).GetObject<C1>();
            var response = Manifest(1, 1);
            response.c.Add("C1s", new long[] { 1 });
            var bytes = new byte[] { 1, 2 };
            response.v.Add("Bytes", bytes);
            var names = new[] { "original" };
            var binaries = new[] { new byte[] { 3, 4 } };
            response.v.Add("Names", names);
            response.v.Add("Binaries", binaries);
            var result = this.CreatePullResult(session, response);

            response.o["C1"] = 0;
            response.c["C1s"][0] = 0;
            bytes[0] = 99;
            names[0] = "changed";
            binaries[0][0] = 99;

            Assert.Same(value, result.GetObject<C1>());
            Assert.Equal(new[] { value }, result.GetCollection<C1>());
            Assert.Equal(new byte[] { 1, 2 }, result.GetValue<byte[]>("Bytes"));
            Assert.Equal(new[] { "original" }, result.GetValue<string[]>("Names"));
            Assert.Equal(new byte[] { 3, 4 }, Assert.Single(result.GetValue<byte[][]>("Binaries")));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task HybridCheckpointsPreserveUnloadedRelationChanges(bool many, bool reset)
        {
            var version = 1;
            Allors.Workspace.Meta.IRoleType roleType = many ? this.M.C1.C1C1Many2Manies : this.M.C1.C1C1Many2One;
            using var handler = new ResponseHandler(request =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/pull")) return Task.FromResult(Json(Manifest(version, 1)));
                var snapshot = this.Snapshot(version, 1, true);
                snapshot.p = snapshot.p.Append(new PermissionResponsePermission { i = 36, c = this.M.C1.Tag, t = roleType.RelationType.Tag, o = (long)Operations.Read }).ToArray();
                snapshot.g[0].p = snapshot.g[0].p.Append(36L).ToArray();
                snapshot.o[0].ro = snapshot.o[0].ro.Append(many
                    ? new SyncResponseRole { t = roleType.RelationType.Tag, c = new long[] { version + 1 } }
                    : new SyncResponseRole { t = roleType.RelationType.Tag, o = version + 1 }).ToArray();
                return Task.FromResult(Json(snapshot));
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var session = this.CreateWorkspace(client).CreateSession();
            var value = (await session.PullAsync(new Pull { ObjectId = 1 })).GetObject<C1>();
            if (reset) value.C1AllorsString = "local";
            session.Checkpoint();
            version = 2;
            await session.PullAsync(new Pull { ObjectId = 1 });
            if (reset) value.Strategy.Reset();
            var changes = session.Checkpoint();

            Assert.True(changes.AssociationsByRoleType.TryGetValue(roleType, out var changed));
            Assert.Contains(value.Strategy, changed);
            Assert.All(changes.RolesByAssociationType.Values, targets => Assert.DoesNotContain(null, targets));
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, false)]
        [InlineData(2, false)]
        [InlineData(1, true)]
        public async Task HybridPullNotificationsObserveCompleteSession(int initialObjectCount, bool throwFromHandler)
        {
            var version = 1;
            using var handler = new ResponseHandler(request =>
            {
                var includeTarget = version > 1 || initialObjectCount == 2;
                if (request.RequestUri.AbsolutePath.EndsWith("/pull"))
                {
                    var manifest = Manifest(version, 1);
                    if (includeTarget) manifest.p = manifest.p.Append(new PullResponseObject { i = 2, v = version + 1, g = new long[] { 11 } }).ToArray();
                    return Task.FromResult(Json(manifest));
                }

                Assert.EndsWith("/sync", request.RequestUri.AbsolutePath);
                var snapshot = this.Snapshot(version, 1, true);
                snapshot.p = snapshot.p.Append(new PermissionResponsePermission { i = 36, c = this.M.C1.Tag, t = this.M.C1.C1C1Many2One.RelationType.Tag, o = (long)Operations.Read }).ToArray();
                snapshot.g[0].p = snapshot.g[0].p.Append(36L).ToArray();
                if (version > 1) snapshot.o[0].ro = snapshot.o[0].ro.Append(new SyncResponseRole { t = this.M.C1.C1C1Many2One.RelationType.Tag, o = 2 }).ToArray();
                if (includeTarget)
                {
                    var target = this.Snapshot(version, 1, true).o[0];
                    target.i = 2;
                    snapshot.o = snapshot.o.Append(target).ToArray();
                }

                return Task.FromResult(Json(snapshot));
            });
            using var client = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
            var session = this.CreateWorkspace(client).CreateSession();
            var pull = new Pull { ObjectId = 1 };
            if (initialObjectCount > 0) await session.PullAsync(pull);
            session.Checkpoint();

            version = 2;
            var notifications = 0;
            var expectedException = new InvalidOperationException("Notification failure");
            EventHandler onChange = (_, _) =>
            {
                ++notifications;
                var source = session.Instantiate<C1>(1);
                Assert.Equal("value2", source.C1AllorsString);
                var target = source.C1C1Many2One;
                Assert.Equal(2, target.Id);
                Assert.Equal("value2", target.C1AllorsString);
                if (notifications != 1) return;

                var changes = session.Checkpoint();
                Assert.Equal(2 - initialObjectCount, changes.Instantiated.Count);
                if (initialObjectCount == 0) Assert.Contains(source.Strategy, changes.Instantiated);
                else
                {
                    Assert.Contains(source.Strategy, changes.AssociationsByRoleType[this.M.C1.C1AllorsString]);
                    Assert.Contains(source.Strategy, changes.AssociationsByRoleType[this.M.C1.C1C1Many2One]);
                    Assert.Contains(target.Strategy, changes.RolesByAssociationType[this.M.C1.C1C1Many2One.AssociationType]);
                }

                if (initialObjectCount < 2) Assert.Contains(target.Strategy, changes.Instantiated);
                else Assert.Contains(target.Strategy, changes.AssociationsByRoleType[this.M.C1.C1AllorsString]);
                if (throwFromHandler) throw expectedException;
            };
            session.OnChange += onChange;
            try
            {
                if (throwFromHandler) Assert.Same(expectedException, await Assert.ThrowsAsync<InvalidOperationException>(() => session.PullAsync(pull)));
                else Assert.False((await session.PullAsync(pull)).HasErrors);
            }
            finally
            {
                session.OnChange -= onChange;
            }

            Assert.Equal(throwFromHandler ? 1 : 2, notifications);
            Assert.Empty(session.Checkpoint().AssociationsByRoleType);
            if (throwFromHandler)
            {
                version = 3;
                var recoveredNotifications = 0;
                session.OnChange += (_, _) => ++recoveredNotifications;
                Assert.False((await session.PullAsync(pull).WaitAsync(TimeSpan.FromSeconds(10))).HasErrors);
                Assert.Equal(2, recoveredNotifications);
                Assert.Equal("value3", session.Instantiate<C1>(1).C1AllorsString);
                Assert.Equal("value3", session.Instantiate<C1>(2).C1AllorsString);
            }
        }

        private static PullResponse Manifest(long dataVersion, long grantVersion) => new PullResponse
        {
            p = new[] { new PullResponseObject { i = 1, v = dataVersion + 1, g = new long[] { 11 } } },
            g = new[] { new long[] { 11, grantVersion } },
            o = new Dictionary<string, long> { ["C1"] = 1 },
            c = new Dictionary<string, long[]>(),
            v = new Dictionary<string, object>(),
        };

        private SyncResponse Snapshot(long dataVersion, long grantVersion, bool readString)
        {
            var permissions = new List<PermissionResponsePermission>
            {
                new PermissionResponsePermission { i = 32, c = this.M.C1.Tag, t = this.M.C1.C1AllorsString.RelationType.Tag, o = (long)Operations.Write },
                new PermissionResponsePermission { i = 33, c = this.M.C1.Tag, t = this.M.C1.C1AllorsInteger.RelationType.Tag, o = (long)Operations.Read },
                new PermissionResponsePermission { i = 34, c = this.M.C1.Tag, t = this.M.C1.C1AllorsBinary.RelationType.Tag, o = (long)Operations.Read },
            };
            var roles = new List<SyncResponseRole>();
            if (readString)
            {
                permissions.Add(new PermissionResponsePermission { i = 31, c = this.M.C1.Tag, t = this.M.C1.C1AllorsString.RelationType.Tag, o = (long)Operations.Read });
                permissions.Add(new PermissionResponsePermission { i = 35, c = this.M.C1.Tag, t = this.M.C1.ClassMethod.Tag, o = (long)Operations.Execute });
                roles.Add(new SyncResponseRole { t = this.M.C1.C1AllorsString.RelationType.Tag, v = "value" + dataVersion });
            }

            roles.Add(new SyncResponseRole { t = this.M.C1.C1AllorsInteger.RelationType.Tag, v = dataVersion });
            roles.Add(new SyncResponseRole { t = this.M.C1.C1AllorsBinary.RelationType.Tag, v = "AQI=" });
            return new SyncResponse
            {
                o = new[] { new SyncResponseObject { i = 1, v = dataVersion + 1, c = this.M.C1.Tag, g = new long[] { 11 }, ro = roles.ToArray() } },
                g = new[] { new AccessResponseGrant { i = 11, v = grantVersion, p = permissions.Select(v => v.i).OrderBy(id => id).ToArray() } },
                r = Array.Empty<AccessResponseRevocation>(),
                p = permissions.ToArray(),
            };
        }

        private static HttpResponseMessage Json(object value) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json"),
        };

        private sealed class ResponseHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> response;
            internal ResponseHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> response) => this.response = response;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => this.response(request);
        }

        [Fact]
        public async Task RefreshesKnownGrantWhenAnObjectIsSynchronized()
        {
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.C1) });
            var c1 = result.GetCollection<C1>().First();
            Assert.True(c1.CanReadC1AllorsString);

            await this.Profile.ChangePermissions(c1.Id, "removeGrantRead");

            result = await session.PullAsync(new Pull { Object = c1 });
            Assert.False(result.HasErrors);
            Assert.False(c1.CanReadC1AllorsString);
        }

        [Fact]
        public async Task RefreshesKnownRevocationWhenAnObjectIsSynchronized()
        {
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.Denied) });
            var denied = Assert.Single(result.GetCollection<Denied>());
            Assert.True(denied.CanReadDefaultWorkspaceProperty);
            Assert.False(denied.CanWriteDefaultWorkspaceProperty);

            foreach (var denyRead in new[] { true, false })
            {
                await this.Profile.ChangePermissions(denied.Id, denyRead ? "addReadRevocation" : "removeReadRevocation");

                result = await session.PullAsync(new Pull { Object = denied });
                Assert.False(result.HasErrors);
                Assert.Equal(!denyRead, denied.CanReadDefaultWorkspaceProperty);
                Assert.False(denied.CanWriteDefaultWorkspaceProperty);
            }
        }

        [Fact]
        public async Task RejectedExistingObjectPushReturnsAccessErrorAndRollsBack()
        {
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.C1) });
            var c1 = result.GetCollection<C1>().First();
            var originalInteger = c1.C1AllorsInteger;
            var originalString = c1.C1AllorsString;
            Assert.True(c1.CanWriteC1AllorsString);
            c1.C1AllorsInteger = 2345;
            c1.C1AllorsString = "Rejected edit";

            await this.Profile.ChangePermissions(c1.Id, "removeGrantWrite");

            var pushResult = await session.PushAsync();
            Assert.True(pushResult.HasErrors);
            Assert.Same(c1, Assert.Single(pushResult.AccessErrors));
            Assert.Empty(pushResult.VersionErrors);
            Assert.Equal(2345, c1.C1AllorsInteger);
            Assert.Equal("Rejected edit", c1.C1AllorsString);

            var verification = this.Profile.CreateExclusiveWorkspace().CreateSession();
            var stored = await verification.PullAsync(new Pull { ObjectId = c1.Id });
            Assert.False(stored.HasErrors);
            var storedC1 = stored.GetObject<C1>();
            Assert.Equal(originalInteger, storedC1.C1AllorsInteger);
            Assert.Equal(originalString, storedC1.C1AllorsString);
        }

        [Fact]
        public async void WithGrant()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            var c1s = result.GetCollection<C1>("C1s");
            foreach (var c1 in result.GetCollection<C1>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    Assert.True(c1.Strategy.CanRead(roleType));
                    Assert.True(c1.Strategy.CanWrite(roleType));
                }
            }
        }

        [Fact]
        public async void WithGrantAsAgent()
        {
            await this.Login("agent");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            var c1s = result.GetCollection<C1>();
            Assert.NotEmpty(c1s);

            foreach (var c1 in c1s)
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    Assert.True(c1.Strategy.CanRead(roleType));
                    Assert.True(c1.Strategy.CanWrite(roleType));
                }
            }
        }

        [Fact]
        public async void WithoutAccessControl()
        {
            await this.Login("noacl");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            foreach (var c1 in result.GetCollection<C1>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    if (roleType.Origin == Origin.Database)
                    {
                        Assert.False(c1.Strategy.CanRead(roleType));
                        Assert.False(c1.Strategy.CanWrite(roleType));
                    }
                    else
                    {
                        Assert.True(c1.Strategy.CanRead(roleType));
                        Assert.True(c1.Strategy.CanWrite(roleType));
                    }
                }
            }
        }

        [Fact]
        public async void WithoutPermissions()
        {
            await this.Login("noperm");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.PullAsync(pull);

            foreach (var c1 in result.GetCollection<C1>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    if (roleType.Origin == Origin.Database)
                    {
                        Assert.False(c1.Strategy.CanRead(roleType));
                        Assert.False(c1.Strategy.CanWrite(roleType));
                    }
                    else
                    {
                        Assert.True(c1.Strategy.CanRead(roleType));
                        Assert.True(c1.Strategy.CanWrite(roleType));
                    }
                }
            }
        }

        [Fact]
        public async Task WithRevocation()
        {
            var session = this.Workspace.CreateSession();
            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.Denied) });
            Assert.False(result.HasErrors);
            var denieds = result.GetCollection<Denied>();
            Assert.NotEmpty(denieds);

            foreach (var denied in denieds)
            {
                Assert.True(denied.Strategy.CanRead(this.M.Denied.DefaultWorkspaceProperty));
                Assert.False(denied.Strategy.CanWrite(this.M.Denied.DefaultWorkspaceProperty));
            }
        }

        [Fact]
        public async void DeniedPermissions()
        {
            var session = this.Workspace.CreateSession();

            var result = await session.PullAsync(new Pull { Extent = new Filter(this.M.Denied) });

            foreach (var denied in result.GetCollection<Denied>())
            {
                foreach (var roleType in this.M.C1.DatabaseOriginRoleTypes)
                {
                    if (roleType.Origin == Origin.Database)
                    {
                        Assert.False(denied.Strategy.CanRead(roleType));
                        Assert.False(denied.Strategy.CanWrite(roleType));
                    }
                    else
                    {
                        Assert.True(denied.Strategy.CanRead(roleType));
                        Assert.True(denied.Strategy.CanWrite(roleType));
                    }
                }
            }
        }
    }
}
