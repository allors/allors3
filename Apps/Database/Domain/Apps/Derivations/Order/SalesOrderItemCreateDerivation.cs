// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class SalesOrderItemCreateDerivation : DomainDerivation
    {
        public SalesOrderItemCreateDerivation(M m) : base(m, new Guid("b1d7286c-fe47-4e09-8433-5796484286a3")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.SalesOrderItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                if (!@this.ExistSalesOrderItemState)
                {
                    @this.SalesOrderItemState = new SalesOrderItemStates(@this.Session()).Provisional;
                }

                if (@this.ExistProduct && !@this.ExistInvoiceItemType)
                {
                    @this.InvoiceItemType = new InvoiceItemTypes(@this.Session()).ProductItem;
                }

                if (!@this.ExistSalesOrderItemShipmentState)
                {
                    @this.SalesOrderItemShipmentState = new SalesOrderItemShipmentStates(@this.Session()).NotShipped;
                }

                if (!@this.ExistSalesOrderItemInvoiceState)
                {
                    @this.SalesOrderItemInvoiceState = new SalesOrderItemInvoiceStates(@this.Session()).NotInvoiced;
                }

                if (!@this.ExistSalesOrderItemPaymentState)
                {
                    @this.SalesOrderItemPaymentState = new SalesOrderItemPaymentStates(@this.Session()).NotPaid;
                }
            }
        }
    }
}
