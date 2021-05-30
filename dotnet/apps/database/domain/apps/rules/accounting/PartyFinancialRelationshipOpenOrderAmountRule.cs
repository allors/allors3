// <copyright file="PartyFinancialRelationshipOpenOrderAmountDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class PartyFinancialRelationshipOpenOrderAmountRule : Rule
    {
        public PartyFinancialRelationshipOpenOrderAmountRule(MetaPopulation m) : base(m, new Guid("3132e3d6-69be-4dde-b06c-f0162f8aa5ed")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.TotalIncVat, v => v.BillToCustomer.Party.PartyFinancialRelationshipsWhereFinancialParty),
                m.SalesOrder.RolePattern(v => v.SalesOrderState, v => v.BillToCustomer.Party.PartyFinancialRelationshipsWhereFinancialParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartyFinancialRelationship>())
            {
                if (@this != null)
                {
                    var party = @this.FinancialParty;

                    // Open Order Amount
                    @this.OpenOrderAmount = party.SalesOrdersWhereBillToCustomer
                        .Where(v =>
                            Equals(v.TakenBy, @this.InternalOrganisation)
                            && v.ExistSalesOrderState
                            && !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Transaction).Finished)
                            && !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Transaction).Cancelled))
                        .Sum(v => v.TotalIncVat);
                }
            }
        }
    }
}
