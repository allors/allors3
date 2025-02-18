// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class CustomerRelationshipDisplayPartyRelationshipRule : Rule
    {
        public CustomerRelationshipDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("e121bf25-ba6d-4cdd-be19-f6d93bce2db6")) =>
        this.Patterns = new Pattern[]
        {
            m.CustomerRelationship.RolePattern(v => v.Customer),
            m.CustomerRelationship.RolePattern(v => v.InternalOrganisation),
            m.Party.RolePattern(v => v.DisplayName, v => v.CustomerRelationshipsWhereCustomer),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.CustomerRelationshipsWhereInternalOrganisation),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                @this.DeriveCustomerRelationshipDisplayPartyRelationship(validation);
            }
        }
    }

    public static class CustomerRelationshipDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveCustomerRelationshipDisplayPartyRelationship(this CustomerRelationship @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.Customer?.DisplayName} is customer of {@this.InternalOrganisation?.DisplayName}";
    }
}
