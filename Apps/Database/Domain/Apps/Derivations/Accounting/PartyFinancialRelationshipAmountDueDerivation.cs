// <copyright file="PartyFinancialRelationshipAmountDueDerivation.cs" company="Allors bvba">
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

    public class PartyFinancialRelationshipAmountDueDerivation : DomainDerivation
    {
        public PartyFinancialRelationshipAmountDueDerivation(M m) : base(m, new Guid("0f4cb6d0-79ca-4a5f-ba8f-d69b67448a96")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesInvoice.TotalIncVat) { Steps =  new IPropertyType[] {m.SalesInvoice.BillToCustomer, m.Party.PartyFinancialRelationshipsWhereFinancialParty } },
                new ChangedPattern(m.SalesInvoice.AmountPaid) { Steps =  new IPropertyType[] {m.SalesInvoice.BillToCustomer, m.Party.PartyFinancialRelationshipsWhereFinancialParty } },
                new ChangedPattern(m.Store.PaymentGracePeriod) { Steps =  new IPropertyType[] { m.Store.SalesInvoicesWhereStore, m.SalesInvoice.BillToCustomer, m.Party.PartyFinancialRelationshipsWhereFinancialParty } }
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartyFinancialRelationship>())
            {
                if (@this != null)
                {
                    var party = @this.FinancialParty;

                    @this.AmountDue = 0;
                    @this.AmountOverDue = 0;

                    // Amount Due
                    foreach (var salesInvoice in party.SalesInvoicesWhereBillToCustomer.Where(v => Equals(v.BilledFrom, @this.InternalOrganisation) &&
                        !v.SalesInvoiceState.Equals(new SalesInvoiceStates(party.Strategy.Session).Paid)))
                    {

                        @this.AmountDue += salesInvoice.TotalIncVat - salesInvoice.AmountPaid;

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
                                @this.AmountOverDue += salesInvoice.TotalIncVat - salesInvoice.AmountPaid;
                            }
                        }
                    }
                }
            }
        }
    }
}
