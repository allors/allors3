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
        public class ProductQuoteApprovalCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdProductQuoteApproval = changeSet.Created.Select(v=>v.GetObject()).OfType<ProductQuoteApproval>();

                foreach (var productQuoteApproval in createdProductQuoteApproval)
                {
                    productQuoteApproval.Title = "Approval of " + productQuoteApproval.ProductQuote.WorkItemDescription;

                    productQuoteApproval.WorkItem = productQuoteApproval.ProductQuote;

                    // Lifecycle
                    if (!productQuoteApproval.ExistDateClosed && !productQuoteApproval.ProductQuote.QuoteState.IsCreated)
                    {
                        productQuoteApproval.DateClosed = productQuoteApproval.Session().Now();
                    }

                    if (productQuoteApproval.Participants.Count == 0)
                    {
                        // Assignments
                        var participants = productQuoteApproval.ExistDateClosed
                                               ? (IEnumerable<Person>)Array.Empty<Person>()
                                               : new UserGroups(productQuoteApproval.Strategy.Session).Administrators.Members.Select(v => (Person)v).ToArray();
                        productQuoteApproval.AssignParticipants(participants);
                    }
                }
            }
        }

        public static void ProductQuoteApprovalRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("f2ee1ab7-07e2-4244-a840-1affa01abef2")] = new ProductQuoteApprovalCreationDerivation();
        }
    }
}
