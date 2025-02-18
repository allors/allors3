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

    public class EmploymentDisplayPartyRelationshipRule : Rule
    {
        public EmploymentDisplayPartyRelationshipRule(MetaPopulation m) : base(m, new Guid("0c76cdad-ec05-4118-9c88-d38dc8c5857a")) =>
        this.Patterns = new Pattern[]
        {
            m.Employment.RolePattern(v => v.Employee),
            m.Employment.RolePattern(v => v.Employer),
            m.Person.RolePattern(v => v.DisplayName, v => v.EmploymentsWhereEmployee),
            m.Organisation.RolePattern(v => v.DisplayName, v => v.EmploymentsWhereEmployer),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Employment>())
            {
                @this.DeriveEmploymentDisplayPartyRelationship(validation);
            }
        }
    }

    public static class EmploymentDisplayPartyRelationshipRuleExtensions
    {
        public static void DeriveEmploymentDisplayPartyRelationship(this Employment @this, IValidation validation) => @this.DisplayPartyRelationship = $"{@this.Employee?.DisplayName} is employee at {@this.Employer?.DisplayName}";
    }
}
