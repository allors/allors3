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

    public class InternalOrganisationDerivation : DomainDerivation
    {
        public InternalOrganisationDerivation(M m) : base(m, new Guid("258A6E3B-7940-4FCC-A33E-AE07C6FBFC32")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.InternalOrganisation.DefaultCollectionMethod),
                new ChangedPattern(this.M.InternalOrganisation.AssignedActiveCollectionMethods),
                new ChangedPattern(this.M.InternalOrganisation.IsInternalOrganisation),
                new ChangedPattern(this.M.InternalOrganisation.InvoiceSequence),
                new ChangedPattern(this.M.InternalOrganisation.RequestSequence),
                new ChangedPattern(this.M.InternalOrganisation.QuoteSequence),
                new ChangedPattern(this.M.InternalOrganisation.WorkEffortSequence),
                new ChangedPattern(this.M.InternalOrganisation.PurchaseShipmentSequence),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InternalOrganisation>())
            {
                var organisation = (Organisation)@this;
                if (organisation.IsInternalOrganisation)
                {
                    if (!@this.ExistDefaultCollectionMethod && @this.Strategy.Session.Extent<PaymentMethod>().Count == 1)
                    {
                        @this.DefaultCollectionMethod = @this.Strategy.Session.Extent<PaymentMethod>().First;
                    }

                    @this.DerivedActiveCollectionMethods = @this.AssignedActiveCollectionMethods;
                    @this.AddDerivedActiveCollectionMethod(@this.DefaultCollectionMethod);

                    if (@this.InvoiceSequence != new InvoiceSequences(@this.Strategy.Session).RestartOnFiscalYear)
                    {
                        if (!@this.ExistPurchaseInvoiceNumberCounter)
                        {
                            @this.PurchaseInvoiceNumberCounter = new CounterBuilder(@this.Strategy.Session).Build();
                        }

                        if (!@this.ExistPurchaseOrderNumberCounter)
                        {
                            @this.PurchaseOrderNumberCounter = new CounterBuilder(@this.Strategy.Session).Build();
                        }
                    }

                    if (@this.RequestSequence != new RequestSequences(@this.Strategy.Session).RestartOnFiscalYear && !@this.ExistRequestNumberCounter)
                    {
                        @this.RequestNumberCounter = new CounterBuilder(@this.Strategy.Session).Build();
                    }

                    if (@this.QuoteSequence != new QuoteSequences(@this.Strategy.Session).RestartOnFiscalYear && !@this.ExistQuoteNumberCounter)
                    {
                        @this.QuoteNumberCounter = new CounterBuilder(@this.Strategy.Session).Build();
                    }

                    if (@this.WorkEffortSequence != new WorkEffortSequences(@this.Strategy.Session).RestartOnFiscalYear && !@this.ExistWorkEffortNumberCounter)
                    {
                        @this.WorkEffortNumberCounter = new CounterBuilder(@this.Strategy.Session).Build();
                    }

                    if (@this.PurchaseShipmentSequence != new PurchaseShipmentSequences(@this.Strategy.Session).RestartOnFiscalYear && !@this.ExistIncomingShipmentNumberCounter)
                    {
                        @this.IncomingShipmentNumberCounter = new CounterBuilder(@this.Strategy.Session).Build();
                    }
                }
            }
        }
    }
}
