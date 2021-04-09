// <copyright file="QuoteItemCreatedDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class QuoteItemCreatedVatRegimeRule : Rule
    {
        public QuoteItemCreatedVatRegimeRule(MetaPopulation m) : base(m, new Guid("0bd97653-57ad-468a-85df-e4abda86d4ae")) =>
            this.Patterns = new Pattern[]
            {
                m.QuoteItem.RolePattern(v => v.AssignedVatRegime),
                m.QuoteItem.AssociationPattern(v => v.QuoteWhereQuoteItem),
                m.Quote.RolePattern(v => v.IssueDate, v => v.QuoteItems),
                m.Quote.RolePattern(v => v.DerivedVatRegime, v => v.QuoteItems),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<QuoteItem>())
            {
                var quote = @this.QuoteWhereQuoteItem;

                if (quote.QuoteState.IsCreated)
                {
                    @this.DerivedVatRegime = @this.AssignedVatRegime ?? quote.DerivedVatRegime;
                    @this.VatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= quote.IssueDate && (!v.ExistThroughDate || v.ThroughDate >= quote.IssueDate));
                }
            }
        }
    }
}
