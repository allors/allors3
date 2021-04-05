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
    using Database.Derivations;

    public class PartCategoryRule : Rule
    {
        public PartCategoryRule(MetaPopulation m) : base(m, new Guid("C2B0DDB7-9410-4E4F-9581-D668A84B3627")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PartCategory, m.PartCategory.Name),
                new RolePattern(m.PartCategory, m.PartCategory.PrimaryParent),
                new RolePattern(m.PartCategory, m.PartCategory.SecondaryParents),
                new RolePattern(m.PartCategory, m.PartCategory.CategoryImage),
                new RolePattern(m.PartCategory, m.PartCategory.Parts),
                new RolePattern(m.LocalisedText, m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.PartCategoryWhereLocalisedName } },
                new RolePattern(m.LocalisedText, m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.PartCategoryWhereLocalisedDescription } },
                new AssociationPattern(m.PartCategory.PrimaryParent),
                new RolePattern(m.PartCategory, m.PartCategory.PrimaryParent) {Steps = new IPropertyType[] {m.PartCategory.PartCategoriesWhereDescendant } },
                new AssociationPattern(m.PartCategory.SecondaryParents),
                new RolePattern(m.PartCategory, m.PartCategory.SecondaryParents) {Steps = new IPropertyType[] {m.PartCategory.PartCategoriesWhereDescendant } },
                new RolePattern(m.PartCategory, m.PartCategory.Parts) {Steps = new IPropertyType[] {m.PartCategory.PartCategoriesWhereDescendant } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartCategory>())
            {
                var defaultLocale = @this.Strategy.Transaction.GetSingleton().DefaultLocale;

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (!@this.ExistCategoryImage)
                {
                    @this.CategoryImage = @this.Strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
                }

                {
                    var primaryAncestors = new List<PartCategory>();
                    var primaryAncestor = @this.PrimaryParent;

                    while (primaryAncestor != null)
                    {
                        if (primaryAncestors.Contains(primaryAncestor))
                        {
                            var cyclic = string.Join(" -> ", primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                            validation.AddError($"{@this} {@this.Meta.PrimaryParent} Cycle detected in " + cyclic);
                            break;
                        }

                        primaryAncestors.Add(primaryAncestor);
                        primaryAncestor = primaryAncestor.PrimaryParent;
                    }

                    @this.PrimaryAncestors = primaryAncestors.ToArray();
                }

                @this.Children = @this.PartCategoriesWherePrimaryParent.Union(@this.PartCategoriesWhereSecondaryParent).ToArray();
                {
                    var descendants = new List<PartCategory>();
                    var children = @this.Children.ToArray();
                    while (children.Length > 0)
                    {
                        if (children.Any(v => descendants.Contains(v)))
                        {
                            var cyclic = string.Join(" -> ", descendants.Union(children).Select(v => v.Name));
                            validation.AddError($"{@this} {@this.Meta.Children} Cycle detected in " + cyclic);
                            break;
                        }

                        descendants.AddRange(children);
                        children = children.SelectMany(v => v.Children).ToArray();
                    }

                    @this.Descendants = descendants.ToArray();
                }

                var descendantsAndSelf = @this.Descendants.Append(@this).ToArray();
            }
        }
    }
}
