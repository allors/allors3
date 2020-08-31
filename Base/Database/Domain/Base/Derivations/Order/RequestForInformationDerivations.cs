// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class RequestForInformationCreationDerivations : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRequestForInformation = changeSet.Created.Select(v=>v.GetObject()).OfType<RequestForInformation>();

                foreach (var requestForInformation in createdRequestForInformation)
                {
                    foreach (RequestItem requestItem in requestForInformation.RequestItems)
                    {
                        requestItem.Sync(requestForInformation);
                    }
                    var deletePermission = new Permissions(requestForInformation.Strategy.Session).Get(requestForInformation.Meta.ObjectType, requestForInformation.Meta.Delete, Operations.Execute);
                    if (requestForInformation.IsDeletable())
                    {
                        requestForInformation.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        requestForInformation.AddDeniedPermission(deletePermission);
                    }
                }


            }
        }
        public static void RequestForInformationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("09b50092-a867-47d8-a9a6-b6e81e5a4fea")] = new RequestForInformationCreationDerivations();
        }
    }
}
