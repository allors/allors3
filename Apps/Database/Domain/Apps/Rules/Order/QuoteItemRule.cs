// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class QuoteItemRule : Rule
    {
        public QuoteItemRule(MetaPopulation m) : base(m, new Guid("17010D27-1BE9-4A8C-8AF5-8A9F9589AAF6")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.QuoteItem, m.QuoteItem.InvoiceItemType),
                new RolePattern(m.QuoteItem, m.QuoteItem.Product),
                new RolePattern(m.QuoteItem, m.QuoteItem.ProductFeature),
                new RolePattern(m.QuoteItem, m.QuoteItem.Deliverable),
                new RolePattern(m.QuoteItem, m.QuoteItem.WorkEffort),
                new RolePattern(m.QuoteItem, m.QuoteItem.SerialisedItem),
                new RolePattern(m.QuoteItem, m.QuoteItem.Quantity),
                new RolePattern(m.QuoteItem, m.QuoteItem.RequestItem),
                new RolePattern(m.QuoteItem, m.QuoteItem.UnitOfMeasure),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                var quote = @this.QuoteWhereQuoteItem;

                if (@this.ExistInvoiceItemType
                    && (@this.InvoiceItemType.IsPartItem
                        || @this.InvoiceItemType.IsProductFeatureItem
                        || @this.InvoiceItemType.IsProductItem))
                {
                    validation.AssertAtLeastOne(@this, this.M.QuoteItem.Product, this.M.QuoteItem.ProductFeature, this.M.QuoteItem.SerialisedItem, this.M.QuoteItem.Deliverable, this.M.QuoteItem.WorkEffort);
                    validation.AssertExistsAtMostOne(@this, this.M.QuoteItem.Product, this.M.QuoteItem.ProductFeature, this.M.QuoteItem.Deliverable, this.M.QuoteItem.WorkEffort);
                    validation.AssertExistsAtMostOne(@this, this.M.QuoteItem.SerialisedItem, this.M.QuoteItem.ProductFeature, this.M.QuoteItem.Deliverable, this.M.QuoteItem.WorkEffort);
                }
                else
                {
                    if (@this.Quantity != 1)
                    {
                        @this.Quantity = 1;
                    }
                }

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError($"{@this}, {@this.Meta.Quantity}, {ErrorMessages.SerializedItemQuantity}");
                }

                if (@this.ExistRequestItem)
                {
                    @this.RequiredByDate = @this.RequestItem.RequiredByDate;
                }

                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = new UnitsOfMeasure(@this.Strategy.Transaction).Piece;
                }

                if (@this.QuoteWhereQuoteItem is ProductQuote productQuote
                    && @this.ExistProduct
                    && !productQuote.ProductQuoteItemsByProduct.Any(v => v.Product.Equals(@this.Product)))
                {
                    productQuote.AddProductQuoteItemsByProduct(new ProductQuoteItemByProductBuilder(transaction).WithProduct(@this.Product).Build());
                }
            }
        }
    }
}
