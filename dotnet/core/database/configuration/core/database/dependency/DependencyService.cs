// <copyright file="DependencyService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Domain;
    using Services;

    public class DependencyService : IDependencyService
    {
        private readonly Dictionary<string, DependencySet> dependencySetById;

        public DependencyService() => this.dependencySetById = new Dictionary<string, DependencySet>();

        public IDependencySet GetDependencySet(string id)
        {
            if (this.dependencySetById.TryGetValue(id, out var dependencySet))
            {
                return dependencySet;
            }

            dependencySet = new DependencySet();
            this.dependencySetById.Add(id, dependencySet);
            return dependencySet;
        }
    }
}
