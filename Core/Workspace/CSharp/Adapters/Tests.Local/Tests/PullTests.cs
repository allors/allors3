// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using Allors.Protocol.Direct.Api;
    using Allors.Protocol.Direct.Api.Pull;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Xunit;

    public class PullTests : Test, IClassFixture<Fixture>
    {
        public PullTests(Fixture fixture) : base(fixture, true) { }

        [Fact]
        public void Default()
        {
            var m = this.Workspace.Context().M;

            var request = new PullRequest
            {
                Pulls = new[]
                {
                    new Pull
                    {
                        Extent = new Extent(m.C1.ObjectType)
                    },
                }
            };

            var api = new Api(this.Session, "Default");

            var response = api.Pull(request);
        }
    }
}
