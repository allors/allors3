// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class StateChangeSet
    {
        private readonly Dictionary<IRoleType, Dictionary<Identity, object>> roleByAssociationByRoleType;
        private readonly Dictionary<IAssociationType, Dictionary<Identity, object>> associationByRoleByRoleType;

        public StateChangeSet(Dictionary<IRoleType, Dictionary<Identity, object>> roleByAssociationByRoleType, Dictionary<IAssociationType, Dictionary<Identity, object>> associationByRoleByAssociationType)
        {
            this.roleByAssociationByRoleType = roleByAssociationByRoleType;
            this.associationByRoleByRoleType = associationByRoleByAssociationType;
        }

        public bool HasChanges =>
            this.roleByAssociationByRoleType.Any(v => v.Value.Count > 0) ||
            this.associationByRoleByRoleType.Any(v => v.Value.Count > 0);

        public Dictionary<Identity, object> ChangedRoles(IRoleType roleType)
        {
            this.roleByAssociationByRoleType.TryGetValue(roleType, out var changedRelations);
            return changedRelations;
        }
    }
}
