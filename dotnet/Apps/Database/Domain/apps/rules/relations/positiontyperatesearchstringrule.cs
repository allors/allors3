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

    public class PositionTypeRateSearchStringRule : Rule
    {
        public PositionTypeRateSearchStringRule(MetaPopulation m) : base(m, new Guid("b56afcd7-acbc-4156-a41d-73f3019f796e")) =>
            this.Patterns = new Pattern[]
            {
                m.PositionTypeRate.RolePattern(v => v.RateType),
                m.PositionTypeRate.RolePattern(v => v.Frequency),
                m.PositionTypeRate.AssociationPattern(v => v.PositionTypesWherePositionTypeRate),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PositionTypeRate>())
            {
                var array = new string[] {
                    @this.RateType?.Name,
                    @this.Frequency?.Name,
                    @this.ExistPositionTypesWherePositionTypeRate ? string.Join(" ", @this.PositionTypesWherePositionTypeRate?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistPositionTypesWherePositionTypeRate ? string.Join(" ", @this.PositionTypesWherePositionTypeRate?.Select(v => v.Title ?? string.Empty).ToArray()) : null,
                    @this.ExistPositionTypesWherePositionTypeRate ? string.Join(" ", @this.PositionTypesWherePositionTypeRate?.SelectMany(v => v.Responsibilities?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
