// <copyright file="SecurityResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Api.Json.Security
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using Allors.Domain;
    using Allors.Protocol.Remote.Security;

    public class SecurityResponseBuilder
    {
        private readonly ISession session;
        private readonly SecurityRequest securityRequest;
        private readonly IAccessControlLists acls;

        public SecurityResponseBuilder(IAccessControlLists acls, ISession session, SecurityRequest securityRequest)
        {
            this.session = session;
            this.securityRequest = securityRequest;
            this.acls = acls;
        }

        public SecurityResponse Build()
        {
            var securityResponse = new SecurityResponse();

            if (this.securityRequest.AccessControls != null)
            {
                var accessControlIds = this.securityRequest.AccessControls;
                var accessControls = this.session.Instantiate(accessControlIds).Cast<AccessControl>().ToArray();

                securityResponse.AccessControls = accessControls
                    .Select(v => new SecurityResponseAccessControl
                    {
                        I = v.Strategy.ObjectId.ToString(),
                        V = v.Strategy.ObjectVersion.ToString(),
                        P = string.Join(",", this.acls.EffectivePermissionIdsByAccessControl[v]),
                    }).ToArray();
            }

            if (this.securityRequest.Permissions.Length > 0)
            {
                var permissionIds = this.securityRequest.Permissions;
                var permissions = this.session.Instantiate(permissionIds)
                    .Cast<Permission>()
                    .Where(v => v switch
                    {
                        ReadPermission permission => permission.RelationType.WorkspaceNames.Length > 0,
                        WritePermission permission => permission.RelationType.WorkspaceNames.Length > 0,
                        ExecutePermission permission => permission.MethodType.WorkspaceNames.Length > 0,
                        _ => throw new Exception(),
                    });

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
