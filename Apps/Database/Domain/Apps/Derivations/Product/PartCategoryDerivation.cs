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

    public class PartCategoryDerivation : DomainDerivation
    {
        public PartCategoryDerivation(M m) : base(m, new Guid("C2B0DDB7-9410-4E4F-9581-D668A84B3627")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PartCategory.Name),
                new ChangedPattern(m.PartCategory.PrimaryParent),
                new ChangedPattern(m.PartCategory.SecondaryParents),
                new ChangedPattern(m.PartCategory.CategoryImage),
                new ChangedPattern(m.PartCategory.Parts),
                new ChangedPattern(m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.PartCategoryWhereLocalisedName } },
                new ChangedPattern(m.LocalisedText.Text) {Steps = new IPropertyType[] {m.LocalisedText.PartCategoryWhereLocalisedDescription } },
                new ChangedPattern(m.PartCategory.PrimaryParent) {Steps = new IPropertyType[] {m.PartCategory.PrimaryParent} },
                new ChangedPattern(m.PartCategory.PrimaryParent) {Steps = new IPropertyType[] {m.PartCategory.PartCategoriesWhereDescendant } },
                new ChangedPattern(m.PartCategory.SecondaryParents) {Steps = new IPropertyType[] {m.PartCategory.SecondaryParents} },
                new ChangedPattern(m.PartCategory.SecondaryParents) {Steps = new IPropertyType[] {m.PartCategory.PartCategoriesWhereDescendant } },
                new ChangedPattern(m.PartCategory.Parts) {Steps = new IPropertyType[] {m.PartCategory.PartCategoriesWhereDescendant } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PartCategory>())
            {
                var defaultLocale = @this.Strategy.Session.GetSingleton().DefaultLocale;

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
                    @this.CategoryImage = @this.Strategy.Session.GetSingleton().Settings.NoImageAvailableImage;
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
