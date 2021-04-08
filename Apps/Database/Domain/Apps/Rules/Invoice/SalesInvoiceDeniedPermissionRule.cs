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

    public class SalesInvoiceDeniedPermissionRule : Rule
    {
        public SalesInvoiceDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("8726348f-85af-429b-a514-55d00dbb14d9")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.TransitionalDeniedPermissions),
            m.SalesInvoice.RolePattern(v => v.IsRepeatingInvoice),
            m.SalesInvoice.RolePattern(v => v.SalesOrders),
            m.SalesInvoice.RolePattern(v => v.PurchaseInvoice),
            m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoice.AssociationPattern(v => v.RepeatingSalesInvoiceWhereSource),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
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
            }
        }
    }
}
