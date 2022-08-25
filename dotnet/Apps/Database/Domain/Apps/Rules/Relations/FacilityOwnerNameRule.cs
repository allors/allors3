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

    public class FacilityOwnerNameRule : Rule
    {
        public FacilityOwnerNameRule(MetaPopulation m) : base(m, new Guid("80041bd0-bbd7-46af-bfa9-6496bd693736")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.Owner),
                m.InternalOrganisation.RolePattern(v => v.DisplayName, v => v.FacilitiesWhereOwner),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilityOwnerName(validation);
            }
        }
    }

    public static class FacilityOwnerNameRuleExtensions
    {
        public static void DeriveFacilityOwnerName(this Facility @this, IValidation validation)
        {
            @this.OwnerName = @this.ExistOwner ? @this.Owner.DisplayName : "N/A";
        }
    }
}
