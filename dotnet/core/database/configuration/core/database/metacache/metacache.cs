// <copyright file="MetaCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Services;

    public class MetaCache : IMetaCache
    {
        private readonly IReadOnlyDictionary<IClass, Type> builderTypeByClass;
        private readonly IReadOnlyDictionary<string, HashSet<IClass>> classesByWorkspaceName;
        private readonly IReadOnlyDictionary<string, HashSet<long>> permissionIdsByWorkspaceName;

        public MetaCache(IDatabase database)
        {
            var metaPopulation = (MetaPopulation)database.MetaPopulation;
            var assembly = database.ObjectFactory.Assembly;

            this.builderTypeByClass = metaPopulation.DatabaseClasses.
                ToDictionary(
                    v => (IClass)v,
                    v => assembly.GetType($"Allors.Database.Domain.{v.Name}Builder", false));

            this.classesByWorkspaceName = new Dictionary<string, HashSet<IClass>>();
            this.permissionIdsByWorkspaceName = new Dictionary<string, HashSet<long>>();

            this.classesByWorkspaceName = metaPopulation.WorkspaceNames
                .ToDictionary(v => v, v => new HashSet<IClass>(metaPopulation.Classes.Where(w => w.WorkspaceNames.Contains(v))));

        }

        public Type GetBuilderType(IClass @class) => this.builderTypeByClass[@class];

        public ISet<IClass> GetWorkspaceClasses(string workspaceName)
        {
            this.classesByWorkspaceName.TryGetValue(workspaceName, out var classes);
            return classes;
        }

        public ISet<long> GetWorkspacePermissionIds(string workspaceName)
        {
            this.permissionIdsByWorkspaceName.TryGetValue(workspaceName, out var classes);
            return classes;
        }
    }
}
