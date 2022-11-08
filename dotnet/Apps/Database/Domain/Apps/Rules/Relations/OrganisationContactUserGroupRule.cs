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

    public class OrganisationContactUserGroupRule : Rule
    {
        public OrganisationContactUserGroupRule(MetaPopulation m) : base(m, new Guid("cc2446db-a9e3-4a57-841b-ed6c903657f3")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.Name),
                m.Organisation.RolePattern(v => v.UniqueId),
                m.Organisation.RolePattern(v => v.CurrentContacts),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Organisation>())
            {
                transaction.Prefetch(@this.PrefetchPolicy, @this);

                if (!@this.ExistContactsUserGroup)
                {
                    var customerContactGroupName = $"Customer contacts at {@this.Name ?? "Unknown"} ({@this.UniqueId})";
                    @this.ContactsUserGroup = new UserGroupBuilder(@this.Strategy.Transaction).WithName(customerContactGroupName).Build();
                    @this.ContactsUserGroup.IsSelectable = @this.IsInternalOrganisation;
                }

                @this.ContactsUserGroup.Members = @this.CurrentContacts.ToArray();
            }
        }
    }
}
