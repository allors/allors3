// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Workspace.Meta;

    public class StateChangeSet
    {
        private readonly Dictionary<IRoleType, Dictionary<long, object>> roleByAssociationByRoleType;
        private readonly Dictionary<IAssociationType, Dictionary<long, object>> associationByRoleByRoleType;

        public StateChangeSet(Dictionary<IRoleType, Dictionary<long, object>> roleByAssociationByRoleType, Dictionary<IAssociationType, Dictionary<long, object>> associationByRoleByAssociationType)
        {
            this.roleByAssociationByRoleType = roleByAssociationByRoleType;
            this.associationByRoleByRoleType = associationByRoleByAssociationType;
        }

        public bool HasChanges =>
            this.roleByAssociationByRoleType.Any(v => v.Value.Count > 0) ||
            this.associationByRoleByRoleType.Any(v => v.Value.Count > 0);

        //public Dictionary<long?, object> ChangedRoles<TRole>(string name)
        //{
        //    IObjectType objectType = this.Meta.ObjectTypeByType[typeof(TRole)];
        //    IRoleType roleType = objectType.RoleTypeByName[name];
        //    return this.ChangedRoles(roleType);
        //}

        public Dictionary<long, object> ChangedRoles(IRoleType roleType)
        {
            this.roleByAssociationByRoleType.TryGetValue(roleType, out var changedRelations);
            return changedRelations;
        }
    }
}
