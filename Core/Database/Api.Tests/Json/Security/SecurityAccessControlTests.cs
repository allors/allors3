// <copyright file="SyncTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Linq;
    using Allors;
    using Allors.Api.Json;
    using Allors.Domain;
    using Allors.Protocol.Remote.Security;
    using Allors.Protocol.Remote.Sync;
    using Xunit;

    public class SecurityAccessControlTests : ApiTest, IClassFixture<Fixture>
    {
        public SecurityAccessControlTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var workspaceMeta = this.Session.Database.State().WorkspaceMetaCache;
            var workspaceX = workspaceMeta.Get("X");

            var accessControl = new AccessControls(this.Session).Administrator;

            this.SetUser("jane@example.com");

            var securityRequest = new SecurityRequest
            {
                AccessControls = new[] { $"{accessControl.Id}" },
            };

            var api = new Api(this.Session, "X");
            var securityResponse = api.Security(securityRequest);

            Assert.Single(securityResponse.AccessControls);

            var securityResponseAccessControl = securityResponse.AccessControls.First();

            Assert.Equal($"{accessControl.Id}", securityResponseAccessControl.I);
            Assert.Equal($"{accessControl.Strategy.ObjectVersion}", securityResponseAccessControl.V);

            var permissions = securityResponseAccessControl.P.Split(",")
                .Select(v => this.Session.Instantiate(v))
                .Cast<Permission>().ToArray();
            
            foreach (var permission in permissions)
            {
                Assert.Contains(permission, accessControl.EffectivePermissions);
                Assert.Contains(permission.ConcreteClass, workspaceX.Classes);
            }

            foreach (var effectivePermission in accessControl.EffectivePermissions.Where(v=>v.InWorkspace("X")))
            {
                Assert.Contains(effectivePermission, permissions);
            }
        }
    }
}
