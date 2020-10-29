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

    public class PurchaseInvoiceDeniedPermissionDerivation : DomainDerivation
    {
        public PurchaseInvoiceDeniedPermissionDerivation(M m) : base(m, new Guid("f245b128-a597-4311-aad9-d68cb54bac7d")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.PurchaseInvoice.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }

                if (!@this.ExistSalesInvoiceWherePurchaseInvoice
                    && (@this.BilledFrom as Organisation)?.IsInternalOrganisation == true
                    && (@this.PurchaseInvoiceState.IsPaid || @this.PurchaseInvoiceState.IsPartiallyPaid || @this.PurchaseInvoiceState.IsNotPaid))
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.CreateSalesInvoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.CreateSalesInvoice));
                }
            }
        }
    }
}
