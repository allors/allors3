// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class PurchaseInvoiceDeniedPermissionRule : Rule
    {
        public PurchaseInvoiceDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("f245b128-a597-4311-aad9-d68cb54bac7d")) =>
            this.Patterns = new Pattern[]
        {
            m.PurchaseInvoice.RolePattern(v => v.TransitionalRevocations),
            m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
            m.PurchaseInvoice.AssociationPattern(v => v.SalesInvoiceWherePurchaseInvoice),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.DerivePurchaseInvoiceDeniedPermissionRule(validation);
            }
        }
    }

    public static class PurchaseInvoiceDeniedPermissionRuleExtensions
    {
        public static void DerivePurchaseInvoiceDeniedPermissionRule(this PurchaseInvoice @this, IValidation validation)
        {
            @this.Revocations = @this.TransitionalRevocations;

            var createSalesInvoiceRevocation = new Revocations(@this.Strategy.Transaction).PurchaseInvoiceCreateSalesInvoiceRevocation;
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).PurchaseInvoiceDeleteRevocation;

            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }

            if (!@this.ExistSalesInvoiceWherePurchaseInvoice
                && (@this.BilledFrom as Organisation)?.IsInternalOrganisation == true
                && (@this.PurchaseInvoiceState.IsPaid || @this.PurchaseInvoiceState.IsPartiallyPaid || @this.PurchaseInvoiceState.IsNotPaid))
            {
                @this.RemoveRevocation(createSalesInvoiceRevocation);
            }
            else
            {
                @this.AddRevocation(createSalesInvoiceRevocation);
            }
        }
    }
}
