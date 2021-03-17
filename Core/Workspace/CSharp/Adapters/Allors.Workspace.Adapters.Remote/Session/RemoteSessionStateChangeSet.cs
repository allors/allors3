// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using Meta;

    public class RemoteSessionStateChangeSet
    {
        public RemoteSessionStateChangeSet(IDictionary<IRoleType, IDictionary<RemoteStrategy, object>> roleByAssociationByRoleType, IDictionary<IAssociationType, IDictionary<RemoteStrategy, object>> associationByRoleByAssociationType)
        {
            this.RoleByAssociationByRoleType = roleByAssociationByRoleType;
            this.AssociationByRoleByRoleType = associationByRoleByAssociationType;
        }

        public IDictionary<IRoleType, IDictionary<RemoteStrategy, object>> RoleByAssociationByRoleType { get; }

        public IDictionary<IAssociationType, IDictionary<RemoteStrategy, object>> AssociationByRoleByRoleType { get; }
    }
}
