// <copyright file="RemoteSessionChangeSet.cs" company="Allors bvba">
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

    internal sealed class RemoteChangeSet : IChangeSet
    {
        public RemoteChangeSet(RemoteSession session, ISet<IStrategy> created, ISet<IStrategy> instantiated, SessionStateChangeSet sessionStateChangeSet)
        {
            this.Session = session;
            this.Created = created;
            this.Instantiated = instantiated;
            this.RoleByAssociationByRoleType = sessionStateChangeSet.RoleByAssociationByRoleType;
            this.AssociationByRoleByRoleType = sessionStateChangeSet.AssociationByRoleByRoleType;
        }

        ISession IChangeSet.Session => this.Session;
        public RemoteSession Session { get; }

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Instantiated { get; }

        public IDictionary<IRoleType, IDictionary<Identity, object>> RoleByAssociationByRoleType { get; }

        public IDictionary<IAssociationType, IDictionary<Identity, object>> AssociationByRoleByRoleType { get; }
    }
}
