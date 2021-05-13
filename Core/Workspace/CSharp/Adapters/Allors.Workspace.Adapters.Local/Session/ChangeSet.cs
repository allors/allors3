// <copyright file="LocalSessionChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Collections;
    using Meta;

    internal sealed class ChangeSet : IChangeSet
    {
        public ChangeSet(Session session)
        {
            this.Session = session;
            this.Created = new HashSet<IStrategy>();
            this.Instantiated = new HashSet<IStrategy>();
            this.AssociationsByRoleType = new Dictionary<IRoleType, ISet<IStrategy>>();
            this.RolesByAssociationType = new Dictionary<IAssociationType, ISet<IStrategy>>();
        }

        public ChangeSet(ISet<IStrategy> created, ISet<IStrategy> instantiated)
        {
            this.Created = created ?? EmptySet<IStrategy>.Instance;
            this.Instantiated = instantiated ?? EmptySet<IStrategy>.Instance;
        }

        ISession IChangeSet.Session => this.Session;
        private Session Session { get; }

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Instantiated { get; }

        public IDictionary<IRoleType, ISet<IStrategy>> AssociationsByRoleType { get; }

        public IDictionary<IAssociationType, ISet<IStrategy>> RolesByAssociationType { get; }

        internal void AddSessionStateChanges(IDictionary<IPropertyType, IDictionary<long, object>> sessionStateChangeSet)
        {
            foreach (var kvp in sessionStateChangeSet)
            {
                var ids = kvp.Value.Keys;
                var strategies = new HashSet<IStrategy>(ids.Select(v => this.Session.GetStrategy(v)));

                switch (kvp.Key)
                {
                    case IAssociationType associationType:
                        this.RolesByAssociationType.Add(associationType, strategies);
                        break;
                    case IRoleType roleType:
                        this.AssociationsByRoleType.Add(roleType, strategies);
                        break;
                }
            }
        }

        internal void Diff(Strategy association, IRelationType relationType, object current, object previous)
        {
            var roleType = relationType.RoleType;

            if (roleType.ObjectType.IsUnit)
            {
                if (!Equals(current, previous))
                {
                    this.AddAssociation(relationType, association);
                }
            }
            else
            {
                if (roleType.IsOne)
                {
                    if (!Equals(current, previous))
                    {
                        if (previous != null)
                        {
                            this.AddRole(relationType, this.Session.GetStrategy((long)previous));
                        }

                        if (current != null)
                        {
                            this.AddRole(relationType, this.Session.GetStrategy((long)current));
                        }

                        this.AddAssociation(relationType, association);
                    }
                }
                else
                {
                    var numbers = this.Session.Numbers;
                    var hasChange = false;

                    var addedRoles = numbers.Except(current, previous);
                    foreach (var v in numbers.Enumerate(addedRoles))
                    {
                        this.AddRole(relationType, this.Session.GetStrategy(v));
                        hasChange = true;
                    }

                    var removedRoles = numbers.Except(previous, current);
                    foreach (var v in numbers.Enumerate(removedRoles))
                    {
                        this.AddRole(relationType, this.Session.GetStrategy(v));
                        hasChange = true;
                    }

                    if (hasChange)
                    {
                        this.AddAssociation(relationType, association);
                    }
                }
            }
        }

        private void AddAssociation(IRelationType relationType, Strategy association)
        {
            var roleType = relationType.RoleType;

            if (!this.AssociationsByRoleType.TryGetValue(roleType, out var associations))
            {
                associations = new HashSet<IStrategy>();
                this.AssociationsByRoleType.Add(roleType, associations);
            }

            _ = associations.Add(association);
        }

        private void AddRole(IRelationType relationType, Strategy role)
        {
            var associationType = relationType.AssociationType;

            if (!this.RolesByAssociationType.TryGetValue(associationType, out var roles))
            {
                roles = new HashSet<IStrategy>();
                this.RolesByAssociationType.Add(associationType, roles);
            }

            _ = roles.Add(role);
        }
    }
}
