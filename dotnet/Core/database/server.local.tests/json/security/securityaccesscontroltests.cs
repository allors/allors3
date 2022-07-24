// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using System.Threading;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Services;
    using Allors.Protocol.Json.Api.Security;
    using Xunit;

    public class SecurityAccessControlTests : ApiTest, IClassFixture<Fixture>
    {
        public SecurityAccessControlTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var workspaceName = "X";
            var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
            var grant = new Grants(this.Transaction).Administrator;
            var securityToken = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();

            this.Transaction.Derive();

            this.SetUser("jane@example.com");

            var accessRequest = new AccessRequest
            {
                g = new[] { grant.Id },
            };

            var api = new Api(this.Transaction, workspaceName, CancellationToken.None);
            var securityResponse = api.Access(accessRequest);

            Assert.Single(securityResponse.g);

            var securityResponseAccessControl = securityResponse.g.First();

            Assert.Equal(grant.Id, securityResponseAccessControl.i);
            Assert.Equal(grant.Strategy.ObjectVersion, securityResponseAccessControl.v);

            var permissions = securityResponseAccessControl.p
                .Select(v => this.Transaction.Instantiate(v))
                .Cast<Permission>()
                .Where(v => v != null)
                .ToArray();

            foreach (var permission in permissions)
            {
                Assert.Contains(permission, grant.EffectivePermissions);
                Assert.Contains(permission.Class, metaCache.GetWorkspaceClasses(workspaceName));
            }

            foreach (var effectivePermission in grant.EffectivePermissions.Where(v => v.InWorkspace(workspaceName)))
            {
                Assert.Contains(effectivePermission, permissions);
            }
        }

        // TODO: non existing workspace should throw error
        //[Fact]
        //public void NoneWorkspace()
        //{
        //    var workspaceName = "Y";
        //    var metaCache = this.Transaction.Database.Services.Get<IMetaCache>();
        //    var grant = new Grants(this.Transaction).Administrator;

        //    this.SetUser("jane@example.com");

        //    var accessRequest = new AccessRequest
        //    {
        //        g = new[] { grant.Id },
        //    };

        //    var api = new Api(this.Transaction, workspaceName, CancellationToken.None);
        //    var accessResponse = api.Access(accessRequest);

        //    Assert.Single(accessResponse.g);

        //    var accessResponseGrant = accessResponse.g.First();

        //    Assert.Equal(grant.Id, accessResponseGrant.i);
        //    Assert.Equal(grant.Strategy.ObjectVersion, accessResponseGrant.v);
        //    Assert.Null(accessResponseGrant.p);
        //}
    }
}
