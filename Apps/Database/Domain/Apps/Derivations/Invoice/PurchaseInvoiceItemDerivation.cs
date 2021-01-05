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

    public class PurchaseInvoiceItemDerivation : DomainDerivation
    {
        public PurchaseInvoiceItemDerivation(M m) : base(m, new Guid("54b6b925-f89f-4c77-9113-4b5eb72f9cfb")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PaymentApplication.AmountApplied) { Steps =  new IPropertyType[] {this.M.PaymentApplication.InvoiceItem}, OfType = m.PurchaseInvoiceItem.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<PurchaseInvoiceItem>())
            {
                var amountPaid = 0M;
                foreach (PaymentApplication paymentApplication in @this.PaymentApplicationsWhereInvoiceItem)
                {
                    amountPaid += paymentApplication.AmountApplied;
                }

                if (amountPaid != @this.AmountPaid)
                {
                    @this.AmountPaid = amountPaid;
                }
            }
        }
    }
}
