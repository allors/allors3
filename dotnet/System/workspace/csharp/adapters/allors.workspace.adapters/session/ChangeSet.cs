// <copyright file="ChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
        }

        private Session Session { get; }

        ISession IChangeSet.Session => this.Session;

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Instantiated { get; }

        public IDictionary<IRoleType, ISet<IStrategy>> AssociationsByRoleType { get; }

        public IDictionary<IAssociationType, ISet<IStrategy>> RolesByAssociationType { get; }

        public void AddSessionStateChanges(IDictionary<IPropertyType, IDictionary<long, object>> sessionStateChangeSet)
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

        public void DiffComposite(Strategy association, IRelationType relationType, Strategy current, long? previous)
        {
            if (Equals(current?.Id, previous))
            {
                return;
            }

            if (previous != null)
            {
                this.AddRole(relationType, this.Session.GetStrategy((long)previous));
            }

            if (current != null)
            {
                this.AddRole(relationType, current);
            }

            this.AddAssociation(relationType, association);
        }

        public void DiffComposite(Strategy association, IRelationType relationType, long? current, Strategy previous)
        {
            if (Equals(current, previous?.Id))
            {
                return;
            }

            if (previous != null)
            {
                this.AddRole(relationType, previous);
            }

            if (current != null)
            {
                this.AddRole(relationType, this.Session.GetStrategy((long)current));
            }

            this.AddAssociation(relationType, association);
        }

        public void DiffComposite(Strategy association, IRelationType relationType, Strategy current, Strategy previous)
        {
            if (Equals(current, previous))
            {
                return;
            }

            if (previous != null)
            {
                this.AddRole(relationType, previous);
            }

            if (current != null)
            {
                this.AddRole(relationType, current);
            }

            this.AddAssociation(relationType, association);
        }

        public void DiffComposites(Strategy association, IRelationType relationType, ISet<Strategy> current, IRange previousRange)
        {
            var ranges = this.Session.Workspace.Ranges;
            var previous = new HashSet<Strategy>(ranges.Ensure(previousRange).Select(v => this.Session.GetStrategy(v)));
            this.DiffComposites(association, relationType, current, previous);
        }

        public void DiffComposites(Strategy association, IRelationType relationType, IRange currentRange, ISet<Strategy> previous)
        {
            var ranges = this.Session.Workspace.Ranges;
            var current = new HashSet<Strategy>(ranges.Ensure(currentRange).Select(v => this.Session.GetStrategy(v)));
            this.DiffComposites(association, relationType, current, previous);
        }

        public void DiffComposites(Strategy association, IRelationType relationType, ISet<Strategy> current, ISet<Strategy> previous)
        {
            var hasChange = false;

            foreach (var v in current.Except(previous))
            {
                this.AddRole(relationType, v);
                hasChange = true;
            }

            foreach (var v in previous.Except(current))
            {
                this.AddRole(relationType, v);
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
