// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class QuoteItemCreationDerivation : DomainDerivation
    {
        public QuoteItemCreationDerivation(M m) : base(m, new Guid("cf1ff6ac-a0d7-41e3-9306-2951fd22c005")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.QuoteItem.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<QuoteItem>())
            {
                if (!@this.ExistQuoteItemState)
                {
                    @this.QuoteItemState = new QuoteItemStates(@this.Strategy.Session).Draft;
                }

                if (@this.ExistProduct && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Strategy.Session).ProductItem;
                }
            }
        }
    }
}
