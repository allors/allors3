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
        private readonly ISecurity security;
        private readonly IUser user;
        private readonly string workspaceName;

        public AccessResponseBuilder(ITransaction transaction, ISecurity security, IUser user, string workspaceName)
        {
            this.transaction = transaction;
            this.security = security;
            this.user = user;
            this.workspaceName = workspaceName;
        }

        public AccessResponse Build(AccessRequest accessRequest)
        {
            var accessResponse = new AccessResponse();

            if (accessRequest.g?.Length > 0)
            {
                var ids = accessRequest.g;
                var grants = this.transaction.Instantiate(ids).Cast<IGrant>().ToArray();
                var versionedGrants = this.security.GetVersionedGrants(this.transaction, this.user, grants, this.workspaceName);

                accessResponse.g = versionedGrants
                    .Select(v =>
                    {
                        var response = new AccessResponseGrant
                        {
                            i = v.Id,
                            v = v.Version,
                            p = v.PermissionRange.Save()
                        };

                        return response;
                    }).ToArray();
            }

            if (accessRequest.r?.Length > 0)
            {
                var revocationIds = accessRequest.r;
                var revocations = this.transaction.Instantiate(revocationIds).Cast<IRevocation>().ToArray();
                var versionedRevocations = this.security.GetVersionedRevocations(this.transaction, this.user, revocations, this.workspaceName);

                accessResponse.r = versionedRevocations
                    .Select(v =>
                    {
                        var response = new AccessResponseRevocation
                        {
                            i = v.Id,
                            v = v.Version,
                            p = v.PermissionRange.Save()
                        };

                        return response;
                    }).ToArray();
            }

            return accessResponse;
        }
    }
}
