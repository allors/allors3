// <copyright file="SecurityResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Security;
    using Meta;
    using Security;

    public class SecurityResponseBuilder
    {
        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;

        public SecurityResponseBuilder(ITransaction transaction, IAccessControlLists accessControlLists, ISet<IClass> allowedClasses)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.AccessControlLists = accessControlLists;
        }

        public IAccessControlLists AccessControlLists { get; }

        public SecurityResponse Build(SecurityRequest securityRequest)
        {
            var securityResponse = new SecurityResponse();

            if (securityRequest.AccessControls?.Length > 0)
            {
                var accessControlIds = securityRequest.AccessControls;
                var accessControls = this.transaction.Instantiate(accessControlIds).Cast<IAccessControl>().ToArray();

                securityResponse.AccessControls = accessControls
                    .Select(v =>
                    {
                        var response = new SecurityResponseAccessControl
                        {
                            Id = v.Strategy.ObjectId,
                            Version = v.Strategy.ObjectVersion,
                        };

                        if (this.AccessControlLists.EffectivePermissionIdsByAccessControl.TryGetValue(v, out var x))
                        {
                            response.PermissionIds = x.ToArray();
                        }

                        return response;
                    }).ToArray();
            }

            if (securityRequest.Permissions?.Length > 0)
            {
                var permissionIds = securityRequest.Permissions;
                var permissions = this.transaction.Instantiate(permissionIds)
                    .Cast<IPermission>()
                    .Where(v => this.allowedClasses?.Contains(v.Class) == true);

                securityResponse.Permissions = permissions.Select(v =>
                    v switch
                    {
                        IReadPermission permission => new long[]
                        {
                            permission.Strategy.ObjectId,
                            permission.Class.Tag,
                            permission.RelationType.Tag,
                            (long)Operations.Read,
                        },
                        IWritePermission permission => new long[]
                        {
                            permission.Strategy.ObjectId,
                            permission.Class.Tag,
                            permission.RelationType.Tag,
                            (long)Operations.Write,
                        },
                        IExecutePermission permission => new long[]
                        {
                            permission.Strategy.ObjectId,
                            permission.Class.Tag,
                            permission.MethodType.Tag,
                            (long)Operations.Execute,
                        },
                        _ => throw new Exception(),
                    }).ToArray();
            }

            return securityResponse;
        }
    }
}
