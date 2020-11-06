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

    public class PurchaseOrderItemCreationDerivation : DomainDerivation
    {
        public PurchaseOrderItemCreationDerivation(M m) : base(m, new Guid("6408b167-42c8-4468-81af-4218a7b1d3ae")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.PurchaseOrderItem.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                if (!@this.ExistPurchaseOrderItemState)
                {
                    @this.PurchaseOrderItemState = new PurchaseOrderItemStates(@this.Strategy.Session).Created;
                }

                if (!@this.ExistPurchaseOrderItemPaymentState)
                {
                    @this.PurchaseOrderItemPaymentState = new PurchaseOrderItemPaymentStates(@this.Strategy.Session).NotPaid;
                }

                if (@this.ExistPart && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Strategy.Session).PartItem;
                }
            }
        }
    }
}
