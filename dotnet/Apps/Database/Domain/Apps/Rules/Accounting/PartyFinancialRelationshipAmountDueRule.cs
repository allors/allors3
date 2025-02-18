// <copyright file="PartyFinancialRelationshipAmountDueDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class PartyFinancialRelationshipAmountDueRule : Rule
    {
        public PartyFinancialRelationshipAmountDueRule(MetaPopulation m) : base(m, new Guid("0f4cb6d0-79ca-4a5f-ba8f-d69b67448a96")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoice.RolePattern(v => v.TotalIncVat, v => v.BillToCustomer.ObjectType.PartyFinancialRelationshipsWhereFinancialParty),
                m.SalesInvoice.RolePattern(v => v.AmountPaid, v => v.BillToCustomer.ObjectType.PartyFinancialRelationshipsWhereFinancialParty),
                m.SalesInvoice.RolePattern(v => v.DueDate, v => v.BillToCustomer.ObjectType.PartyFinancialRelationshipsWhereFinancialParty),
                m.Store.RolePattern(v => v.PaymentGracePeriod, v => v.SalesInvoicesWhereStore.ObjectType.BillToCustomer.ObjectType.PartyFinancialRelationshipsWhereFinancialParty),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
                        !v.SalesInvoiceState.Equals(new SalesInvoiceStates(party.Strategy.Transaction).Paid)))
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

                            if (party.Strategy.Transaction.Now() > dueDate)
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
