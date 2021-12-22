// <copyright file="UnifiedProductProductIdentificationNamesRule.cs" company="Allors bvba">
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

    public class UnifiedProductProductIdentificationNamesRule : Rule
    {
        public UnifiedProductProductIdentificationNamesRule(MetaPopulation m) : base(m, new Guid("3b8b3d58-a1fb-4ce0-ad3e-51723851039f")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedProduct.RolePattern(v => v.ProductIdentifications),
                m.ProductIdentification.RolePattern(v => v.Identification, v => v.UnifiedProductWhereProductIdentification),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<UnifiedProduct>())
            {
                var array = new string[] {
                    string.Join(", ", @this.ProductIdentifications?.Select((v) => v.Identification ?? string.Empty).ToArray())
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.ProductIdentificationNames = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}
