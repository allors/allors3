// <copyright file="PartProductTypeNameRule.cs" company="Allors bv">
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

    public class PartProductTypeNameRule : Rule
    {
        public PartProductTypeNameRule(MetaPopulation m) : base(m, new Guid("ebb39ed9-2df9-440f-9b6d-7721bf887b23")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.ProductType),
                m.ProductType.RolePattern(v => v.Name, v => v.PartsWhereProductType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartProductTypeName(validation);
            }
        }
    }

    public static class PartProductTypeNameRuleExtensions
    {
        public static void DerivePartProductTypeName(this Part @this, IValidation validation) => @this.ProductTypeName = @this.ProductType?.Name;
    }
}
