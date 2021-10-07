// <copyright file="DependencySet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Meta;
    using Services;

    public class Dependencies : IDependencies
    {
        private readonly Dictionary<IComposite, IPropertyType[]> dependenciesByComposite;

        internal Dependencies() => this.dependenciesByComposite = new Dictionary<IComposite, IPropertyType[]>();

        public IPropertyType[] GetDependencies(IComposite composite)
        {
            this.dependenciesByComposite.TryGetValue(composite, out var dependencies);
            return dependencies ?? Array.Empty<IPropertyType>();
        }

        public void Add(IComposite composite, IPropertyType propertyType)
        {
            void Add(IComposite type)
            {
                if (!this.dependenciesByComposite.TryGetValue(type, out var dependencies))
                {
                    dependencies = new[] { propertyType };
                    this.dependenciesByComposite.Add(type, dependencies);
                }
                else if (!dependencies.Contains(propertyType))
                {
                    dependencies = dependencies.Append(propertyType).ToArray();
                    this.dependenciesByComposite[type] = dependencies;
                }
            }

            Add(composite);
            foreach (var superType in composite.Supertypes.Where(v => v.Origin == Origin.Database))
            {
                Add(superType);
            }

            foreach (var subType in composite.Subtypes.Where(v => v.Origin == Origin.Database))
            {
                Add(subType);
            }

        }
    }
}
