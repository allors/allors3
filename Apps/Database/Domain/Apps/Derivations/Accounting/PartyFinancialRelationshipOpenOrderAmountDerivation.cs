// <copyright file="PartyFinancialRelationshipOpenOrderAmountDerivation.cs" company="Allors bvba">
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

    public class PartyFinancialRelationshipOpenOrderAmountDerivation : DomainDerivation
    {
        public PartyFinancialRelationshipOpenOrderAmountDerivation(M m) : base(m, new Guid("3132e3d6-69be-4dde-b06c-f0162f8aa5ed")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrder.TotalIncVat) { Steps =  new IPropertyType[] {m.SalesOrder.BillToCustomer, m.Party.PartyFinancialRelationshipsWhereFinancialParty } },
                new ChangedPattern(m.SalesOrder.SalesOrderState) { Steps =  new IPropertyType[] {m.SalesOrder.BillToCustomer, m.Party.PartyFinancialRelationshipsWhereFinancialParty } }
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
                            && !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Session).Finished)
                            && !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Session).Cancelled))
                        .Sum(v => v.TotalIncVat);
                }
            }
        }
    }
}
