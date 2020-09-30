// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class ProductQuoteApprovalDerivation : DomainDerivation
    {
        public ProductQuoteApprovalDerivation(M m) : base(m, new Guid("102A7185-6BF4-4804-B978-A2D6A782461A")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(M.ProductQuoteApproval.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var productQuoteApproval in matches.Cast<ProductQuoteApproval>())
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
}
