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
        public class RequestForProposalCreationDerivations : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdRequestForProposal = changeSet.Created.Select(session.Instantiate).OfType<RequestForProposal>();

                foreach (var requestForProposal in createdRequestForProposal)
                { 
                    foreach (RequestItem requestItem in requestForProposal.RequestItems)
                    {
                        requestItem.Sync(requestForProposal);
                    }
                
                    var deletePermission = new Permissions(requestForProposal.Strategy.Session).Get(requestForProposal.Meta.ObjectType, requestForProposal.Meta.Delete, Operations.Execute);
                    if (requestForProposal.IsDeletable())
                    {
                        requestForProposal.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        requestForProposal.AddDeniedPermission(deletePermission);
                    }
                }


            }
        }
        public static void RequestForProposalRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("36c2211a-3a76-4037-a2cb-7da16761561e")] = new RequestForProposalCreationDerivations();
        }
    }
}
