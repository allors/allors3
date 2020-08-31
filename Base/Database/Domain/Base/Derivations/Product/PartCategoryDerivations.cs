// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PartCategoryCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdPartCategories = changeSet.Created.Select(v=>v.GetObject()).OfType<PartCategory>();

                foreach(var partCategory in createdPartCategories)
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
                                var cycle = string.Join(" -> ", primaryAncestors.Append(primaryAncestor).Select(v => v.Name));
                                validation.AddError($"{partCategory} {partCategory.Meta.PrimaryParent} Cycle detected in " + cycle);
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
                                var cycle = string.Join(" -> ", descendants.Union(children).Select(v => v.Name));
                                validation.AddError($"{partCategory} {partCategory.Meta.Children} Cycle detected in " + cycle);
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

        public static void PartCategoryRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("cb377da1-1df2-4dfd-9dfb-66048e0865b6")] = new PartCategoryCreationDerivation();
        }
    }
}
