// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Database;
    using Meta;
    using IMetaPopulation = Meta.IMetaPopulation;
    using IRoleType = Database.Meta.IRoleType;

    internal class DatabaseStore
    {
        internal DatabaseStore(IMetaPopulation metaPopulation)
        {
            this.MetaPopulation = metaPopulation;
            this.DatabaseRolesById = new ConcurrentDictionary<long, DatabaseRoles>();
        }

        public IMetaPopulation MetaPopulation { get; }

        public ConcurrentDictionary<long, DatabaseRoles> DatabaseRolesById { get; }

        public void Sync(PullResult pullResult)
        {
            var objects = pullResult.Objects.Where(v =>
            {
                if (this.DatabaseRolesById.TryGetValue(v.Id, out var databaseRoles))
                {
                    return v.Strategy.ObjectVersion != databaseRoles.Version;
                }

                return true;
            });

            // TODO: Prefetch objects


            static object GetRole(IObject @object, IRoleType roleType)
            {
                if (roleType.ObjectType.IsUnit)
                {
                    return @object.Strategy.GetUnitRole(roleType);
                }
                else if (roleType.IsOne)
                {
                    return @object.Strategy.GetCompositeRole(roleType)?.Id;
                }
                else
                {
                    var roles = @object.Strategy.GetCompositeRoles(roleType);
                    if (roles.Count > 0)
                    {
                        return @object.Strategy.GetCompositeRoles(roleType).Select(v => v.Id).ToArray();
                    }
                }

                return Array.Empty<long>();
            }

            foreach (var v in objects)
            {
                var id = v.Id;
                var databaseClass = v.Strategy.Class;
                var roleTypes = databaseClass.DatabaseRoleTypes.Where(w => w.RelationType.WorkspaceNames.Length > 0);

                var workspaceClass = (IClass)this.MetaPopulation.Find(databaseClass.Id);

                this.DatabaseRolesById[id] = new DatabaseRoles
                {
                    Id = id,
                    Class = workspaceClass,
                    Version = v.Strategy.ObjectVersion,
                    RoleByRoleType = roleTypes.ToDictionary(w =>
                            ((Meta.IRelationType)this.MetaPopulation.Find(w.RelationType.Id)).RoleType,
                        w => GetRole(v, w)),
                };
            }
        }
    }
}
