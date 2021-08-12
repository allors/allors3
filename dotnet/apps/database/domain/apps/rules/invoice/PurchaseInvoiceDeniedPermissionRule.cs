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

    public class PurchaseInvoiceDeniedPermissionRule : Rule
    {
        public PurchaseInvoiceDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("f245b128-a597-4311-aad9-d68cb54bac7d")) =>
            this.Patterns = new Pattern[]
        {
            m.PurchaseInvoice.RolePattern(v => v.TransitionalDeniedPermissions),
            m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
            m.PurchaseInvoice.AssociationPattern(v => v.SalesInvoiceWherePurchaseInvoice),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }

                var createSalesInvoicePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.CreateSalesInvoice);
                if (!@this.ExistSalesInvoiceWherePurchaseInvoice
                    && (@this.BilledFrom as Organisation)?.IsInternalOrganisation == true
                    && (@this.PurchaseInvoiceState.IsPaid || @this.PurchaseInvoiceState.IsPartiallyPaid || @this.PurchaseInvoiceState.IsNotPaid))
                {
                    @this.RemoveDeniedPermission(createSalesInvoicePermission);
                }
                else
                {
                    @this.AddDeniedPermission(createSalesInvoicePermission);
                }
            }
        }
    }
}
