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
    using Derivations.Rules;
    using Meta;

    public class InternalOrganisationCustomerRelationShipsRule : Rule
    {
        public InternalOrganisationCustomerRelationShipsRule(MetaPopulation m) : base(m, new Guid("851c1595-5d71-4136-9523-90d9ca234a8b")) =>
            this.Patterns = new Pattern[]
            {
                m.Party.RolePattern(v => v.DerivationTrigger, v => v.CustomerRelationshipsWhereCustomer),
                m.CustomerRelationship.RolePattern(v => v.InternalOrganisation),
                m.InternalOrganisation.RolePattern(v => v.ExportAccounting, v => v.CustomerRelationshipsWhereInternalOrganisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                if (@this.InternalOrganisation.ExportAccounting)
                {
                    var partyFinancial = @this.InternalOrganisation.PartyFinancialRelationshipsWhereInternalOrganisation.FirstOrDefault(v => Equals(v.FinancialParty, @this.Customer) && v.Debtor);

                    if (partyFinancial == null)
                    {
                        var partyFinancialRelationShip = new PartyFinancialRelationshipBuilder(@this.Strategy.Transaction)
                            .WithFinancialParty(@this.Customer)
                            .WithInternalOrganisation(@this.InternalOrganisation)
                            .WithDebtor(true)
                            .WithInternalSubAccountNumber(@this.InternalOrganisation.NextSubAccountNumber())
                            .WithFromDate(@this.FromDate)
                            .WithThroughDate(@this.ThroughDate)
                            .Build();
                    }
                }
            }
        }
    }
}
