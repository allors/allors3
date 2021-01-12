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

    public class QuoteItemDerivation : DomainDerivation
    {
        public QuoteItemDerivation(M m) : base(m, new Guid("17010D27-1BE9-4A8C-8AF5-8A9F9589AAF6")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.QuoteItem.InvoiceItemType),
                new ChangedPattern(this.M.Quote.QuoteItems) { Steps = new IPropertyType[] {m.Quote.QuoteItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                var quote = @this.QuoteWhereQuoteItem;
                var productQuote = (ProductQuote)@this.QuoteWhereQuoteItem;

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
                    @this.Quantity = 1;
                }

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError($"{@this} {@this.Meta.Quantity} {ErrorMessages.SerializedItemQuantity}");
                }

                if (cycle.ChangeSet.IsCreated(@this) && !@this.ExistDetails)
                {
                    @this.DeriveDetails();
                }

                if (@this.ExistRequestItem)
                {
                    @this.RequiredByDate = @this.RequestItem.RequiredByDate;
                }

                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = new UnitsOfMeasure(@this.Strategy.Session).Piece;
                }

                if (productQuote != null
                    && @this.ExistProduct
                    && !productQuote.ProductQuoteItemsByProduct.Any(v => v.Product.Equals(@this.Product)))
                {
                    productQuote.AddProductQuoteItemsByProduct(new ProductQuoteItemByProductBuilder(session).WithProduct(@this.Product).Build());
                }
            }
        }
    }
}
