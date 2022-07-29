// <copyright file="IPreparedSelects.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Data;
    using Database;
    using Domain;
    using Meta;
    using Services;

    public partial class PrefetchPolicyCache : IPrefetchPolicyCache
    {
        private readonly MetaPopulation m;
        private readonly IDictionary<string, IDictionary<IClass, PrefetchPolicy>> prefetchPolicyByClassByWorkspace;

        public PrefetchPolicyCache(IDatabase database, IMetaCache metaCache)
        {
            this.m = database.Services.Get<MetaPopulation>();

            this.PermissionsWithClass = new PrefetchPolicyBuilder()
                    .WithRule(this.m.Permission.ClassPointer)
                    .Build();

            this.Security = new PrefetchPolicyBuilder().WithSecurityRules(this.m).Build();

            this.prefetchPolicyByClassByWorkspace = new Dictionary<string, IDictionary<IClass, PrefetchPolicy>>();
            foreach (var workspaceName in this.m.WorkspaceNames)
            {
                var roleTypesByClass = metaCache.GetWorkspaceRoleTypesByClass(workspaceName);

                var prefetchPolicyByClass = new Dictionary<IClass, PrefetchPolicy>();
                foreach (var @class in metaCache.GetWorkspaceClasses(workspaceName))
                {
                    var prefetchPolicyBuilder = new PrefetchPolicyBuilder();
                    prefetchPolicyBuilder.WithWorkspaceRules(this.m, roleTypesByClass[@class]);
                    var prefetchPolicy = prefetchPolicyBuilder.Build();
                    prefetchPolicyByClass[@class] = prefetchPolicy;
                }

                this.prefetchPolicyByClassByWorkspace[workspaceName] = prefetchPolicyByClass;
            }
        }

        public PrefetchPolicy PermissionsWithClass { get; }

        public PrefetchPolicy Security { get; }

        public IDictionary<IClass, PrefetchPolicy> WorkspacePrefetchPolicyByClass(string workspaceName) => this.prefetchPolicyByClassByWorkspace[workspaceName];

        public PrefetchPolicy ForNodes(Node[] nodes)
        {
            var builder = new PrefetchPolicyBuilder();
            builder.WithSecurityRules(this.m);
            foreach (var node in nodes)
            {
                var propertyType = node.PropertyType;
                if (propertyType.ObjectType.IsComposite)
                {
                    builder.WithRule(propertyType, this.Security);
                }
                else
                {
                    builder.WithRule(propertyType);
                }
            }

            return builder.Build();
        }

        public PrefetchPolicy ForDependency(IComposite composite, ISet<IPropertyType> propertyTypes)
        {
            var builder = new PrefetchPolicyBuilder();
            builder.WithSecurityRules(this.m);
            foreach (var propertyType in propertyTypes)
            {
                if (propertyType.ObjectType.IsComposite)
                {
                    builder.WithRule(propertyType, this.Security);
                }
                else
                {
                    builder.WithRule(propertyType);
                }
            }

            return builder.Build();
        }
    }
}
