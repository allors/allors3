// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using Meta;

    public class SessionStateChangeSet
    {
        public SessionStateChangeSet(IDictionary<IRoleType, IDictionary<long, object>> roleByAssociationByRoleType, IDictionary<IAssociationType, IDictionary<long, object>> associationByRoleByAssociationType)
        {
            this.RoleByAssociationByRoleType = roleByAssociationByRoleType;
            this.AssociationByRoleByRoleType = associationByRoleByAssociationType;
        }

        public IDictionary<IRoleType, IDictionary<long, object>> RoleByAssociationByRoleType { get; }

        public IDictionary<IAssociationType, IDictionary<long, object>> AssociationByRoleByRoleType { get; }
    }
}
