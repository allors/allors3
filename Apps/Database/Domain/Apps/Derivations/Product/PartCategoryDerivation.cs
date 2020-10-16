// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PartCategoryDerivation : DomainDerivation
    {
        public PartCategoryDerivation(M m) : base(m, new Guid("C2B0DDB7-9410-4E4F-9581-D668A84B3627")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.PartCategory.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var partCategory in matches.Cast<PartCategory>())
            {
                var defaultLocale = partCategory.Strategy.Session.GetSingleton().DefaultLocale;

                if (partCategory.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    partCategory.Name = partCategory.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (partCategory.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    partCategory.Description = partCategory.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (!partCategory.ExistCategoryImage)
                {
                    partCategory.CategoryImage = partCategory.Strategy.Session.GetSingleton().Settings.NoImageAvailableImage;
                }

                {
                    var primaryAncestors = new List<PartCategory>();
                    var primaryAncestor = partCategory.PrimaryParent;

                    while (primaryAncestor != null)
                    {
                        if (primaryAncestors.Contains(primaryAncestor))
                        {
                            var cyclic = string.Join(" -> ", primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                            validation.AddError($"{partCategory} {partCategory.Meta.PrimaryParent} Cycle detected in " + cyclic);
                            break;
                        }

                        primaryAncestors.Add(primaryAncestor);
                        primaryAncestor = primaryAncestor.PrimaryParent;
                    }

                    partCategory.PrimaryAncestors = primaryAncestors.ToArray();
                }

                partCategory.Children = partCategory.PartCategoriesWherePrimaryParent.Union(partCategory.PartCategoriesWhereSecondaryParent).ToArray();
                {
                    var descendants = new List<PartCategory>();
                    var children = partCategory.Children.ToArray();
                    while (children.Length > 0)
                    {
                        if (children.Any(v => descendants.Contains(v)))
                        {
                            var cyclic = string.Join(" -> ", descendants.Union(children).Select(v => v.Name));
                            validation.AddError($"{partCategory} {partCategory.Meta.Children} Cycle detected in " + cyclic);
                            break;
                        }

                        descendants.AddRange(children);
                        children = children.SelectMany(v => v.Children).ToArray();
                    }

                    partCategory.Descendants = descendants.ToArray();
                }

                var descendantsAndSelf = partCategory.Descendants.Append(partCategory).ToArray();
            }
        }
    }
}
