// <copyright file="SecurityResponseBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Linq;
    using Allors.Protocol.Json.Api.Security;
    using Domain;
    using Security;

    public class AccessResponseBuilder
    {
        private readonly ITransaction transaction;
        private readonly string workspaceName;
        private readonly ISecurity security;

        public AccessResponseBuilder(ITransaction transaction, ISecurity security, string workspaceName)
        {
            this.transaction = transaction;
            this.security = security;
            this.workspaceName = workspaceName;
        }

        public AccessResponse Build(AccessRequest accessRequest)
        {
            var accessResponse = new AccessResponse();

            if (accessRequest.g?.Length > 0)
            {
                var ids = accessRequest.g;
                var grants = this.transaction.Instantiate(ids).Cast<IGrant>().ToArray();

                var grantPermissions = this.security.GetGrantPermissions(this.transaction, grants, this.workspaceName);

                accessResponse.g = grants
                    .Select(v =>
                    {
                        var response = new AccessResponseGrant
                        {
                            i = v.Strategy.ObjectId,
                            v = v.Strategy.ObjectVersion,
                            p = grantPermissions[v].Range.Save()
                        };

                        return response;
                    }).ToArray();
            }

            if (accessRequest.r?.Length > 0)
            {
                var revocationIds = accessRequest.r;
                var revocations = this.transaction.Instantiate(revocationIds).Cast<IRevocation>().ToArray();

                var revocationPermissions = this.security.GetRevocationPermissions(this.transaction, revocations, this.workspaceName);

                accessResponse.r = revocations
                    .Select(v =>
                    {
                        var response = new AccessResponseRevocation
                        {
                            i = v.Strategy.ObjectId,
                            v = v.Strategy.ObjectVersion,
                            p = revocationPermissions[v].Range.Save()
                        };

                        return response;
                    }).ToArray();
            }

            return accessResponse;
        }
    }
}
