// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;

    public static partial class DabaseExtensions
    {
        public class PartyFinancialRelationshipCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPartyFinancialRelationships = changeSet.Created.Select(v => v.GetObject()).OfType<PartyFinancialRelationship>();

                var createdSalesInvoices = changeSet.Created.Select(v => v.GetObject()).OfType<SalesInvoice>();

                foreach (var partyFinancialRelationship in createdPartyFinancialRelationships)
                {
                    DerivePartyFinancialRealtionship(partyFinancialRelationship);
                }

                foreach (var createdSalesInvoice in createdSalesInvoices)
                {
                    var organisation = createdSalesInvoice.BillToCustomer;
                    if(organisation != null)
                    {
                        var financialRelationship = organisation.PartyFinancialRelationshipsWhereFinancialParty.First;

                        DerivePartyFinancialRealtionship(financialRelationship);
                    }
                }

                static void DerivePartyFinancialRealtionship(PartyFinancialRelationship partyFinancialRelationship)
                {
                    if(partyFinancialRelationship != null)
                    {
                        var party = partyFinancialRelationship.FinancialParty;

                        partyFinancialRelationship.AmountDue = 0;
                        partyFinancialRelationship.AmountOverDue = 0;

                        // Open Order Amount
                        partyFinancialRelationship.OpenOrderAmount = party.SalesOrdersWhereBillToCustomer
                            .Where(v =>
                                Equals(v.TakenBy, partyFinancialRelationship.InternalOrganisation) &&
                                !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Session).Finished) &&
                                !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Session).Cancelled))
                            .Sum(v => v.TotalIncVat);

                        // Amount Due
                        // Amount OverDue
                        foreach (var salesInvoice in party.SalesInvoicesWhereBillToCustomer.Where(v => Equals(v.BilledFrom, partyFinancialRelationship.InternalOrganisation) &&
                                                                                                                !v.SalesInvoiceState.Equals(new SalesInvoiceStates(party.Strategy.Session).Paid)))
                        {
                            if (salesInvoice.AmountPaid > 0)
                            {
                                partyFinancialRelationship.AmountDue += salesInvoice.TotalIncVat - salesInvoice.AmountPaid;
                            }
                            else
                            {
                                foreach (SalesInvoiceItem invoiceItem in salesInvoice.InvoiceItems)
                                {
                                    if (!invoiceItem.SalesInvoiceItemState.Equals(
                                        new SalesInvoiceItemStates(party.Strategy.Session).Paid))
                                    {
                                        if (invoiceItem.ExistTotalIncVat)
                                        {
                                            partyFinancialRelationship.AmountDue += invoiceItem.TotalIncVat - invoiceItem.AmountPaid;
                                        }
                                    }
                                }
                            }

                            var gracePeriod = salesInvoice.Store?.PaymentGracePeriod;

                            if (salesInvoice.DueDate.HasValue)
                            {
                                var dueDate = salesInvoice.DueDate.Value;

                                if (gracePeriod.HasValue)
                                {
                                    dueDate = salesInvoice.DueDate.Value.AddDays(gracePeriod.Value);
                                }

                                if (party.Strategy.Session.Now() > dueDate)
                                {
                                    partyFinancialRelationship.AmountOverDue += salesInvoice.TotalIncVat - salesInvoice.AmountPaid;
                                }
                            }
                        }

                        var internalOrganisations = new Organisations(partyFinancialRelationship.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

                        if (!partyFinancialRelationship.ExistInternalOrganisation && internalOrganisations.Count() == 1)
                        {
                            partyFinancialRelationship.InternalOrganisation = internalOrganisations.First();
                        }

                        partyFinancialRelationship.Parties = new Party[] { partyFinancialRelationship.FinancialParty, partyFinancialRelationship.InternalOrganisation };
                    }
                }
            }
        }

        public static void PartyFinancialRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("645da664-7ebd-4603-a073-0730034e10ca")] = new PartyFinancialRelationshipCreationDerivation();
        }
    }
}
