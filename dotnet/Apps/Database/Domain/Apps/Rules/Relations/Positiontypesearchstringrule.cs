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

    public class PositionTypeSearchStringRule : Rule
    {
        public PositionTypeSearchStringRule(MetaPopulation m) : base(m, new Guid("d2b70cb5-8699-48a6-b212-5d195167981b")) =>
            this.Patterns = new Pattern[]
            {
                m.PositionType.RolePattern(v => v.Description),
                m.PositionType.RolePattern(v => v.Title),
                m.PositionType.RolePattern(v => v.PositionTypeRate),
                m.PositionTypeRate.RolePattern(v => v.SearchString, v => v.PositionTypesWherePositionTypeRate.ObjectType),
                m.PositionType.RolePattern(v => v.Responsibilities),
                m.Responsibility.RolePattern(v => v.Description, v => v.PositionTypesWhereResponsibility.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PositionType>())
            {
                @this.DerivePositionTypeSearchString(validation);
            }
        }
    }

    public static class PositionTypeSearchStringRuleExtensions
    {
        public static void DerivePositionTypeSearchString(this PositionType @this, IValidation validation)
        {
            var array = new string[] {
                    @this.Description,
                    @this.Title,
                    @this.ExistResponsibilities ? string.Join(" ", @this.Responsibilities?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.PositionTypeRate?.SearchString,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
