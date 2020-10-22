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

    public class SalesInvoiceDeletePermissionDerivation : DomainDerivation
    {
        public SalesInvoiceDeletePermissionDerivation(M m) : base(m, new Guid("8726348f-85af-429b-a514-55d00dbb14d9")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(this.M.SalesInvoice.Class),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var salesInvoice in matches.Cast<SalesInvoice>())
            {
                var deletePermission = new Permissions(salesInvoice.Strategy.Session).Get(salesInvoice.Meta.ObjectType, salesInvoice.Meta.Delete);
                if (salesInvoice.ExistSalesInvoiceState &&
                    salesInvoice.SalesInvoiceState.Equals(new SalesInvoiceStates(salesInvoice.Strategy.Session).ReadyForPosting) &&
                    salesInvoice.SalesInvoiceItems.All(v => v.IsDeletable) &&
                    !salesInvoice.ExistSalesOrders &&
                    !salesInvoice.ExistPurchaseInvoice &&
                    !salesInvoice.ExistRepeatingSalesInvoiceWhereSource &&
                    !salesInvoice.IsRepeatingInvoice)
                {
                    salesInvoice.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    salesInvoice.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
