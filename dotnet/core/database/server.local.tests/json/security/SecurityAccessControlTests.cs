// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System.Linq;
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Database.Protocol.Json;
    using Allors.Database.Services;
    using Xunit;

    public class SecurityAccessControlTests : ApiTest, IClassFixture<Fixture>
    {
        public SecurityAccessControlTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var workspaceName = "X";
            var metaCache = this.Transaction.Database.Services().Get<IMetaCache>();
            var accessControl = new AccessControls(this.Transaction).Administrator;

            this.SetUser("jane@example.com");

            var securityRequest = new SecurityRequest
            {
                a = new[] { accessControl.Id },
            };

            var api = new Api(this.Transaction, workspaceName);
            var securityResponse = api.Security(securityRequest);

            Assert.Single(securityResponse.a);

            var securityResponseAccessControl = securityResponse.a.First();

            Assert.Equal(accessControl.Id, securityResponseAccessControl.i);
            Assert.Equal(accessControl.Strategy.ObjectVersion, securityResponseAccessControl.v);

            var permissions = securityResponseAccessControl.p
                .Select(v => this.Transaction.Instantiate(v))
                .Cast<Permission>()
                .Where(v => v != null)
                .ToArray();

            foreach (var permission in permissions)
            {
                Assert.Contains(permission, accessControl.EffectivePermissions);
                Assert.Contains(permission.Class, metaCache.GetWorkspaceClasses(workspaceName));
            }

            foreach (var effectivePermission in accessControl.EffectivePermissions.Where(v => v.InWorkspace(workspaceName)))
            {
                Assert.Contains(effectivePermission, permissions);
            }
        }

        [Fact]
        public void NoneWorkspace()
        {
            var workspaceName = "None";
            var metaCache = this.Transaction.Database.Services().Get<IMetaCache>();
            var accessControl = new AccessControls(this.Transaction).Administrator;

            this.SetUser("jane@example.com");

            var securityRequest = new SecurityRequest
            {
                a = new[] { accessControl.Id },
            };

            var api = new Api(this.Transaction, workspaceName);
            var securityResponse = api.Security(securityRequest);

            Assert.Single(securityResponse.a);

            var securityResponseAccessControl = securityResponse.a.First();

            Assert.Equal(accessControl.Id, securityResponseAccessControl.i);
            Assert.Equal(accessControl.Strategy.ObjectVersion, securityResponseAccessControl.v);

            var permissions = securityResponseAccessControl.p
                .Select(v => this.Transaction.Instantiate(v))
                .Cast<Permission>()
                .Where(v => v != null)
                .ToArray();

            foreach (var permission in permissions)
            {
                Assert.Contains(permission, accessControl.EffectivePermissions);
                Assert.Contains(permission.Class, metaCache.GetWorkspaceClasses(workspaceName));
            }

            foreach (var effectivePermission in accessControl.EffectivePermissions.Where(v => v.InWorkspace(workspaceName)))
            {
                Assert.Contains(effectivePermission, permissions);
            }
        }
    }
}
