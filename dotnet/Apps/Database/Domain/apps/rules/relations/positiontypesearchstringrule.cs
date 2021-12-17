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

    public class PositionTypeSearchStringRule : Rule
    {
        public PositionTypeSearchStringRule(MetaPopulation m) : base(m, new Guid("d2b70cb5-8699-48a6-b212-5d195167981b")) =>
            this.Patterns = new Pattern[]
            {
                m.PositionType.RolePattern(v => v.Description),
                m.PositionType.RolePattern(v => v.Title),
                m.PositionType.RolePattern(v => v.PositionTypeRate),
                m.PositionTypeRate.RolePattern(v => v.SearchString, v => v.PositionTypesWherePositionTypeRate.PositionType),
                m.PositionType.RolePattern(v => v.Responsibilities),
                m.Responsibility.RolePattern(v => v.Description, v => v.PositionTypesWhereResponsibility.PositionType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PositionType>())
            {
                var array = new string[] {
                    @this.Description,
                    @this.Title,
                    string.Join(" ", @this.Responsibilities?.Select(v => v.Description)),
                    string.Join(" ", @this.PositionTypeRate?.SearchString),
                };

                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
