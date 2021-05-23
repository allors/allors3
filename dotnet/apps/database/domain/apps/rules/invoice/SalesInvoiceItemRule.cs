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

    public class SalesInvoiceItemRule : Rule
    {
        public SalesInvoiceItemRule(MetaPopulation m) : base(m, new Guid("cf575774-c5f3-4368-bb92-07576f59f4b7")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoiceItem.RolePattern(v => v.Product),
                m.SalesInvoiceItem.RolePattern(v => v.ProductFeatures),
                m.SalesInvoiceItem.RolePattern(v => v.Part),
                m.SalesInvoiceItem.RolePattern(v => v.SerialisedItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var salesInvoice = @this.SalesInvoiceWhereSalesInvoiceItem;
                var salesInvoiceItemStates = new SalesInvoiceItemStates(transaction);

                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.Product, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);
                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.SerialisedItem, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);

                if (@this.ExistSerialisedItem
                    && salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceType
                    && !@this.ExistNextSerialisedItemAvailability
                    && salesInvoice.SalesInvoiceType.Equals(new SalesInvoiceTypes(@this.Transaction()).SalesInvoice))
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsSerialised && @this.Quantity != 1)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsNonSerialised && @this.Quantity == 0)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }
            }
        }
    }
}
