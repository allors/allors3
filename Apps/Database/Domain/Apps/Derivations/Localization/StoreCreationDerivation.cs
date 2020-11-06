// <copyright file="PartyFinancialRelationshipCreationDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class StoreCreationDerivation : DomainDerivation
    {
        public StoreCreationDerivation(M m) : base(m, new Guid("e0646540-b470-4db3-a627-dfe38a560819")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(m.Store.Class)
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Store>())
            {
                if (!@this.ExistAutoGenerateCustomerShipment)
                {
                    @this.AutoGenerateCustomerShipment = true;
                }

                if (!@this.ExistAutoGenerateShipmentPackage)
                {
                    @this.AutoGenerateShipmentPackage = true;
                }

                if (!@this.ExistSalesOrderCounter)
                {
                    @this.SalesOrderCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!@this.ExistOutgoingShipmentCounter)
                {
                    @this.OutgoingShipmentCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!@this.ExistBillingProcess)
                {
                    @this.BillingProcess = new BillingProcesses(@this.Strategy.Session).BillingForShipmentItems;
                }

                if (!@this.ExistSalesInvoiceTemporaryCounter)
                {
                    @this.SalesInvoiceTemporaryCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!@this.ExistCreditNoteCounter)
                {
                    @this.CreditNoteCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }
            }
        }
    }
}
