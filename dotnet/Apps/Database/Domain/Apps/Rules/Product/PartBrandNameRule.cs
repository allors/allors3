// <copyright file="PartBrandNameRule.cs" company="Allors bvba">
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

    public class PartBrandNameRule : Rule
    {
        public PartBrandNameRule(MetaPopulation m) : base(m, new Guid("5f12393e-d462-4fa9-8da6-0a48d1a1e7a0")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.Brand),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartBrandName(validation);
            }
        }
    }

    public static class PartBrandNameRuleExtensions
    {
        public static void DerivePartBrandName(this Part @this, IValidation validation) => @this.BrandName = @this.Brand?.Name;
    }
}
