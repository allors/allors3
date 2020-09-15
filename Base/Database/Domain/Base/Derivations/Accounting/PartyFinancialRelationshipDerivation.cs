// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class PartyFinancialRelationshipDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("8403E05C-8C82-47E9-8649-748294FC8463");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.PartyFinancialRelationship.Class),
            new ChangedConcreteRolePattern(M.SalesInvoice.TotalIncVat) { Steps =  new IPropertyType[] {M.SalesInvoice.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } },
            new ChangedConcreteRolePattern(M.SalesInvoice.AmountPaid) { Steps =  new IPropertyType[] {M.SalesInvoice.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } },
            new ChangedConcreteRolePattern(M.SalesOrder.TotalIncVat) { Steps =  new IPropertyType[] {M.SalesOrder.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } },
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var partyFinancialRelationship in matches.Cast<PartyFinancialRelationship>())
            {
                if (partyFinancialRelationship != null)
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
}
