// <copyright file="PartDefaultFacilityNameRule.cs" company="Allors bv">
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

    public class PartDefaultFacilityNameRule : Rule
    {
        public PartDefaultFacilityNameRule(MetaPopulation m) : base(m, new Guid("18d97936-6b7a-4249-a110-d22aa37a5948")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.DefaultFacility),
                m.Facility.RolePattern(v => v.Name, v => v.PartsWhereDefaultFacility),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartDefaultFacilityName(validation);
            }
        }
    }

    public static class PartDefaultFacilityNameRuleExtensions
    {
        public static void DerivePartDefaultFacilityName(this Part @this, IValidation validation) => @this.DefaultFacilityName = @this.DefaultFacility?.Name;
    }
}
