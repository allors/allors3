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
    using Ranges;
    using Security;

    public class PermissionResponseBuilder
    {
        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly IRanges<long> ranges;

        public PermissionResponseBuilder(ITransaction transaction, IAccessControl accessControl, ISet<IClass> allowedClasses, IRanges<long> ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.ranges = ranges;
            this.AccessControl = accessControl;
        }

        public IAccessControl AccessControl { get; }

        public AccessResponse Build(AccessRequest accessRequest)
        {
            var securityResponse = new AccessResponse();

            if (accessRequest.g?.Length > 0)
            {
                var accessControlIds = accessRequest.g;
                var accessControls = this.transaction.Instantiate(accessControlIds).Cast<IGrant>().ToArray();

                securityResponse.g = accessControls
                    .Select(v =>
                    {
                        var response = new AccessResponseGrant
                        {
                            i = v.Strategy.ObjectId,
                            v = v.Strategy.ObjectVersion,
                        };

                        if (this.AccessControl.PermissionIdsByAccessControl.TryGetValue(v, out var x))
                        {
                            response.p = this.ranges.Import(x).Save();
                        }

                        return response;
                    }).ToArray();
            }

            if (accessRequest.r?.Length > 0)
            {
                var revocationIds = accessRequest.r;
                var revocations = this.transaction.Instantiate(revocationIds).Cast<IRevocation>().ToArray();

                securityResponse.r = revocations
                    .Select(v =>
                    {
                        var response = new AccessResponseRevocation
                        {
                            i = v.Strategy.ObjectId,
                            v = v.Strategy.ObjectVersion,
                        };

                        if (this.AccessControl.DeniedPermissionIdsByRevocation.TryGetValue(v, out var x))
                        {
                            response.p = this.ranges.Import(x).Save();
                        }

                        return response;
                    }).ToArray();
            }


            if (accessRequest.p?.Length > 0)
            {
                var permissionIds = accessRequest.p;
                var permissions = this.transaction.Instantiate(permissionIds)
                    .Cast<IPermission>()
                    .Where(v => this.allowedClasses?.Contains(v.Class) == true);

                securityResponse.p = permissions.Select(v => v switch
                {
                    IReadPermission permission => new SecurityResponsePermission
                    {
                        i = permission.Strategy.ObjectId,
                        c = permission.Class.Tag,
                        t = permission.RelationType.Tag,
                        o = (long)Operations.Read
                    },
                    IWritePermission permission => new SecurityResponsePermission
                    {
                        i = permission.Strategy.ObjectId,
                        c = permission.Class.Tag,
                        t = permission.RelationType.Tag,
                        o = (long)Operations.Write
                    },
                    IExecutePermission permission => new SecurityResponsePermission
                    {
                        i = permission.Strategy.ObjectId,
                        c = permission.Class.Tag,
                        t = permission.MethodType.Tag,
                        o = (long)Operations.Execute
                    },
                    _ => throw new Exception(),
                }).ToArray();
            }

            return securityResponse;
        }
    }
}
