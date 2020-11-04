// <copyright file="ChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using Meta;

    public sealed class ChangeSet : IChangeSet
    {
        internal ChangeSet(Session session, WorkspaceChangeSet workspaceChangeSet)
        {
            this.Session = session;
        }

        public ISession Session { get; }

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Deleted { get; }

        public ISet<long> Associations { get; }

        public ISet<long> Roles { get; }

        public IDictionary<long, ISet<IRoleType>> RoleTypesByAssociation { get; }

        public IDictionary<long, ISet<IAssociationType>> AssociationTypesByRole { get; }

        public IDictionary<IRoleType, ISet<long>> AssociationsByRoleType { get; }

        public IDictionary<IAssociationType, ISet<long>> RolesByAssociationType { get; }
    }
}
