// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SerialisedItemQuoteItemWhereSerialisedItemRule : Rule
    {
        public SerialisedItemQuoteItemWhereSerialisedItemRule(MetaPopulation m) : base(m, new Guid("7b19faf5-ee38-4571-8e1a-e188b5d41fa0")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.QuoteItem.SerialisedItem),
                new RolePattern(m.QuoteItem, m.QuoteItem.QuoteItemState) { Steps = new IPropertyType[] { m.QuoteItem.SerialisedItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.OnQuote = @this.QuoteItemsWhereSerialisedItem.Any(v => v.QuoteItemState.IsDraft
                            || v.QuoteItemState.IsSubmitted || v.QuoteItemState.IsApproved
                            || v.QuoteItemState.IsAwaitingAcceptance || v.QuoteItemState.IsAccepted);
            }
        }
    }
}
