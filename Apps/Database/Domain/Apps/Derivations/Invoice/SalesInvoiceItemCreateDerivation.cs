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

    public class SalesInvoiceItemCreateDerivation : DomainDerivation
    {
        public SalesInvoiceItemCreateDerivation(M m) : base(m, new Guid("2e13bf13-40b5-4040-ad38-9e444bb6ffce")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.SalesInvoiceItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                if (!@this.ExistSalesInvoiceItemState)
                {
                    @this.SalesInvoiceItemState = new SalesInvoiceItemStates(@this.Strategy.Session).ReadyForPosting;
                }

                if (@this.ExistProduct && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Strategy.Session).ProductItem;
                }

                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}
