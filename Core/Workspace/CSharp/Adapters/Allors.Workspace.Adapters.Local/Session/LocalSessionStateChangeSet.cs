// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using Meta;

    public class LocalSessionStateChangeSet
    {
        public LocalSessionStateChangeSet(IDictionary<IRoleType, IDictionary<LocalStrategy, object>> roleByAssociationByRoleType, IDictionary<IAssociationType, IDictionary<LocalStrategy, object>> associationByRoleByAssociationType)
        {
            this.RoleByAssociationByRoleType = roleByAssociationByRoleType;
            this.AssociationByRoleByRoleType = associationByRoleByAssociationType;
        }

        public IDictionary<IRoleType, IDictionary<LocalStrategy, object>> RoleByAssociationByRoleType { get; }

        public IDictionary<IAssociationType, IDictionary<LocalStrategy, object>> AssociationByRoleByRoleType { get; }
    }
}
