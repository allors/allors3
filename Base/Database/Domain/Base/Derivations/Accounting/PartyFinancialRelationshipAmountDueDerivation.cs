// <copyright file="PartyFinancialRelationshipAmountDueDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class PartyFinancialRelationshipAmountDueDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("0f4cb6d0-79ca-4a5f-ba8f-d69b67448a96");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedConcreteRolePattern(M.SalesInvoice.TotalIncVat) { Steps =  new IPropertyType[] {M.SalesInvoice.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } },
            new ChangedConcreteRolePattern(M.SalesInvoice.AmountPaid) { Steps =  new IPropertyType[] {M.SalesInvoice.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } },
            new ChangedRolePattern(M.Store.PaymentGracePeriod) { Steps =  new IPropertyType[] { M.Store.SalesInvoicesWhereStore, M.SalesInvoice.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } }
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

                    // Amount Due
                    foreach (var salesInvoice in party.SalesInvoicesWhereBillToCustomer.Where(v => Equals(v.BilledFrom, partyFinancialRelationship.InternalOrganisation) &&
                        !v.SalesInvoiceState.Equals(new SalesInvoiceStates(party.Strategy.Session).Paid)))
                    {

                        partyFinancialRelationship.AmountDue += salesInvoice.TotalIncVat - salesInvoice.AmountPaid;

                        // Amount OverDue
                        var gracePeriod = salesInvoice.Store?.PaymentGracePeriod;

                        if (salesInvoice.DueDate.HasValue)
                        {
                            var dueDate = salesInvoice.DueDate.Value.Date;

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
                }
            }
        }
    }
}
