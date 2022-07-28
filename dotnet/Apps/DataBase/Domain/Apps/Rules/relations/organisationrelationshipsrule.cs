// <copyright file="Domain.cs" company="Allors bvba">
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
    using Derivations.Rules;

    public class OrganisationRelationshipsRule: Rule
    {
        public OrganisationRelationshipsRule(MetaPopulation m) : base(m, new Guid("0379B923-210D-46DD-9D18-9D7BF5ED6FEA")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.DerivationTrigger),
                m.InternalOrganisation.AssociationPattern(v => v.EmploymentsWhereEmployer),
                m.Employment.RolePattern(v => v.FromDate, v => v.Employer, m.Organisation),
                m.Employment.RolePattern(v => v.ThroughDate, v => v.Employer, m.Organisation),
                m.InternalOrganisation.AssociationPattern(v => v.CustomerRelationshipsWhereInternalOrganisation),
                m.CustomerRelationship.RolePattern(v => v.FromDate, v => v.InternalOrganisation),
                m.CustomerRelationship.RolePattern(v => v.ThroughDate, v => v.InternalOrganisation),
                m.InternalOrganisation.AssociationPattern(v => v.SupplierRelationshipsWhereInternalOrganisation),
                m.SupplierRelationship.RolePattern(v => v.FromDate, v => v.InternalOrganisation),
                m.SupplierRelationship.RolePattern(v => v.ThroughDate, v => v.InternalOrganisation),
                m.Organisation.AssociationPattern(v => v.OrganisationContactRelationshipsWhereOrganisation),
                m.OrganisationContactRelationship.RolePattern(v => v.FromDate, v => v.Organisation),
                m.OrganisationContactRelationship.RolePattern(v => v.ThroughDate, v => v.Organisation),
                m.InternalOrganisation.AssociationPattern(v => v.SubContractorRelationshipsWhereContractor),
                m.SubContractorRelationship.RolePattern(v => v.FromDate, v => v.Contractor),
                m.SubContractorRelationship.RolePattern(v => v.ThroughDate, v => v.Contractor),
                m.Party.AssociationPattern(v => v.PartyContactMechanismsWhereParty, m.Organisation),
                m.PartyContactMechanism.RolePattern(v => v.FromDate, v => v.Party.Party, m.Organisation),
                m.PartyContactMechanism.RolePattern(v => v.ThroughDate,v => v.Party.Party, m.Organisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Organisation>())
            {
                transaction.Prefetch(@this.PrefetchPolicy, @this);
                @this.DeriveOrganisationRelationships(validation);
            }
        }
    }

    public static class OrganisationrelationshipsRuleExtensions
    {
        public static void DeriveOrganisationRelationships(this Organisation @this, IValidation validation) => @this.DeriveRelationships();
    }
}
