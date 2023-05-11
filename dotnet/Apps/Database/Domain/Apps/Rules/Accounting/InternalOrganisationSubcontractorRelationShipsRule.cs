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

    public class InternalOrganisationSubcontractorRelationShipsRule : Rule
    {
        public InternalOrganisationSubcontractorRelationShipsRule(MetaPopulation m) : base(m, new Guid("5bf15691-7f9d-4220-b49a-9d5270a97e57")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.DerivationTrigger, v => v.SubContractorRelationshipsWhereSubContractor),
                m.SubContractorRelationship.RolePattern(v => v.Contractor),
                m.InternalOrganisation.RolePattern(v => v.ExportAccounting, v => v.SubContractorRelationshipsWhereContractor),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                if (@this.ExistContractor && @this.Contractor.ExportAccounting)
                {
                    var partyFinancial = @this.Contractor.PartyFinancialRelationshipsWhereInternalOrganisation.FirstOrDefault(v => Equals(v.FinancialParty, @this.SubContractor) && v.Debtor);

                    if (partyFinancial == null)
                    {
                        var partyFinancialRelationShip = new PartyFinancialRelationshipBuilder(@this.Strategy.Transaction)
                            .WithFinancialParty(@this.SubContractor)
                            .WithInternalOrganisation(@this.Contractor)
                            .WithDebtor(true)
                            .WithInternalSubAccountNumber(@this.Contractor.NextSubAccountNumber())
                            .WithFromDate(@this.FromDate)
                            .WithThroughDate(@this.ThroughDate)
                            .Build();
                    }
                }
            }
        }
    }
}
