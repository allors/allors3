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

    public class PurchaseInvoiceItemCreationDerivation : DomainDerivation
    {
        public PurchaseInvoiceItemCreationDerivation(M m) : base(m, new Guid("25ec02e8-82df-44a9-8c55-4ffa8853054e")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.PurchaseInvoiceItem.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoiceItem>())
            {
                if (!@this.ExistPurchaseInvoiceItemState)
                {
                    @this.PurchaseInvoiceItemState = new PurchaseInvoiceItemStates(@this.Strategy.Session).Created;
                }

                if (@this.ExistPart && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Strategy.Session).PartItem;
                }
            }
        }
    }
}
