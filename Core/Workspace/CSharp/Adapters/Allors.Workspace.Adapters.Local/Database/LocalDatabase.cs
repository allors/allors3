// <copyright file="v.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Database;
    using Meta;
    using IMetaPopulation = Meta.IMetaPopulation;
    using IRoleType = Database.Meta.IRoleType;

    internal class LocalDatabase
    {
        private readonly Dictionary<IClass, Dictionary<IOperandType, LocalPermission>> readPermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, LocalPermission>> writePermissionByOperandTypeByClass;
        private readonly Dictionary<IClass, Dictionary<IOperandType, LocalPermission>> executePermissionByOperandTypeByClass;

        internal LocalDatabase(IMetaPopulation metaPopulation, Identities identities)
        {
            this.MetaPopulation = metaPopulation;
            this.ObjectsById = new ConcurrentDictionary<long, LocalDatabaseObject>();
            this.Identities = identities;
        }

        public IMetaPopulation MetaPopulation { get; }

        public ConcurrentDictionary<long, LocalDatabaseObject> ObjectsById { get; }

        internal Identities Identities { get; }

        public void Sync(LocalPullResult pullResult)
        {
            var objects = pullResult.Objects.Where(v =>
            {
                if (this.ObjectsById.TryGetValue(v.Id, out var databaseRoles))
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
                var roleByRoleType = roleTypes.ToDictionary(w =>
                        ((Meta.IRelationType)this.MetaPopulation.Find(w.RelationType.Id)).RoleType,
                    w => GetRole(v, w));

                this.ObjectsById[id] = new LocalDatabaseObject(this, id, workspaceClass, v.Strategy.ObjectVersion, roleByRoleType, null, null);
            }
        }

        internal LocalDatabaseObject Get(long identity)
        {
            this.ObjectsById.TryGetValue(identity, out var databaseObjects);
            return databaseObjects;
        }

        internal LocalPermission GetPermission(IClass @class, IOperandType operandType, Operations operation)
        {
            switch (operation)
            {
                case Operations.Read:
                    if (this.readPermissionByOperandTypeByClass.TryGetValue(@class, out var readPermissionByOperandType))
                    {
                        if (readPermissionByOperandType.TryGetValue(operandType, out var readPermission))
                        {
                            return readPermission;
                        }
                    }

                    return null;

                case Operations.Write:
                    if (this.writePermissionByOperandTypeByClass.TryGetValue(@class, out var writePermissionByOperandType))
                    {
                        if (writePermissionByOperandType.TryGetValue(operandType, out var writePermission))
                        {
                            return writePermission;
                        }
                    }

                    return null;

                default:
                    if (this.executePermissionByOperandTypeByClass.TryGetValue(@class, out var executePermissionByOperandType))
                    {
                        if (executePermissionByOperandType.TryGetValue(operandType, out var executePermission))
                        {
                            return executePermission;
                        }
                    }

                    return null;
            }
        }
    }
}
