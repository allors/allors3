// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class SessionState
    {
        private readonly IDictionary<IRoleType, IDictionary<RemoteStrategy, object>> roleByAssociationByRoleType;
        private readonly IDictionary<IAssociationType, IDictionary<RemoteStrategy, object>> associationByRoleByAssociationType;

        private IDictionary<IRoleType, IDictionary<RemoteStrategy, object>> changedRoleByAssociationByRoleType;
        private IDictionary<IAssociationType, IDictionary<RemoteStrategy, object>> changedAssociationByRoleByAssociationType;

        public SessionState()
        {
            this.roleByAssociationByRoleType = new Dictionary<IRoleType, IDictionary<RemoteStrategy, object>>();
            this.associationByRoleByAssociationType = new Dictionary<IAssociationType, IDictionary<RemoteStrategy, object>>();

            this.changedRoleByAssociationByRoleType = new Dictionary<IRoleType, IDictionary<RemoteStrategy, object>>();
            this.changedAssociationByRoleByAssociationType = new Dictionary<IAssociationType, IDictionary<RemoteStrategy, object>>();
        }

        public SessionStateChangeSet Checkpoint()
        {
            foreach (var roleType in this.changedRoleByAssociationByRoleType.Keys.ToArray())
            {
                var changedRoleByAssociation = this.changedRoleByAssociationByRoleType[roleType];
                var roleByAssociation = this.RoleByAssociation(roleType);

                foreach (var association in changedRoleByAssociation.Keys.ToArray())
                {
                    var role = changedRoleByAssociation[association];
                    roleByAssociation.TryGetValue(association, out var originalRole);

                    var areEqual = ReferenceEquals(originalRole, role) ||
                                   (roleType.IsOne && Equals(originalRole, role)) ||
                                   (roleType.IsMany && ((IStructuralEquatable)originalRole)?.Equals((IStructuralEquatable)role) == true);

                    if (areEqual)
                    {
                        changedRoleByAssociation.Remove(association);
                        continue;
                    }

                    roleByAssociation[association] = role;
                }

                if (roleByAssociation.Count == 0)
                {
                    this.changedRoleByAssociationByRoleType.Remove(roleType);
                }
            }

            foreach (var associationType in this.changedAssociationByRoleByAssociationType.Keys.ToArray())
            {
                var changedAssociationByRole = this.changedAssociationByRoleByAssociationType[associationType];
                var associationByRole = this.AssociationByRole(associationType);

                foreach (var role in changedAssociationByRole.Keys.ToArray())
                {
                    var changedAssociation = changedAssociationByRole[role];
                    associationByRole.TryGetValue(role, out var originalRole);

                    var areEqual = ReferenceEquals(originalRole, changedAssociation) ||
                                   (associationType.IsOne && Equals(originalRole, changedAssociation)) ||
                                   (associationType.IsMany && ((IStructuralEquatable)originalRole)?.Equals((IStructuralEquatable)changedAssociation) == true);

                    if (areEqual)
                    {
                        changedAssociationByRole.Remove(role);
                        continue;
                    }

                    associationByRole[role] = changedAssociation;
                }

                if (associationByRole.Count == 0)
                {
                    this.changedAssociationByRoleByAssociationType.Remove(associationType);
                }
            }

            var changeSet = new SessionStateChangeSet(this.changedRoleByAssociationByRoleType, this.changedAssociationByRoleByAssociationType);

            this.changedRoleByAssociationByRoleType = new Dictionary<IRoleType, IDictionary<RemoteStrategy, object>>();
            this.changedAssociationByRoleByAssociationType = new Dictionary<IAssociationType, IDictionary<RemoteStrategy, object>>();

            return changeSet;
        }

        public void GetRole(RemoteStrategy association, IRoleType roleType, out object role)
        {
            if (this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation) &&
                changedRoleByAssociation.TryGetValue(association, out role))
            {
                return;
            }

            this.RoleByAssociation(roleType).TryGetValue(association, out role);
        }

        public void SetRole(RemoteStrategy association, IRoleType roleType, object role)
        {
            if (role == null)
            {
                this.RemoveRole(association, roleType);
                return;
            }

            if (roleType.ObjectType.IsUnit)
            {
                // Role
                var unitRole = roleType.NormalizeUnit(role);
                this.ChangedRoleByAssociation(roleType)[association] = unitRole;
            }
            else
            {
                var associationType = roleType.AssociationType;
                this.GetRole(association, roleType, out var previousRole);
                if (roleType.IsOne)
                {
                    var roleIdentity = (RemoteStrategy)role;
                    this.GetAssociation(roleIdentity, associationType, out var previousAssociation);

                    // Role
                    var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                    changedRoleByAssociation[association] = roleIdentity;

                    // Association
                    var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
                    if (associationType.IsOne)
                    {
                        // One to One
                        var previousAssociationObject = (RemoteStrategy)previousAssociation;
                        if (previousAssociationObject != null)
                        {
                            changedRoleByAssociation[previousAssociationObject] = null;
                        }

                        if (previousRole != null)
                        {
                            var previousRoleObject = (RemoteStrategy)previousRole;
                            changedAssociationByRole[previousRoleObject] = null;
                        }

                        changedAssociationByRole[roleIdentity] = association;
                    }
                    else
                    {
                        changedAssociationByRole[roleIdentity] = NullableSortableArraySet.Remove(previousAssociation, roleIdentity);
                    }
                }
                else
                {
                    var compositesRole = (RemoteStrategy[])role;
                    var previousRoles = (RemoteStrategy[])previousRole ?? Array.Empty<RemoteStrategy>();

                    // Use Diff (Add/Remove)
                    var addedRoles = compositesRole.Except(previousRoles);
                    var removedRoles = previousRoles.Except(compositesRole);

                    foreach (var addedRole in addedRoles)
                    {
                        this.AddRole(association, roleType, addedRole);
                    }

                    foreach (var removeRole in removedRoles)
                    {
                        this.RemoveRole(association, roleType, removeRole);
                    }
                }
            }
        }

        public void AddRole(RemoteStrategy association, IRoleType roleType, RemoteStrategy role)
        {
            var associationType = roleType.AssociationType;
            this.GetAssociation(role, associationType, out var previousAssociation);

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            this.GetRole(association, roleType, out var previousRole);
            var roleArray = (RemoteStrategy[])previousRole;
            roleArray = NullableSortableArraySet.Add(roleArray, role);
            changedRoleByAssociation[association] = roleArray;

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            if (associationType.IsOne)
            {
                // One to Many
                var previousAssociationObject = (RemoteStrategy)previousAssociation;
                if (previousAssociationObject != null)
                {
                    this.GetRole(previousAssociationObject, roleType, out var previousAssociationRole);
                    changedRoleByAssociation[previousAssociationObject] = NullableSortableArraySet.Remove(previousAssociationRole, role);
                }

                changedAssociationByRole[role] = association;
            }
            else
            {
                // Many to Many
                changedAssociationByRole[role] = NullableSortableArraySet.Add(previousAssociation, association);
            }
        }

        public void RemoveRole(RemoteStrategy association, IRoleType roleType, RemoteStrategy role)
        {
            var associationType = roleType.AssociationType;
            this.GetAssociation(role, associationType, out var previousAssociation);

            this.GetRole(association, roleType, out var previousRole);
            if (previousRole != null)
            {
                // Role
                var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                changedRoleByAssociation[association] = NullableSortableArraySet.Remove(previousRole, role);

                // Association
                var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
                if (associationType.IsOne)
                {
                    // One to Many
                    changedAssociationByRole[role] = null;
                }
                else
                {
                    // Many to Many
                    changedAssociationByRole[role] = NullableSortableArraySet.Add(previousAssociation, association);
                }
            }
        }

        public void RemoveRole(RemoteStrategy association, IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                // Role
                this.ChangedRoleByAssociation(roleType)[association] = null;
            }
            else
            {
                var associationType = roleType.AssociationType;
                this.GetRole(association, roleType, out var previousRole);

                if (roleType.IsOne)
                {
                    // Role
                    var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                    changedRoleByAssociation[association] = null;

                    // Association
                    var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
                    if (associationType.IsOne)
                    {
                        // One to One
                        if (previousRole != null)
                        {
                            var previousRoleObject = (RemoteStrategy)previousRole;
                            changedAssociationByRole[previousRoleObject] = null;
                        }
                    }
                }
                else
                {
                    var previousRoles = (RemoteStrategy[])previousRole ?? Array.Empty<RemoteStrategy>();

                    // Use Diff (Remove)
                    foreach (var removeRole in previousRoles)
                    {
                        this.RemoveRole(association, roleType, removeRole);
                    }
                }
            }
        }

        public void GetAssociation(RemoteStrategy role, IAssociationType associationType, out object association)
        {
            if (this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole) &&
                changedAssociationByRole.TryGetValue(role, out association))
            {
                return;
            }

            this.AssociationByRole(associationType).TryGetValue(role, out association);
        }

        private IDictionary<RemoteStrategy, object> AssociationByRole(IAssociationType associationType)
        {
            if (!this.associationByRoleByAssociationType.TryGetValue(associationType, out var associationByRole))
            {
                associationByRole = new Dictionary<RemoteStrategy, object>();
                this.associationByRoleByAssociationType[associationType] = associationByRole;
            }

            return associationByRole;
        }

        private IDictionary<RemoteStrategy, object> RoleByAssociation(IRoleType roleType)
        {
            if (!this.roleByAssociationByRoleType.TryGetValue(roleType, out var roleByAssociation))
            {
                roleByAssociation = new Dictionary<RemoteStrategy, object>();
                this.roleByAssociationByRoleType[roleType] = roleByAssociation;
            }

            return roleByAssociation;
        }

        private IDictionary<RemoteStrategy, object> ChangedAssociationByRole(IAssociationType associationType)
        {
            if (!this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole))
            {
                changedAssociationByRole = new Dictionary<RemoteStrategy, object>();
                this.changedAssociationByRoleByAssociationType[associationType] = changedAssociationByRole;
            }

            return changedAssociationByRole;
        }

        private IDictionary<RemoteStrategy, object> ChangedRoleByAssociation(IRoleType roleType)
        {
            if (!this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation))
            {
                changedRoleByAssociation = new Dictionary<RemoteStrategy, object>();
                this.changedRoleByAssociationByRoleType[roleType] = changedRoleByAssociation;
            }

            return changedRoleByAssociation;
        }
    }
}
