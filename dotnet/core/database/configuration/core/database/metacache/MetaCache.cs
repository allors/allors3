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
        private readonly IReadOnlyDictionary<IClass, IRoleType[]> requiredRoleTypesByClass;
        private readonly IReadOnlyDictionary<IClass, IRoleType[]> uniqueRoleTypesByClass;
        private readonly IReadOnlyDictionary<IClass, Type> builderTypeByClass;
        private readonly IReadOnlyDictionary<string, HashSet<IClass>> workspaceClassesByWorkspaceName;

        public MetaCache(IDatabase database)
        {
            var metaPopulation = (MetaPopulation)database.MetaPopulation;
            var assembly = database.ObjectFactory.Assembly;

            this.requiredRoleTypesByClass = metaPopulation.DatabaseClasses
                .ToDictionary(
                    v => (IClass)v,
                    v => v.DatabaseRoleTypes
                          .Where(concreteRoleType => concreteRoleType.IsRequired)
                          .ToArray());


            this.uniqueRoleTypesByClass = metaPopulation.DatabaseClasses
                .ToDictionary(
                    v => (IClass)v,
                    v => v.DatabaseRoleTypes
                        .Where(concreteRoleType => concreteRoleType.IsUnique)
                        .ToArray());

            this.builderTypeByClass = metaPopulation.DatabaseClasses.
                ToDictionary(
                    v => (IClass)v,
                    v => assembly.GetType($"Allors.Database.Domain.{v.Name}Builder", false));

            this.workspaceClassesByWorkspaceName = metaPopulation.WorkspaceNames
                .ToDictionary(v => v, v => new HashSet<IClass>(metaPopulation.Classes.Where(w => w.WorkspaceNames.Contains(v))));
        }

        public IRoleType[] GetRequiredRoleTypes(IClass @class) => this.requiredRoleTypesByClass[@class];

        public IRoleType[] GetUniqueRoleTypes(IClass @class) => this.uniqueRoleTypesByClass[@class];

        public Type GetBuilderType(IClass @class) => this.builderTypeByClass[@class];

        public ISet<IClass> GetWorkspaceClasses(string workspaceName)
        {
            this.workspaceClassesByWorkspaceName.TryGetValue(workspaceName, out var classes);
            return classes;
        }
    }
}
