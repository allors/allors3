// <copyright file="ChangeSet.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Collections;
    using Meta;
    using Ranges;

    public sealed class ChangeSet : IChangeSet
    {
        public ChangeSet(Session session, ISet<IStrategy> created, ISet<IStrategy> instantiated)
        {
            this.Session = session;
            this.Created = created ?? EmptySet<IStrategy>.Instance;
            this.Instantiated = instantiated ?? EmptySet<IStrategy>.Instance;
            this.AssociationsByRoleType = new Dictionary<IRoleType, ISet<IStrategy>>();
            this.RolesByAssociationType = new Dictionary<IAssociationType, ISet<IStrategy>>();
            this.RoleIdsByAssociationType = new Dictionary<IAssociationType, ISet<long>>();
        }

        private Session Session { get; }

        ISession IChangeSet.Session => this.Session;

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Instantiated { get; }

        public IDictionary<IRoleType, ISet<IStrategy>> AssociationsByRoleType { get; }

        public IDictionary<IAssociationType, ISet<IStrategy>> RolesByAssociationType { get; }

        public IDictionary<IAssociationType, ISet<long>> RoleIdsByAssociationType { get; }

        public void AddSessionStateChanges(IDictionary<IPropertyType, IDictionary<Strategy, object>> sessionStateChangeSet)
        {
            foreach (var kvp in sessionStateChangeSet)
            {
                var strategies = new HashSet<IStrategy>(kvp.Value.Keys);

                switch (kvp.Key)
                {
                    case IAssociationType associationType:
                        this.RolesByAssociationType.Add(associationType, strategies);
                        this.RoleIdsByAssociationType.Add(associationType, new HashSet<long>(kvp.Value.Keys.Select(v => v.Id)));
                        break;
                    case IRoleType roleType:
                        this.AssociationsByRoleType.Add(roleType, strategies);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"PropertyType {kvp.Key}");
                }
            }
        }

        public void DiffUnit(Strategy association, IRelationType relationType, object current, object previous)
        {
            if (!Equals(current, previous))
            {
                this.AddAssociation(relationType, association);
            }
        }

        public void DiffComposite(Strategy association, IRelationType relationType, long current, long previous)
        {
            if (current == previous)
            {
                return;
            }

            if (previous != 0)
            {
                this.AddRoleId(relationType, previous);
            }

            if (current != 0)
            {
                this.AddRoleId(relationType, current);
            }

            this.AddAssociation(relationType, association);
        }

        public void DiffComposites(Strategy association, IRelationType relationType, IRange<long> current, IRange<long> previous)
        {
            var hasChange = false;

            foreach (var v in previous.Except(current))
            {
                this.AddRoleId(relationType, v);
                hasChange = true;
            }

            foreach (var v in current.Except(previous))
            {
                this.AddRoleId(relationType, v);
                hasChange = true;
            }

            if (hasChange)
            {
                this.AddAssociation(relationType, association);
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

            associations.Add(association);
        }

        // The ID is always recorded; the strategy only when this session holds it.
        private void AddRoleId(IRelationType relationType, long id)
        {
            var associationType = relationType.AssociationType;

            if (!this.RoleIdsByAssociationType.TryGetValue(associationType, out var roleIds))
            {
                roleIds = new HashSet<long>();
                this.RoleIdsByAssociationType.Add(associationType, roleIds);
            }

            roleIds.Add(id);

            var role = this.Session.GetStrategy(id);
            if (role != null)
            {
                this.AddRole(relationType, role);
            }
        }

        private void AddRole(IRelationType relationType, Strategy role)
        {
            var associationType = relationType.AssociationType;

            if (!this.RolesByAssociationType.TryGetValue(associationType, out var roles))
            {
                roles = new HashSet<IStrategy>();
                this.RolesByAssociationType.Add(associationType, roles);
            }

            roles.Add(role);
        }
    }
}
