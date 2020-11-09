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
                new ChangedPattern(this.M.ProductQuoteApproval.ProductQuote)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuoteApproval>())
            {
                @this.Title = "Approval of " + @this.ProductQuote.WorkItemDescription;

                @this.WorkItem = @this.ProductQuote;

                // Lifecycle
                if (!@this.ExistDateClosed && !@this.ProductQuote.QuoteState.IsCreated)
                {
                    @this.DateClosed = @this.Session().Now();
                }

                if (@this.Participants.Count == 0)
                {
                    // Assignments
                    var participants = @this.ExistDateClosed
                                           ? (IEnumerable<Person>)Array.Empty<Person>()
                                           : new UserGroups(@this.Strategy.Session).Administrators.Members.Select(v => (Person)v).ToArray();
                    @this.AssignParticipants(participants);
                }
            }
        }
    }
}
