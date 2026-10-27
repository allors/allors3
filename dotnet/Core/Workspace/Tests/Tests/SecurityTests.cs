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
        [InlineData("missing-table")]
        [InlineData("unadvertised-entry")]
        [InlineData("missing-definition")]
        [InlineData("duplicate-entry")]
        [InlineData("missing-object")]
        [InlineData("invalid-role")]
        [InlineData("result-name")]
        public async Task HybridSyncRejectsIncompleteResponsesBeforePublishing(string failure)
        {
            var stage = 0;
            using var handler = new ResponseHandler(async request =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/pull"))
                {
                    var response = Manifest(stage == 0 ? 1 : 2, stage == 0 ? 1 : 2);
                    if (stage == 1 && failure == "result-name") response.o.Add("c1", 1);
                    if (stage == 1 && failure == "invalid-role") response.p = new[] { response.p[0], new PullResponseObject { i = 2, v = 3, g = new long[] { 11 } } };
                    return Json(response);
                }

                Assert.EndsWith("/sync", request.RequestUri.AbsolutePath);
                var hint = JsonSerializer.Deserialize<SyncRequest>(await request.Content.ReadAsStringAsync());
                Assert.DoesNotContain(hint.g, pair => pair[1] == 2);
                var snapshot = this.Snapshot(stage == 0 ? 1 : 2, stage == 0 ? 1 : 2, stage == 0);
                if (stage == 1)
                {
                    switch (failure)
                    {
                        case "missing-table": snapshot.r = null; break;
                        case "unadvertised-entry": snapshot.g[0].p = null; break;
                        case "missing-definition": snapshot.p = Array.Empty<PermissionResponsePermission>(); break;
                        case "duplicate-entry": snapshot.g = new[] { snapshot.g[0], snapshot.g[0] }; break;
                        case "missing-object": snapshot.o = Array.Empty<SyncResponseObject>(); break;
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
            await Assert.ThrowsAnyAsync<Exception>(() => session.PullAsync(pull));
            Assert.Equal("local", value.C1AllorsString);
            Assert.True(value.CanReadC1AllorsString);
            session.Reset();
            Assert.Equal("value1", value.C1AllorsString);
            Assert.Equal(2, value.Strategy.Version);
            stage = 2;
            Assert.False((await session.PullAsync(pull)).HasErrors);
            Assert.Equal(3, value.Strategy.Version);
            Assert.False(value.CanReadC1AllorsString);
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
                g = new[] { new AccessResponseGrant { i = 11, v = grantVersion, p = permissions.Select(v => v.i).ToArray() } },
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
