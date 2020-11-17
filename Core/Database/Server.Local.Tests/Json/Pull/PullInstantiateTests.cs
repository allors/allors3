// <copyright file="ContentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    
    using Allors.Domain;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Data;
    using Allors.Database.Protocol.Json;
    using Xunit;

    public class PullInstantiateTests : ApiTest, IClassFixture<Fixture>
    {
        public PullInstantiateTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Session).Build();

            this.Session.Commit();

            var extent = new Allors.Data.Extent(m.WorkspaceXObject1.ObjectType);
            var pullRequest = new PullRequest
            {
                Pulls = new[]
                {
                    new Pull
                    {
                        Object = x1.Id.ToString(),
                    },
                },
            };

            var api = new Api(this.Session, "X");
            var pullResponse = api.Pull(pullRequest);
            var wx1 = pullResponse.NamedObjects["WorkspaceXObject1"];

            Assert.Equal(x1.Id.ToString(), wx1);
        }

        [Fact]
        public void DifferentWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Session).Build();

            this.Session.Commit();

            var extent = new Allors.Data.Extent(m.WorkspaceXObject1.ObjectType);
            var pullRequest = new PullRequest
            {
                Pulls = new[]
                {
                    new Pull
                    {
                        Object = x1.Id.ToString(),
                    },
                },
            };

            var api = new Api(this.Session, "Y");
            var pullResponse = api.Pull(pullRequest);
            Assert.Empty(pullResponse.NamedObjects);
        }

        [Fact]
        public void NoneWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = new WorkspaceXObject1Builder(this.Session).Build();

            this.Session.Commit();

            var extent = new Allors.Data.Extent(m.WorkspaceXObject1.ObjectType);
            var pullRequest = new PullRequest
            {
                Pulls = new[]
                {
                    new Pull
                    {
                        Object = x1.Id.ToString(),
                    },
                },
            };

            var api = new Api(this.Session, "None");
            var pullResponse = api.Pull(pullRequest);

            Assert.Empty(pullResponse.NamedObjects);
        }
    }
}
