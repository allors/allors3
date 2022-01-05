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
    using Derivations.Rules;

    public class SerialisedItemPurchaseInvoiceNumberRule : Rule
    {
        public SerialisedItemPurchaseInvoiceNumberRule(MetaPopulation m) : base(m, new Guid("edd51dad-9c51-4856-b1fa-e3f1869d01fb")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.PurchaseInvoice),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemPurchaseInvoiceNumber(validation);
            }
        }
    }

    public static class SerialisedItemPurchaseInvoiceNumberRuleExtensions
    {
        public static void DeriveSerialisedItemPurchaseInvoiceNumber(this SerialisedItem @this, IValidation validation) => @this.PurchaseInvoiceNumber = @this.PurchaseInvoice?.InvoiceNumber;
    }
}
