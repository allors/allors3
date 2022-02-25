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

    public class StoreRule : Rule
    {
        public StoreRule(MetaPopulation m) : base(m, new Guid("cf3acae4-a895-4a0b-b154-18cfa30691bb")) =>
            this.Patterns = new Pattern[]
            {
                m.Store.RolePattern(v => v.InternalOrganisation),
                m.Store.RolePattern(v => v.DefaultCollectionMethod),
                m.Store.RolePattern(v => v.CollectionMethods),
                m.Store.RolePattern(v => v.FiscalYearsStoreSequenceNumbers),
                m.Store.RolePattern(v => v.SalesInvoiceNumberCounter),
                m.Store.RolePattern(v => v.CustomerShipmentNumberPrefix),
                m.Store.RolePattern(v => v.DropShipmentNumberPrefix),
                m.Store.RolePattern(v => v.OutgoingTransferNumberPrefix),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Store>())
            {
                if (@this.ExistDefaultCollectionMethod && !@this.CollectionMethods.Contains(@this.DefaultCollectionMethod))
                {
                    @this.AddCollectionMethod(@this.DefaultCollectionMethod);
                }

                if (!@this.ExistDefaultCollectionMethod && @this.CollectionMethods.Count() == 1)
                {
                    @this.DefaultCollectionMethod = @this.CollectionMethods.FirstOrDefault();
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
