// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PurchaseInvoiceItemDerivation : DomainDerivation
    {
        public PurchaseInvoiceItemDerivation(M m) : base(m, new Guid("55013d40-956f-4d36-8d50-0375c0d97af9")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PurchaseInvoiceItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoiceItem>())
            {
                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}
