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
    using Xunit;

    public class SecurityAccessControlTests : ApiTest, IClassFixture<Fixture>
    {
        public SecurityAccessControlTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SameWorkspace()
        {
            var workspaceName = "X";

            var workspaceMeta = this.Session.Database.Context().WorkspaceMetaCache;
            var workspaceX = workspaceMeta.Get(workspaceName);

            var accessControl = new AccessControls(this.Session).Administrator;

            this.SetUser("jane@example.com");

            var securityRequest = new SecurityRequest
            {
                AccessControls = new[] { $"{accessControl.Id}" },
            };

            var api = new Api(this.Session, workspaceName);
            var securityResponse = api.Security(securityRequest);

            Assert.Single(securityResponse.AccessControls);

            var securityResponseAccessControl = securityResponse.AccessControls.First();

            Assert.Equal($"{accessControl.Id}", securityResponseAccessControl.Id);
            Assert.Equal($"{accessControl.Strategy.ObjectVersion}", securityResponseAccessControl.Version);

            var permissions = securityResponseAccessControl.PermissionIds.Split(",")
                .Select(v => this.Session.Instantiate(v))
                .Cast<Permission>()
                .Where(v => v != null)
                .ToArray();

            foreach (var permission in permissions)
            {
                Assert.Contains(permission, accessControl.EffectivePermissions);
                Assert.Contains(permission.Class, workspaceX.Classes);
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

            var workspaceMeta = this.Session.Database.Context().WorkspaceMetaCache;
            var workspace = workspaceMeta.Get(workspaceName);

            var accessControl = new AccessControls(this.Session).Administrator;

            this.SetUser("jane@example.com");

            var securityRequest = new SecurityRequest
            {
                AccessControls = new[] { $"{accessControl.Id}" },
            };

            var api = new Api(this.Session, workspaceName);
            var securityResponse = api.Security(securityRequest);

            Assert.Single(securityResponse.AccessControls);

            var securityResponseAccessControl = securityResponse.AccessControls.First();

            Assert.Equal($"{accessControl.Id}", securityResponseAccessControl.Id);
            Assert.Equal($"{accessControl.Strategy.ObjectVersion}", securityResponseAccessControl.Version);

            var permissions = securityResponseAccessControl.PermissionIds.Split(",")
                .Select(v => this.Session.Instantiate(v))
                .Cast<Permission>()
                .Where(v => v != null)
                .ToArray();

            foreach (var permission in permissions)
            {
                Assert.Contains(permission, accessControl.EffectivePermissions);
                Assert.Contains(permission.Class, workspace.Classes);
            }

            foreach (var effectivePermission in accessControl.EffectivePermissions.Where(v => v.InWorkspace(workspaceName)))
            {
                Assert.Contains(effectivePermission, permissions);
            }
        }

    }
}
