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
    using System.Linq;
    using Meta;

    internal sealed class RemoteChangeSet : IChangeSet
    {
        public RemoteChangeSet(RemoteSession session, ISet<IStrategy> created, ISet<IStrategy> instantiated, SessionStateChangeSet sessionStateChangeSet)
        {
            this.Session = session;
            this.Created = created;
            this.Instantiated = instantiated;
            this.AssociationByRoleType = sessionStateChangeSet.RoleByAssociationByRoleType
                .ToDictionary(
                    v => v.Key,
                    v => (ISet<long>)new HashSet<long>(v.Value.Keys));
            this.RoleByRoleType = sessionStateChangeSet.AssociationByRoleByRoleType.ToDictionary(
                v => v.Key,
                v => (ISet<long>)new HashSet<long>(v.Value.Keys));
            ;
        }

        ISession IChangeSet.Session => this.Session;
        public RemoteSession Session { get; }

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Instantiated { get; }

        public IDictionary<IRoleType, ISet<long>> AssociationByRoleType { get; }

        public IDictionary<IAssociationType, ISet<long>> RoleByRoleType { get; }

        internal void AddAssociation(IRelationType relationType, long association)
        {
            var roleType = relationType.RoleType;

            if (!this.AssociationByRoleType.TryGetValue(roleType, out var associations))
            {
                associations = new HashSet<long>();
                this.AssociationByRoleType.Add(roleType, associations);
            }

            _ = associations.Add(association);
        }

        internal void AddRole(IRelationType relationType, long role)
        {
            var associationType = relationType.AssociationType;

            if (!this.RoleByRoleType.TryGetValue(associationType, out var roles))
            {
                roles = new HashSet<long>();
                this.RoleByRoleType.Add(associationType, roles);
            }

            _ = roles.Add(role);
        }
    }
}
