// <copyright file="partpartcategoriesdisplaynamerule.cs" company="Allors bv">
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

    public class PartPartCategoriesDisplayNameRule : Rule
    {
        public PartPartCategoriesDisplayNameRule (MetaPopulation m) : base(m, new Guid("1f10de19-f736-4477-93e2-e6f3446cf09e")) =>
            this.Patterns = new Pattern[]
            {
                m.PartCategory.RolePattern(v => v.Name, v => v.Parts),
                m.PartCategory.RolePattern(v => v.PrimaryParent, v => v.Parts),
                m.PartCategory.RolePattern(v => v.Parts, v => v.Parts)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Part>())
            {
                @this.DerivePartPartCategoriesDisplayName(validation);
            }
        }
    }

    public static class PartPartCategoriesDisplayNameRuleExtensions
    {
        public static void DerivePartPartCategoriesDisplayName(this Part @this, IValidation validation)
        {
            foreach (var category in @this.PartCategoriesWherePart)
            {
                var primaryAncestors = new List<PartCategory>();
                var primaryAncestor = category.PrimaryParent;
                while (primaryAncestor != null)
                {
                    if (primaryAncestors.Contains(primaryAncestor))
                    {
                        var loop = string.Join(" -> ", primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                        validation.AddError(category, category.Meta.PrimaryParent, "Cycle detected in " + loop);
                        break;
                    }

                    primaryAncestors.Add(primaryAncestor);
                    primaryAncestor = primaryAncestor.PrimaryParent;
                }

                primaryAncestors.Reverse();
                primaryAncestors.Add(category);
                @this.PartCategoriesDisplayName = string.Join("/", primaryAncestors.Select(v => v.Name));
            }
        }
    }
}
