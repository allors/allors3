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

    public class InternalOrganisationDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("258A6E3B-7940-4FCC-A33E-AE07C6FBFC32");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.InternalOrganisation.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var internalOrganisationExtension in matches.Cast<InternalOrganisation>())
            {
                var organisation = (Organisation)internalOrganisationExtension;
                if (organisation.IsInternalOrganisation)
                {
                    if (!internalOrganisationExtension.ExistDefaultCollectionMethod && internalOrganisationExtension.Strategy.Session.Extent<PaymentMethod>().Count == 1)
                    {
                        internalOrganisationExtension.DefaultCollectionMethod = internalOrganisationExtension.Strategy.Session.Extent<PaymentMethod>().First;
                    }

                    if (!internalOrganisationExtension.ExistPurchaseInvoiceCounter)
                    {
                        internalOrganisationExtension.PurchaseInvoiceCounter = new CounterBuilder(internalOrganisationExtension.Strategy.Session)
                            .WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                    }

                    if (!internalOrganisationExtension.ExistRequestCounter)
                    {
                        internalOrganisationExtension.RequestCounter = new CounterBuilder(internalOrganisationExtension.Strategy.Session).WithUniqueId(Guid.NewGuid())
                            .WithValue(0).Build();
                    }

                    if (!internalOrganisationExtension.ExistQuoteCounter)
                    {
                        internalOrganisationExtension.QuoteCounter = new CounterBuilder(internalOrganisationExtension.Strategy.Session).WithUniqueId(Guid.NewGuid())
                            .WithValue(0).Build();
                    }

                    if (!internalOrganisationExtension.ExistPurchaseOrderCounter)
                    {
                        internalOrganisationExtension.PurchaseOrderCounter = new CounterBuilder(internalOrganisationExtension.Strategy.Session).WithUniqueId(Guid.NewGuid())
                            .WithValue(0).Build();
                    }

                    if (!internalOrganisationExtension.ExistIncomingShipmentCounter)
                    {
                        internalOrganisationExtension.IncomingShipmentCounter = new CounterBuilder(internalOrganisationExtension.Strategy.Session)
                            .WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                    }

                    if (!internalOrganisationExtension.ExistSubAccountCounter)
                    {
                        internalOrganisationExtension.SubAccountCounter = new CounterBuilder(internalOrganisationExtension.Strategy.Session).WithUniqueId(Guid.NewGuid())
                            .WithValue(0).Build();
                    }

                    if (!internalOrganisationExtension.ExistInvoiceSequence)
                    {
                        internalOrganisationExtension.InvoiceSequence = new InvoiceSequences(internalOrganisationExtension.Strategy.Session).RestartOnFiscalYear;
                    }

                    if (!internalOrganisationExtension.ExistFiscalYearStartMonth)
                    {
                        internalOrganisationExtension.FiscalYearStartMonth = 1;
                    }

                    if (!internalOrganisationExtension.ExistFiscalYearStartDay)
                    {
                        internalOrganisationExtension.FiscalYearStartDay = 1;
                    }
                }
            }
        }
    }
}
