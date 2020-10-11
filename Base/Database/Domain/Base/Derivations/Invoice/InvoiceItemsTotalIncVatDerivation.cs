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
    using Resources;

    public class InvoiceItemsTotalIncVatDerivation : DomainDerivation
    {
        public InvoiceItemsTotalIncVatDerivation(M m) : base(m, new Guid("DB8D8C77-4E1A-4775-A243-79C7A558CFE4")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedConcreteRolePattern(this.M.SalesInvoiceItem.TotalIncVat),
                new ChangedConcreteRolePattern(this.M.PurchaseInvoiceItem.TotalIncVat),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var invoiceItem in matches.Cast<InvoiceItem>())
            {
                var totalInvoiceItemAmountPaid =
                    invoiceItem?.PaymentApplicationsWhereInvoiceItem.Sum(v => v.AmountApplied);
                if (totalInvoiceItemAmountPaid > invoiceItem.TotalIncVat)
                {
                    cycle.Validation.AddError($"{invoiceItem} {this.M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}");
                }
            }
        }
    }
}
