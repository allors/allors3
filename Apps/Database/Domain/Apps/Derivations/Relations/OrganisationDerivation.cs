// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class OrganisationDerivation : DomainDerivation
    {
        public OrganisationDerivation(M m) : base(m, new Guid("0379B923-210D-46DD-9D18-9D7BF5ED6FEA")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Organisation.Name),
                new ChangedPattern(m.Organisation.DerivationTrigger),
                new ChangedPattern(m.Employment.Employer) {Steps = new IPropertyType[]{ m.Employment.Employer} },
                new ChangedPattern(m.Employment.FromDate) {Steps = new IPropertyType[]{ m.Employment.Employer}, OfType = this.M.Organisation.Class},
                new ChangedPattern(m.Employment.ThroughDate) {Steps = new IPropertyType[]{ m.Employment.Employer}, OfType = this.M.Organisation.Class},
                new ChangedPattern(m.CustomerRelationship.InternalOrganisation) {Steps = new IPropertyType[]{ m.CustomerRelationship.InternalOrganisation} },
                new ChangedPattern(m.CustomerRelationship.FromDate) {Steps = new IPropertyType[]{ m.CustomerRelationship.InternalOrganisation} },
                new ChangedPattern(m.CustomerRelationship.ThroughDate) {Steps = new IPropertyType[]{ m.CustomerRelationship.InternalOrganisation} },
                new ChangedPattern(m.SupplierRelationship.FromDate) {Steps = new IPropertyType[]{ m.SupplierRelationship.InternalOrganisation}, OfType = this.M.Organisation.Class},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var @this in matches.Cast<Organisation>())
            {
                var now = session.Now();

                session.Prefetch(@this.PrefetchPolicy);

                @this.PartyName = @this.Name;

                @this.ActiveEmployees = @this.EmploymentsWhereEmployer
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Employee)
                    .ToArray();

                (@this).ActiveCustomers = @this.CustomerRelationshipsWhereInternalOrganisation
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .Select(v => v.Customer)
                    .ToArray();

                if (!@this.ExistContactsUserGroup)
                {
                    var customerContactGroupName = $"Customer contacts at {@this.Name} ({@this.UniqueId})";
                    @this.ContactsUserGroup = new UserGroupBuilder(@this.Strategy.Session).WithName(customerContactGroupName).Build();
                }

                @this.DeriveRelationships();

                @this.ContactsUserGroup.Members = @this.CurrentContacts.ToArray();

                @this.DeriveRelationships();
            }
        }
    }
}
