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

    public class AccessResponseBuilder
    {
        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly IRanges<long> ranges;

        public AccessResponseBuilder(ITransaction transaction, IAccessControl accessControl, ISet<IClass> allowedClasses, IRanges<long> ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.ranges = ranges;
            this.AccessControl = accessControl;
        }

        public IAccessControl AccessControl { get; }

        public AccessResponse Build(AccessRequest accessRequest)
        {
            var accessResponse = new AccessResponse();

            if (accessRequest.g?.Length > 0)
            {
                var ids = accessRequest.g;
                var grants = this.transaction.Instantiate(ids).Cast<IGrant>().ToArray();

                accessResponse.g = grants
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

                accessResponse.r = revocations
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

            return accessResponse;
        }
    }
}
