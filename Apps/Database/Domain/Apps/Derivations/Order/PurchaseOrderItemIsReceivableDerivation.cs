// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class PurchaseOrderItemIsReceivableDerivation : DomainDerivation
    {
        public PurchaseOrderItemIsReceivableDerivation(M m) : base(m, new Guid("525c9efa-5ba0-41bf-9118-a7b79441a9d9")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.PurchaseOrderItem.InvoiceItemType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.IsReceivable = @this.ExistPart
                    && (@this.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Session()).PartItem)
                        || @this.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Session()).ProductItem));
            }
        }
    }
}
