// <copyright file="PartyFinancialRelationshipOpenOrderAmountDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class PartyFinancialRelationshipOpenOrderAmountDerivation : DomainDerivation
    {
        public PartyFinancialRelationshipOpenOrderAmountDerivation(M m) : base(m, new Guid("3132e3d6-69be-4dde-b06c-f0162f8aa5ed")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedConcreteRolePattern(M.SalesOrder.TotalIncVat) { Steps =  new IPropertyType[] {M.SalesOrder.BillToCustomer, M.Party.PartyFinancialRelationshipsWhereFinancialParty } }
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var partyFinancialRelationship in matches.Cast<PartyFinancialRelationship>())
            {
                if (partyFinancialRelationship != null)
                {
                    var party = partyFinancialRelationship.FinancialParty;

                    // Open Order Amount
                    partyFinancialRelationship.OpenOrderAmount = party.SalesOrdersWhereBillToCustomer
                        .Where(v =>
                            Equals(v.TakenBy, partyFinancialRelationship.InternalOrganisation) &&
                            !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Session).Finished) &&
                            !v.SalesOrderState.Equals(new SalesOrderStates(party.Strategy.Session).Cancelled))
                        .Sum(v => v.TotalIncVat);
                }
            }
        }
    }
}
