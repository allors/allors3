// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class StoreDerivation : DomainDerivation
    {
        public StoreDerivation(M m) : base(m, new Guid("cf3acae4-a895-4a0b-b154-18cfa30691bb")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Store.InternalOrganisation),
                new RolePattern(m.Store.DefaultCollectionMethod),
                new RolePattern(m.Store.CollectionMethods),
                new RolePattern(m.Store.FiscalYearsStoreSequenceNumbers),
                new RolePattern(m.Store.SalesInvoiceNumberCounter),
                new RolePattern(m.Store.CustomerShipmentNumberPrefix),
                new RolePattern(m.Store.PurchaseReturnNumberPrefix),
                new RolePattern(m.Store.DropShipmentNumberPrefix),
                new RolePattern(m.Store.OutgoingTransferNumberPrefix),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Store>())
            {
                if (@this.ExistDefaultCollectionMethod && !@this.CollectionMethods.Contains(@this.DefaultCollectionMethod))
                {
                    @this.AddCollectionMethod(@this.DefaultCollectionMethod);
                }

                if (!@this.ExistDefaultCollectionMethod && @this.CollectionMethods.Count == 1)
                {
                    @this.DefaultCollectionMethod = @this.CollectionMethods.First;
                }

                if (!@this.ExistDefaultCollectionMethod && @this.InternalOrganisation.ExistDefaultCollectionMethod)
                {
                    @this.DefaultCollectionMethod = @this.InternalOrganisation.DefaultCollectionMethod;

                    if (!@this.ExistCollectionMethods || !@this.CollectionMethods.Contains(@this.DefaultCollectionMethod))
                    {
                        @this.AddCollectionMethod(@this.DefaultCollectionMethod);
                    }
                }

                if (@this.InternalOrganisation.InvoiceSequence != new InvoiceSequences(@this.Transaction()).RestartOnFiscalYear)
                {
                    if (!@this.ExistSalesInvoiceNumberCounter)
                    {
                        @this.SalesInvoiceNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }

                    if (!@this.ExistSalesOrderNumberCounter)
                    {
                        @this.SalesOrderNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }

                    if (!@this.ExistCreditNoteNumberCounter)
                    {
                        @this.CreditNoteNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                    }
                }

                if (@this.InternalOrganisation.CustomerShipmentSequence != new CustomerShipmentSequences(@this.Transaction()).RestartOnFiscalYear && !@this.ExistCustomerShipmentNumberCounter)
                {
                    @this.CustomerShipmentNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                }

                if (@this.InternalOrganisation.PurchaseReturnSequence != new PurchaseReturnSequences(@this.Transaction()).RestartOnFiscalYear && !@this.ExistPurchaseReturnNumberCounter)
                {
                    @this.PurchaseReturnNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                }

                if (@this.InternalOrganisation.DropShipmentSequence != new DropShipmentSequences(@this.Transaction()).RestartOnFiscalYear && !@this.ExistDropShipmentNumberCounter)
                {
                    @this.DropShipmentNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                }

                if (@this.InternalOrganisation.OutgoingTransferSequence != new OutgoingTransferSequences(@this.Transaction()).RestartOnFiscalYear && !@this.ExistOutgoingTransferNumberCounter)
                {
                    @this.OutgoingTransferNumberCounter = new CounterBuilder(@this.Strategy.Transaction).Build();
                }

                validation.AssertExistsAtMostOne(@this, @this.M.Store.FiscalYearsStoreSequenceNumbers, @this.M.Store.SalesInvoiceNumberCounter);
            }
        }
    }
}
