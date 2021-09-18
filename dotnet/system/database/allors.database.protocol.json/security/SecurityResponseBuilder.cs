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

    public class SecurityResponseBuilder
    {
        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly IRanges<long> ranges;

        public SecurityResponseBuilder(ITransaction transaction, IAccessControlLists accessControlLists, ISet<IClass> allowedClasses, IRanges<long> ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.ranges = ranges;
            this.AccessControlLists = accessControlLists;
        }

        public IAccessControlLists AccessControlLists { get; }

        public SecurityResponse Build(SecurityRequest securityRequest)
        {
            var securityResponse = new SecurityResponse();

            if (securityRequest.a?.Length > 0)
            {
                var accessControlIds = securityRequest.a;
                var accessControls = this.transaction.Instantiate(accessControlIds).Cast<IGrant>().ToArray();

                securityResponse.a = accessControls
                    .Select(v =>
                    {
                        var response = new SecurityResponseAccessControl
                        {
                            i = v.Strategy.ObjectId,
                            v = v.Strategy.ObjectVersion,
                        };

                        if (this.AccessControlLists.PermissionIdsByAccessControl.TryGetValue(v, out var x))
                        {
                            response.p = this.ranges.Import(x).Save();
                        }

                        return response;
                    }).ToArray();
            }

            if (securityRequest.r?.Length > 0)
            {
                var revocationIds = securityRequest.r;
                var revocations = this.transaction.Instantiate(revocationIds).Cast<IRevocation>().ToArray();

                securityResponse.r = revocations
                    .Select(v =>
                    {
                        var response = new SecurityResponseRevocation
                        {
                            i = v.Strategy.ObjectId,
                            v = v.Strategy.ObjectVersion,
                        };

                        if (this.AccessControlLists.DeniedPermissionIdsByRevocation.TryGetValue(v, out var x))
                        {
                            response.p = this.ranges.Import(x).Save();
                        }

                        return response;
                    }).ToArray();
            }


            if (securityRequest.p?.Length > 0)
            {
                var permissionIds = securityRequest.p;
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
