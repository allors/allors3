// <copyright file="ProductQuoteItemByProductDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class ProductQuoteItemByProductRule : Rule
    {
        public ProductQuoteItemByProductRule(MetaPopulation m) : base(m, new Guid("fb8b7202-76c5-48f0-972f-e77a56b9a0ab")) =>
            this.Patterns = new Pattern[]
            {
                m.QuoteItemVersion.RolePattern(v => v.Product, v => v.QuoteItemWhereCurrentVersion.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote.ProductQuoteItemsByProduct),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote.ProductQuoteItemsByProduct),
                m.QuoteItem.RolePattern(v => v.Quantity, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote.ProductQuoteItemsByProduct),
                m.QuoteItem.RolePattern(v => v.TotalBasePrice, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote.ProductQuoteItemsByProduct),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<ProductQuoteItemByProduct>())
            {
                var sameProductItems = @this.ProductQuoteWhereProductQuoteItemsByProduct?.QuoteItems
                    .Where(v => v.IsValid && v.ExistProduct && v.Product.Equals(@this.Product))
                    .ToArray();

                @this.QuantityOrdered = sameProductItems.Sum(w => w.Quantity);
                @this.ValueOrdered = sameProductItems.Sum(w => w.TotalBasePrice);
            }
        }
    }
}
