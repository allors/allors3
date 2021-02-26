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

    public class ProductQuoteItemByProductDerivation : DomainDerivation
    {
        public ProductQuoteItemByProductDerivation(M m) : base(m, new Guid("fb8b7202-76c5-48f0-972f-e77a56b9a0ab")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(this.M.QuoteItemVersion.Product) { Steps = new IPropertyType[] {m.QuoteItemVersion.QuoteItemWhereCurrentVersion, m.QuoteItem.QuoteWhereQuoteItem, m.ProductQuote.ProductQuoteItemsByProduct } },
                new AssociationPattern(this.M.QuoteItem.Product) { Steps = new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem, m.ProductQuote.ProductQuoteItemsByProduct} },
                new AssociationPattern(this.M.QuoteItem.Quantity) { Steps = new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem, m.ProductQuote.ProductQuoteItemsByProduct } },
                new AssociationPattern(this.M.QuoteItem.TotalBasePrice) { Steps = new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem, m.ProductQuote.ProductQuoteItemsByProduct } },
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
