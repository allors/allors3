// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Data;
    using Allors.Database.Protocol.Json;
    using Xunit;
    using Extent = Allors.Database.Data.Extent;

    public class PullInstantiateTests : ApiTest, IClassFixture<Fixture>
    {
        public PullInstantiateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            var extent = new Extent(m.WorkspaceXObject1);
            var pullRequest = new PullRequest
            {
                List = new[]
                {
                    new Pull
                    {
                        Object = x1.Id,
                    },
                },
            };

            var api = new Api(this.Transaction, "X");
            var pullResponse = api.Pull(pullRequest);
            var wx1 = pullResponse.Objects["WorkspaceXObject1"];

            Assert.Equal(x1.Id, wx1);
        }

        [Fact]
        public void DifferentWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            var extent = new Extent(m.WorkspaceXObject1);
            var pullRequest = new PullRequest
            {
                List = new[]
                {
                    new Pull
                    {
                        Object = x1.Id,
                    },
                },
            };

            var api = new Api(this.Transaction, "Y");
            var pullResponse = api.Pull(pullRequest);
            Assert.Empty(pullResponse.Objects);
        }

        [Fact]
        public void NoneWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Transaction).Build();

            this.Transaction.Commit();

            var extent = new Extent(m.WorkspaceXObject1);
            var pullRequest = new PullRequest
            {
                List = new[]
                {
                    new Pull
                    {
                        Object = x1.Id,
                    },
                },
            };

            var api = new Api(this.Transaction, "None");
            var pullResponse = api.Pull(pullRequest);

            Assert.Empty(pullResponse.Objects);
        }
    }
}
