// <copyright file="QuoteItemCreatedDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class QuoteItemCreatedIrpfRegimeRule : Rule
    {
        public QuoteItemCreatedIrpfRegimeRule(MetaPopulation m) : base(m, new Guid("b66c0721-4aa5-4ca7-91a0-534f6cfc6718")) =>
            this.Patterns = new Pattern[]
            {
                m.QuoteItem.RolePattern(v => v.AssignedIrpfRegime),
                m.QuoteItem.AssociationPattern(v => v.QuoteWhereQuoteItem),
                m.Quote.RolePattern(v => v.IssueDate, v => v.QuoteItems),
                m.Quote.RolePattern(v => v.DerivedIrpfRegime, v => v.QuoteItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<QuoteItem>())
            {
                var quote = @this.QuoteWhereQuoteItem;

                if (quote.QuoteState.IsCreated)
                {
                    @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? quote.DerivedIrpfRegime;
                    @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= quote.IssueDate && (!v.ExistThroughDate || v.ThroughDate >= quote.IssueDate));
                    @this.IrpfRatePercentage = @this.IrpfRate?.Rate ?? 0;
                }
            }
        }
    }
}
