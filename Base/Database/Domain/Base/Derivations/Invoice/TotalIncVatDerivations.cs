// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class InvoiceItemsTotalIncVatValidation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.InvoiceItem.TotalIncVat, out var changedInvoiceItem);
                var invoiceItems = changedInvoiceItem?.Select(session.Instantiate).OfType<InvoiceItem>();

                if(invoiceItems?.Any() == true)
                {
                    foreach (var invoiceItem in invoiceItems)
                    {
                        var totalInvoiceItemAmountPaid = invoiceItem?.PaymentApplicationsWhereInvoiceItem.Sum(v => v.AmountApplied);
                        if (totalInvoiceItemAmountPaid > invoiceItem.TotalIncVat)
                        {
                            validation.AddError($"{invoiceItem} {M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}");
                        }
                    }
                }
            }
        }

        public static void InvoiceItemsTotalIncVatRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("a9c35d0e-1366-4f69-b44e-6819d3c99b74")] = new InvoiceItemsTotalIncVatValidation();
        }
    }
}
