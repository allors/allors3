// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Threading;
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
