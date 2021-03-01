// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SalesInvoiceDeniedPermissionDerivation : DomainDerivation
    {
        public SalesInvoiceDeniedPermissionDerivation(M m) : base(m, new Guid("8726348f-85af-429b-a514-55d00dbb14d9")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(this.M.SalesInvoice.TransitionalDeniedPermissions),
            new AssociationPattern(this.M.SalesInvoice.IsRepeatingInvoice),
            new AssociationPattern(this.M.SalesInvoice.SalesOrders),
            new AssociationPattern(this.M.SalesInvoice.PurchaseInvoice),
            new AssociationPattern(this.M.SalesInvoiceItem.SalesInvoiceItemState) { Steps = new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem } },
            new RolePattern(this.M.RepeatingSalesInvoice.Source),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);

                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
