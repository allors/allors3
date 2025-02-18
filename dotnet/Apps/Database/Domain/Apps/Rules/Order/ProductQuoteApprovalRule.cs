// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class ProductQuoteApprovalRule : Rule
    {
        public ProductQuoteApprovalRule(MetaPopulation m) : base(m, new Guid("102A7185-6BF4-4804-B978-A2D6A782461A")) =>
            this.Patterns = new[]
            {
                m.ProductQuoteApproval.RolePattern(v => v.ProductQuote)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuoteApproval>())
            {
                @this.Title = "Approval of " + @this.ProductQuote.WorkItemDescription;

                @this.WorkItem = @this.ProductQuote;

                // Services
                if (!@this.ExistDateClosed && !@this.ProductQuote.QuoteState.IsAwaitingApproval)
                {
                    @this.DateClosed = @this.Transaction().Now();
                }
            }
        }
    }
}
