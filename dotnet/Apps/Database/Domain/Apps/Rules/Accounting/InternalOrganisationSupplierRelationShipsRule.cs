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

    public class InternalOrganisationSupplierRelationShipsRule : Rule
    {
        public InternalOrganisationSupplierRelationShipsRule(MetaPopulation m) : base(m, new Guid("91e75f3d-41c7-4185-845c-1b219675069a")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.DerivationTrigger, v => v.SupplierRelationshipsWhereSupplier),
                m.SupplierRelationship.RolePattern(v => v.InternalOrganisation),
                m.InternalOrganisation.RolePattern(v => v.ExportAccounting, v => v.SupplierRelationshipsWhereInternalOrganisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                if (@this.ExistInternalOrganisation && @this.InternalOrganisation.ExportAccounting)
                {
                    var partyFinancial = @this.InternalOrganisation.PartyFinancialRelationshipsWhereInternalOrganisation.FirstOrDefault(v => Equals(v.FinancialParty, @this.Supplier) && !v.Debtor);

                    if (partyFinancial == null)
                    {
                        var partyFinancialRelationShip = new PartyFinancialRelationshipBuilder(@this.Strategy.Transaction)
                            .WithFinancialParty(@this.Supplier)
                            .WithInternalOrganisation(@this.InternalOrganisation)
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
