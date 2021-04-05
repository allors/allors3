// <copyright file="Domain.cs" company="Allors bvba">
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

    public class QuoteRule : Rule
    {
        public QuoteRule(MetaPopulation m) : base(m, new Guid("B2464D89-5370-44D7-BB6B-7E6FA48EEF0B")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Quote, m.Quote.Issuer),
                new RolePattern(m.ProductQuote, m.ProductQuote.QuoteItems),
                new RolePattern(m.QuoteItem, m.QuoteItem.QuoteItemState) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Quote>())
            {
                if (!@this.ExistQuoteNumber && @this.ExistIssuer)
                {
                    var year = @this.IssueDate.Year;
                    @this.QuoteNumber = @this.Issuer.NextQuoteNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.Issuer.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.Issuer.QuoteSequence.IsEnforcedSequence ? @this.Issuer.QuoteNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.QuoteNumberPrefix;
                    @this.SortableQuoteNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.QuoteNumber, year.ToString());
                }

                @this.ValidQuoteItems = @this.QuoteItems.Where(v => v.IsValid).ToArray();

                foreach (QuoteItem quoteItem in @this.QuoteItems)
                {
                    quoteItem.Sync(@this);
                }
            }
        }
    }
}
