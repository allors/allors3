// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;

    public static partial class DabaseExtensions
    {
        public class ProposalCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdProposal = changeSet.Created.Select(v=>v.GetObject()).OfType<Proposal>();

                foreach (var proposal in createdProposal)
                {
                    foreach (QuoteItem quoteItem in proposal.QuoteItems)
                    {
                        quoteItem.Sync(proposal);
                    }

                    var deletePermission = new Permissions(proposal.Strategy.Session).Get(proposal.Meta.ObjectType, proposal.Meta.Delete, Operations.Execute);
                    if (proposal.IsDeletable)
                    {
                        proposal.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        proposal.AddDeniedPermission(deletePermission);
                    }
                }
            }
        }

        public static void PropasalRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("005c51a4-819f-4676-87fd-042f54cb7fc6")] = new ProposalCreationDerivation();
        }
    }
}
