// <copyright file="MetaCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Services;

    public class MetaCache : IMetaCache
    {
        private readonly MetaPopulation metaPopulation;
        private readonly IDictionary<IClass, Type> builderTypeByClass;
        private readonly IDictionary<string, ISet<IClass>> classesByWorkspaceName;
        private readonly IDictionary<string, IDictionary<IClass, ISet<IRoleType>>> roleTypesByClassByWorkspaceName;
        private readonly ConcurrentDictionary<string, string> fingerprintByWorkspaceName;

        public MetaCache(IDatabase database)
        {
            this.metaPopulation = (MetaPopulation)database.MetaPopulation;
            var assembly = database.ObjectFactory.Assembly;

            this.builderTypeByClass = this.metaPopulation.DatabaseClasses.
                ToDictionary(
                    v => (IClass)v,
                    v => assembly.GetType($"Allors.Database.Domain.{v.Name}Builder", false));

            this.classesByWorkspaceName = new Dictionary<string, ISet<IClass>>();
            this.roleTypesByClassByWorkspaceName = new Dictionary<string, IDictionary<IClass, ISet<IRoleType>>>();
            this.fingerprintByWorkspaceName = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

            foreach (var workspaceName in this.metaPopulation.WorkspaceNames)
            {
                ISet<IClass> classes = new HashSet<IClass>(this.metaPopulation.Classes.Where(w => w.WorkspaceNames.Contains(workspaceName)));
                this.classesByWorkspaceName[workspaceName] = classes;

                var roleTypesByClass = new Dictionary<IClass, ISet<IRoleType>>();
                foreach (var @class in classes)
                {
                    var roleTypes = new HashSet<IRoleType>(@class.DatabaseRoleTypes.Where(v => v.RelationType.WorkspaceNames.Contains(workspaceName)));
                    roleTypesByClass[@class] = roleTypes;
                }

                this.roleTypesByClassByWorkspaceName[workspaceName] = roleTypesByClass;
                this.fingerprintByWorkspaceName[workspaceName] = this.ComputeFingerprint(workspaceName);
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

        public string GetWorkspaceFingerprint(string workspaceName) =>
            workspaceName == null ? null : this.fingerprintByWorkspaceName.GetOrAdd(workspaceName, this.ComputeFingerprint);

        // The same members that the generator puts in the workspace meta of this workspace: the
        // composites, relation types and method types whose workspace names include it.
        private string ComputeFingerprint(string workspaceName) =>
            MetaFingerprint.Compute(this.metaPopulation.Composites.Where(v => v.WorkspaceNames.Contains(workspaceName)).Select(v => v.Tag)
                .Concat(this.metaPopulation.RelationTypes.Where(v => v.WorkspaceNames.Contains(workspaceName)).Select(v => v.Tag))
                .Concat(this.metaPopulation.MethodTypes.Where(v => v.WorkspaceNames.Contains(workspaceName)).Select(v => v.Tag)));
    }
}
