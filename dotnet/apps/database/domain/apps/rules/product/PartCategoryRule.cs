// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class PartCategoryRule : Rule
    {
        public PartCategoryRule(MetaPopulation m) : base(m, new Guid("C2B0DDB7-9410-4E4F-9581-D668A84B3627")) =>
            this.Patterns = new Pattern[]
            {
                m.PartCategory.RolePattern(v => v.PrimaryParent),
                m.PartCategory.RolePattern(v => v.SecondaryParents),
                m.PartCategory.RolePattern(v => v.Parts),
                m.PartCategory.RolePattern(v => v.PrimaryParent, v => v.PartCategoriesWhereDescendant),
                m.PartCategory.RolePattern(v => v.SecondaryParents, v => v.PartCategoriesWhereDescendant),
                m.PartCategory.RolePattern(v => v.Parts, v => v.PartCategoriesWhereDescendant),
                m.PartCategory.AssociationPattern(v => v.PartCategoriesWhereSecondaryParent),
                m.PartCategory.AssociationPattern(v => v.PartCategoriesWherePrimaryParent),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartCategory>())
            {
                var primaryAncestors = new List<PartCategory>();
                var primaryAncestor = @this.PrimaryParent;

                while (primaryAncestor != null)
                {
                    if (primaryAncestors.Contains(primaryAncestor))
                    {
                        var cyclic = string.Join(" -> ", primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                        // TODO: Move text to Resources
                        validation.AddError(@this, @this.Meta.PrimaryParent, "Cycle detected in " + cyclic);
                        break;
                    }

                    primaryAncestors.Add(primaryAncestor);
                    primaryAncestor = primaryAncestor.PrimaryParent;
                }

                @this.PrimaryAncestors = primaryAncestors.ToArray();

                @this.Children = @this.PartCategoriesWherePrimaryParent.Union(@this.PartCategoriesWhereSecondaryParent).ToArray();

                var descendants = new List<PartCategory>();
                var children = @this.Children.ToArray();
                while (children.Length > 0)
                {
                    if (children.Any(v => descendants.Contains(v)))
                    {
                        var cyclic = string.Join(" -> ", descendants.Union(children).Select(v => v.Name));
                        // TODO: Move text to Resources
                        validation.AddError(@this, @this.Meta.Children, "Cycle detected in " + cyclic);
                        break;
                    }

                    descendants.AddRange(children);
                    children = children.SelectMany(v => v.Children).ToArray();
                }

                @this.Descendants = descendants.ToArray();

                var descendantsAndSelf = @this.Descendants.Append(@this).ToArray();
            }
        }
    }
}
