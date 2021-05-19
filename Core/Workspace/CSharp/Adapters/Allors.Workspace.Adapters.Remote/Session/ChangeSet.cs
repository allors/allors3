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

    internal sealed class ChangeSet : IChangeSet
    {
        public ChangeSet(Session session, ISet<IStrategy> created, ISet<IStrategy> instantiated)
        {
            this.Session = session;
            this.Created = created;
            this.Instantiated = instantiated;
            this.AssociationsByRoleType = new Dictionary<IRoleType, ISet<IStrategy>>();
            this.RolesByAssociationType = new Dictionary<IAssociationType, ISet<IStrategy>>();
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

        internal void AddAssociation(IRelationType relationType, Strategy association)
        {
            var roleType = relationType.RoleType;

            if (!this.AssociationsByRoleType.TryGetValue(roleType, out var associations))
            {
                associations = new HashSet<IStrategy>();
                this.AssociationsByRoleType.Add(roleType, associations);
            }

            _ = associations.Add(association);
        }

        internal void AddRole(IRelationType relationType, Strategy role)
        {
            var associationType = relationType.AssociationType;

            if (!this.RolesByAssociationType.TryGetValue(associationType, out var roles))
            {
                roles = new HashSet<IStrategy>();
                this.RolesByAssociationType.Add(associationType, roles);
            }

            _ = roles.Add(role);
        }

        internal void DiffCookedWithCooked(Strategy association, IRelationType relationType, object current, object previous)
        {
            var roleType = relationType.RoleType;

            if (roleType.ObjectType.IsUnit)
            {
                if (Equals(current, previous))
                {
                    return;
                }

                this.AddAssociation(relationType, association);
            }
            else
            {
                if (roleType.IsOne)
                {
                    if (Equals(current, previous))
                    {
                        return;
                    }

                    if (current != null)
                    {
                        this.AddRole(relationType, (Strategy)current);
                    }

                    if (previous != null)
                    {
                        this.AddRole(relationType, (Strategy)previous);
                    }

                    this.AddAssociation(relationType, association);
                }
                else
                {
                    var currentRole = (Strategy[])current;
                    var previousRole = (Strategy[])previous;

                    if (currentRole?.Length > 0 && previousRole?.Length > 0)
                    {
                        var addedRoles = currentRole.Except(previousRole);
                        var removedRoles = previousRole.Except(currentRole);

                        var hasChange = false;
                        foreach (var role in addedRoles.Concat(removedRoles))
                        {
                            this.AddRole(relationType, role);
                            hasChange = true;
                        }

                        if (hasChange)
                        {
                            this.AddAssociation(relationType, association);
                        }
                    }
                    else if (currentRole?.Length > 0)
                    {
                        foreach (var role in currentRole)
                        {
                            this.AddRole(relationType, role);
                        }

                        this.AddAssociation(relationType, association);
                    }
                    else if (previousRole?.Length > 0)
                    {
                        foreach (var role in previousRole)
                        {
                            this.AddRole(relationType, role);
                        }

                        this.AddAssociation(relationType, association);
                    }
                }
            }
        }

        internal void DiffCookedWithRaw(Strategy association, IRelationType relationType, object current, object previous)
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
                    if (current == null && previous == null)
                    {
                        return;
                    }

                    var currentRole = (Strategy)current;

                    if (current != null && previous != null)
                    {
                        var previousRole = this.Session.GetStrategy((long)previous);
                        if (Equals(current, previousRole))
                        {
                            return;
                        }

                        this.AddRole(relationType, previousRole);
                        this.AddRole(relationType, currentRole);
                        this.AddAssociation(relationType, association);
                    }
                    else if (current != null)
                    {
                        this.AddRole(relationType, currentRole);
                        this.AddAssociation(relationType, association);
                    }
                    else
                    {
                        var previousRole = this.Session.GetStrategy((long)previous);
                        this.AddRole(relationType, previousRole);
                        this.AddAssociation(relationType, association);
                    }
                }
                else
                {
                    if (current == null && previous == null)
                    {
                        return;
                    }

                    var currentRole = (Strategy[])current;

                    if (previous == null)
                    {
                        foreach (var v in currentRole)
                        {
                            this.AddRole(relationType, v);
                        }

                        this.AddAssociation(relationType, association);
                    }
                    else
                    {
                        var previousRole = ((long[])previous).Select(v => this.Session.GetStrategy(v)).ToArray();

                        if (currentRole == null)
                        {
                            foreach (var v in previousRole)
                            {
                                this.AddRole(relationType, v);
                            }

                            this.AddAssociation(relationType, association);
                        }
                        else
                        {
                            var addedRoles = currentRole.Except(previousRole);
                            var removedRoles = previousRole.Except(currentRole);

                            var hasChange = false;
                            foreach (var role in addedRoles.Concat(removedRoles))
                            {
                                this.AddRole(relationType, role);
                                hasChange = true;
                            }

                            if (hasChange)
                            {
                                this.AddAssociation(relationType, association);
                            }
                        }
                    }
                }
            }
        }

        internal void DiffRawWithRaw(Strategy association, IRelationType relationType, object current, object previous)
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
                    if (current == null && previous == null)
                    {
                        return;
                    }

                    var currentRole = (long[])current;

                    if (previous == null)
                    {
                        foreach (var v in currentRole)
                        {
                            this.AddRole(relationType, this.Session.GetStrategy(v));
                        }

                        this.AddAssociation(relationType, association);
                    }
                    else
                    {
                        var previousRole = (long[])previous;

                        if (currentRole == null)
                        {
                            foreach (var v in previousRole)
                            {
                                this.AddRole(relationType, this.Session.GetStrategy(v));
                            }

                            this.AddAssociation(relationType, association);
                        }
                        else
                        {
                            var addedRoles = currentRole.Except(previousRole);
                            var removedRoles = previousRole.Except(currentRole);

                            var hasChange = false;
                            foreach (var v in addedRoles.Concat(removedRoles))
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
            }
        }

        public void DiffRawWithCooked(Strategy association, IRelationType relationType, object current, object previous)
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
                    var previousRole = (Strategy)previous;
                    if (!Equals(current, previousRole?.Id))
                    {
                        if (previous != null)
                        {
                            this.AddRole(relationType, previousRole);
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
                    if (current == null && previous == null)
                    {
                        return;
                    }

                    var currentRole = (long[])current;

                    if (previous == null)
                    {
                        foreach (var v in currentRole)
                        {
                            this.AddRole(relationType, this.Session.GetStrategy(v));
                        }

                        this.AddAssociation(relationType, association);
                    }
                    else
                    {
                        var previousRole = (Strategy[])previous;

                        if (currentRole == null)
                        {
                            foreach (var v in previousRole)
                            {
                                this.AddRole(relationType, v);
                            }

                            this.AddAssociation(relationType, association);
                        }
                        else
                        {
                            var previousRoleIds = previousRole.Select(v => v.Id).ToArray();
                            var addedRoles = currentRole.Except(previousRoleIds);
                            var removedRoles = previousRoleIds.Except(currentRole);

                            var hasChange = false;
                            foreach (var v in addedRoles.Concat(removedRoles))
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
            }
        }
    }
}
