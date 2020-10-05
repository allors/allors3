// <copyright file="SecurityResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Api.Json.Security
{
    using System;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using Allors.Domain;
    using Allors.Protocol.Remote.Security;
    using Services;

    public class SecurityResponseBuilder
    {
        private readonly ISession session;
        private readonly SecurityRequest securityRequest;

        public SecurityResponseBuilder(ISession session, string workspaceName, SecurityRequest securityRequest)
        {
            this.session = session;
            this.securityRequest = securityRequest;

            var sessionState = session.State();
            var databaseState = session.Database.State();

            this.WorkspaceMeta = databaseState.WorkspaceMetaCache.Get(workspaceName);
            this.AccessControlLists = new WorkspaceAccessControlLists(workspaceName, sessionState.User);
        }

        public IAccessControlLists AccessControlLists { get; }

        public IWorkspaceMetaCacheEntry WorkspaceMeta { get; }

        public SecurityResponse Build()
        {
            var classes = this.WorkspaceMeta?.Classes;

            var securityResponse = new SecurityResponse();

            if (this.securityRequest.AccessControls?.Length > 0)
            {
                var accessControlIds = this.securityRequest.AccessControls;
                var accessControls = this.session.Instantiate(accessControlIds).Cast<AccessControl>().ToArray();

                securityResponse.AccessControls = accessControls
                    .Select(v =>
                    {
                        var response = new SecurityResponseAccessControl
                        {
                            I = v.Strategy.ObjectId.ToString(),
                            V = v.Strategy.ObjectVersion.ToString(),
                        };

                        if (this.AccessControlLists.EffectivePermissionIdsByAccessControl.TryGetValue(v, out var x))
                        {
                            response.P = string.Join(",", x);
                        }

                        return response;
                    }).ToArray();
            }

            if (this.securityRequest.Permissions?.Length > 0)
            {
                var permissionIds = this.securityRequest.Permissions;
                var permissions = this.session.Instantiate(permissionIds)
                    .Cast<Permission>()
                    .Where(v => classes?.Contains(v.ConcreteClass) == true);

                securityResponse.Permissions = permissions.Select(v =>
                    v switch
                    {
                        ReadPermission permission => new[]
                        {
                            permission.Strategy.ObjectId.ToString(),
                            permission.ClassPointer.ToString("D"),
                            permission.RelationTypePointer.ToString("D"),
                            Operations.Read.ToString(),
                        },
                        WritePermission permission => new[]
                        {
                            permission.Strategy.ObjectId.ToString(),
                            permission.ClassPointer.ToString("D"),
                            permission.RelationTypePointer.ToString("D"),
                            Operations.Write.ToString(),
                        },
                        ExecutePermission permission => new[]
                        {
                            permission.Strategy.ObjectId.ToString(),
                            permission.ClassPointer.ToString("D"),
                            permission.MethodTypePointer.ToString("D"),
                            Operations.Execute.ToString(),
                        },
                        _ => throw new Exception(),
                    }).ToArray();
            }

            return securityResponse;
        }
    }
}
