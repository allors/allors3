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

    public class FacilityFacilityTypeNameRule : Rule
    {
        public FacilityFacilityTypeNameRule(MetaPopulation m) : base(m, new Guid("27b46677-dcfb-4c13-9f4c-3110f7d060ae")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.FacilityType),
                m.FacilityType.RolePattern(v => v.Name, v => v.FacilitiesWhereFacilityType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilityFacilityTypeName(validation);
            }
        }
    }

    public static class FacilityFacilityTypeNameRuleExtensions
    {
        public static void DeriveFacilityFacilityTypeName(this Facility @this, IValidation validation)
        {
            @this.FacilityTypeName = @this.ExistFacilityType ? @this.FacilityType.Name : "N/A";
        }
    }
}
