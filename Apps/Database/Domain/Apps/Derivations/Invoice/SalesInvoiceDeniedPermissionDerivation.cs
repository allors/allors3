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
            new ChangedPattern(this.M.SalesInvoice.TransitionalDeniedPermissions),
            new ChangedPattern(this.M.SalesInvoiceItem.SalesInvoiceItemState) { Steps = new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem } },
            new ChangedPattern(this.M.RepeatingSalesInvoice.Source) { Steps = new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.ExistSalesInvoiceState &&
                    @this.SalesInvoiceState.Equals(new SalesInvoiceStates(@this.Strategy.Transaction).ReadyForPosting) &&
                    @this.SalesInvoiceItems.All(v => v.IsDeletable) &&
                    !@this.ExistSalesOrders &&
                    !@this.ExistPurchaseInvoice &&
                    !@this.ExistRepeatingSalesInvoiceWhereSource)
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
