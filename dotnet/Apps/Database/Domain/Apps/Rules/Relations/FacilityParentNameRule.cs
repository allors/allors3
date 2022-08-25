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

    public class FacilityParentNameRule : Rule
    {
        public FacilityParentNameRule(MetaPopulation m) : base(m, new Guid("cdb74b71-0af2-4d9c-884c-61c833e58b5d")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.ParentFacility),
                m.Facility.RolePattern(v => v.Name, v => v.FacilitiesWhereParentFacility),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilityParentName(validation);
            }
        }
    }

    public static class FacilityParentNameRuleExtensions
    {
        public static void DeriveFacilityParentName(this Facility @this, IValidation validation)
        {
            @this.ParentName = @this.ExistParentFacility ? @this.ParentFacility.Name : string.Empty;
        }
    }
}
