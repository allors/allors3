// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace.Meta;

    public class WorkspaceChangeSet
    {
        private readonly Dictionary<IRoleType, Dictionary<WorkspaceObject, object>> roleByAssociationByRoleType;
        private readonly Dictionary<IAssociationType, Dictionary<WorkspaceObject, object>> associationByRoleByRoleType;

        public WorkspaceChangeSet(IMetaPopulation metaPopulation, Dictionary<IRoleType, Dictionary<WorkspaceObject, object>> roleByAssociationByRoleType, Dictionary<IAssociationType, Dictionary<WorkspaceObject, object>> associationByRoleByAssociationType)
        {
            this.MetaPopulation = metaPopulation;
            this.roleByAssociationByRoleType = roleByAssociationByRoleType;
            this.associationByRoleByRoleType = associationByRoleByAssociationType;
        }

        public IMetaPopulation MetaPopulation { get; }

        public bool HasChanges =>
            this.roleByAssociationByRoleType.Any(v => v.Value.Count > 0) ||
            this.associationByRoleByRoleType.Any(v => v.Value.Count > 0);

        //public Dictionary<WorkspaceObject, object> ChangedRoles<TRole>(string name)
        //{
        //    IObjectType objectType = this.Meta.ObjectTypeByType[typeof(TRole)];
        //    IRoleType roleType = objectType.RoleTypeByName[name];
        //    return this.ChangedRoles(roleType);
        //}

        public Dictionary<WorkspaceObject, object> ChangedRoles(IRoleType roleType)
        {
            this.roleByAssociationByRoleType.TryGetValue(roleType, out var changedRelations);
            return changedRelations;
        }
    }
}
