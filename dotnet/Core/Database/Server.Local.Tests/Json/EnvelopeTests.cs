// <copyright file="EnvelopeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Allors;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Services;
    using Allors.Protocol.Json.Api;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Xunit;

    /// <summary>
    /// Every response says which database and user it is from and which workspace name and
    /// meta fingerprint the server used; a request that names another workspace or another
    /// fingerprint is refused, a request that names neither is served.
    /// </summary>
    public class EnvelopeTests : ApiTest, IClassFixture<Fixture>
    {
        private const string WorkspaceName = "Default";

        public EnvelopeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void EveryResponseCarriesTheEnvelope()
        {
            var user = this.SetUser("jane@example.com");
            var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
            var api = new Api(this.Transaction, WorkspaceName, CancellationToken.None);

            var responses = new Response[]
            {
                api.Pull(new PullRequest()),
                api.Sync(new SyncRequest { o = Array.Empty<long>() }),
                api.Access(new AccessRequest()),
                api.Permission(new PermissionRequest()),
                api.Push(new PushRequest()),
                api.Invoke(new InvokeRequest { l = Array.Empty<Invocation>() }),
            };

            foreach (var response in responses)
            {
                Assert.False(response.HasErrors);
                Assert.Equal(this.Transaction.Database.Id, response._db);
                Assert.Equal(user.Id, response._u);
                Assert.Equal(WorkspaceName, response._w);
                Assert.Equal(metaCache.GetWorkspaceFingerprint(WorkspaceName), response._f);
            }
        }

        [Fact]
        public void TheFingerprintIsTheHashOfTheSortedTagsOfTheWorkspaceMeta()
        {
            var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
            var m = this.Transaction.Database.MetaPopulation;

            var tags = m.Composites.Where(v => v.WorkspaceNames.Contains(WorkspaceName)).Select(v => v.Tag)
                .Concat(m.RelationTypes.Where(v => v.WorkspaceNames.Contains(WorkspaceName)).Select(v => v.Tag))
                .Concat(m.MethodTypes.Where(v => v.WorkspaceNames.Contains(WorkspaceName)).Select(v => v.Tag));

            var fingerprint = metaCache.GetWorkspaceFingerprint(WorkspaceName);

            Assert.Matches(new Regex("^[0-9a-f]{16}$"), fingerprint);
            Assert.Equal(MetaFingerprint.Compute(tags), fingerprint);
            Assert.NotEqual(fingerprint, metaCache.GetWorkspaceFingerprint("X"));
        }

        [Fact]
        public void ARequestForAnotherWorkspaceNameIsRefused()
        {
            this.SetUser("jane@example.com");
            var api = new Api(this.Transaction, WorkspaceName, CancellationToken.None);

            var response = api.Pull(new PullRequest { _w = "X", l = Array.Empty<Allors.Protocol.Json.Data.Pull>() });

            Assert.True(response.HasErrors);
            Assert.Contains("'X'", response._e);
            Assert.Contains("'Default'", response._e);
            Assert.Null(response.p);
            Assert.Equal(WorkspaceName, response._w);
            Assert.Equal(this.Transaction.Database.Id, response._db);

            var syncResponse = api.Sync(new SyncRequest { _w = "X", o = Array.Empty<long>() });
            Assert.True(syncResponse.HasErrors);
            Assert.Null(syncResponse.o);
        }

        [Fact]
        public void ARequestWithAnotherFingerprintIsRefused()
        {
            this.SetUser("jane@example.com");
            var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
            var api = new Api(this.Transaction, WorkspaceName, CancellationToken.None);

            var response = api.Pull(new PullRequest { _w = WorkspaceName, _f = "0000000000000000", l = Array.Empty<Allors.Protocol.Json.Data.Pull>() });

            Assert.True(response.HasErrors);
            Assert.Contains("0000000000000000", response._e);
            Assert.Contains(metaCache.GetWorkspaceFingerprint(WorkspaceName), response._e);
            Assert.Null(response.p);
            Assert.Equal(metaCache.GetWorkspaceFingerprint(WorkspaceName), response._f);
        }

        [Fact]
        public void ARequestThatNamesNeitherIsServed()
        {
            this.SetUser("jane@example.com");
            var api = new Api(this.Transaction, WorkspaceName, CancellationToken.None);

            var response = api.Pull(new PullRequest { l = Array.Empty<Allors.Protocol.Json.Data.Pull>() });

            Assert.False(response.HasErrors);
            Assert.NotNull(response.p);
        }

        [Fact]
        public void ARequestThatNamesTheSameWorkspaceAndFingerprintIsServed()
        {
            this.SetUser("jane@example.com");
            var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
            var api = new Api(this.Transaction, WorkspaceName, CancellationToken.None);

            var response = api.Pull(new PullRequest { _w = WorkspaceName, _f = metaCache.GetWorkspaceFingerprint(WorkspaceName), l = Array.Empty<Allors.Protocol.Json.Data.Pull>() });

            Assert.False(response.HasErrors);
            Assert.NotNull(response.p);
        }

        [Fact]
        public void ANamedPullCarriesTheEnvelopeToo()
        {
            var user = this.SetUser("jane@example.com");
            var api = new Api(this.Transaction, WorkspaceName, CancellationToken.None);

            var response = api.CreatePullResponseBuilder().Build();

            Assert.Equal(this.Transaction.Database.Id, response._db);
            Assert.Equal(user.Id, response._u);
            Assert.Equal(WorkspaceName, response._w);
            Assert.NotNull(response._f);
        }
    }
}
