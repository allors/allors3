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
        private readonly IDictionary<IClass, Type> builderTypeByClass;
        private readonly IDictionary<string, ISet<IClass>> classesByWorkspaceName;
        private readonly IDictionary<string, IDictionary<IClass, ISet<IRoleType>>> roleTypesByClassByWorkspaceName;

        public MetaCache(IDatabase database)
        {
            var metaPopulation = (MetaPopulation)database.MetaPopulation;
            var assembly = database.ObjectFactory.Assembly;

            this.builderTypeByClass = metaPopulation.DatabaseClasses.
                ToDictionary(
                    v => (IClass)v,
                    v => assembly.GetType($"Allors.Database.Domain.{v.Name}Builder", false));

            this.classesByWorkspaceName = new Dictionary<string, ISet<IClass>>();
            this.roleTypesByClassByWorkspaceName = new Dictionary<string, IDictionary<IClass, ISet<IRoleType>>>();

            foreach (var workspaceName in metaPopulation.WorkspaceNames)
            {
                ISet<IClass> classes = new HashSet<IClass>(metaPopulation.Classes.Where(w => w.WorkspaceNames.Contains(workspaceName)));
                this.classesByWorkspaceName[workspaceName] = classes;

                var roleTypesByClass = new Dictionary<IClass, ISet<IRoleType>>();
                foreach (var @class in classes)
                {
                    var roleTypes = new HashSet<IRoleType>(@class.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Contains(workspaceName)));
                    roleTypesByClass[@class] = roleTypes;
                }

                this.roleTypesByClassByWorkspaceName[workspaceName] = roleTypesByClass;
            }
        }

        public Type GetBuilderType(IClass @class) => this.builderTypeByClass[@class];

        public ISet<IClass> GetWorkspaceClasses(string workspaceName)
        {
            this.classesByWorkspaceName.TryGetValue(workspaceName, out var classes);
            return classes;
        }

        public IDictionary<IClass, ISet<IRoleType>> GetWorkspaceRoleTypesByClass(string workspaceName)
        {
            this.roleTypesByClassByWorkspaceName.TryGetValue(workspaceName, out var rolesByClass);
            return rolesByClass;
        }
    }
}
