// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class InternalOrganisationExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdInternalOrganisationExtensions = changeSet.Created.Select(v=>v.GetObject()).OfType<InternalOrganisation>();

                foreach (var internalOrganisationExtension in createdInternalOrganisationExtensions)
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

        public static void InternalOrganisationExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("b0b78130-464e-4bdd-b41d-73dc6141dfc7")] = new InternalOrganisationExtensionsCreationDerivation();
        }
    }
}
