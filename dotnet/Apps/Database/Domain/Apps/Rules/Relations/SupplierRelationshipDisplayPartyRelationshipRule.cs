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

    public class SupplierRelationshipDisplayPartyRelationshipRule : Rule
    {
        public SupplierRelationshipDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("30b07b0b-4755-41d7-ab74-ad00604a32c2")) =>
        this.Patterns = new Pattern[]
        {
            m.SupplierRelationship.RolePattern(v => v.Supplier),
            m.SupplierRelationship.RolePattern(v => v.InternalOrganisation),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.SupplierRelationshipsWhereSupplier),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.SupplierRelationshipsWhereInternalOrganisation),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                @this.DeriveSupplierRelationshipDisplayPartyRelationship(validation);
            }
        }
    }

    public static class SupplierRelationshipDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveSupplierRelationshipDisplayPartyRelationship(this SupplierRelationship @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.Supplier?.DisplayName} is supplier for {@this.InternalOrganisation?.DisplayName}";
    }
}
