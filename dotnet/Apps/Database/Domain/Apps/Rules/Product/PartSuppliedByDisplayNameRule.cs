// <copyright file="PartSuppliedByDisplayNameRule.cs" company="Allors bv">
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

    public class PartSuppliedByDisplayNameRule : Rule
    {
        public PartSuppliedByDisplayNameRule(MetaPopulation m) : base(m, new Guid("5ea5b027-c838-4aea-b71a-6ef631bee2ad")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.SuppliedBy),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PartsWhereSuppliedBy),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartSuppliedByDisplayName(validation);
            }
        }
    }

    public static class PartSuppliedByDisplayNameRuleExtensions
    {
        public static void DerivePartSuppliedByDisplayName(this Part @this, IValidation validation)
        {
            var array = new string[] {
                    string.Join(", ", @this.SuppliedBy?.Select((v) => v.DisplayName?? string.Empty).ToArray())
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SuppliedByDisplayName = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveDisplayName();
            }
        }
    }
}
