// <copyright file="PartManufacturedByDisplayNameRule.cs" company="Allors bvba">
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

    public class PartManufacturedByDisplayNameRule : Rule
    {
        public PartManufacturedByDisplayNameRule(MetaPopulation m) : base(m, new Guid("40fa7536-8373-488f-bda9-54580e162fc0")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.ManufacturedBy),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PartsWhereManufacturedBy),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartManufacturedByDisplayName(validation);
            }
        }
    }
    public static class PartManufacturedByDisplayNameRuleExtensions
    {
        public static void DerivePartManufacturedByDisplayName(this Part @this, IValidation validation) => @this.ManufacturedByDisplayName = @this.ManufacturedBy?.DisplayName;
    }
}
