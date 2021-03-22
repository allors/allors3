// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class LocalSessionState
    {
        private readonly IDictionary<IRoleType, IDictionary<LocalStrategy, object>> roleByAssociationByRoleType;
        private readonly IDictionary<IAssociationType, IDictionary<LocalStrategy, object>> associationByRoleByAssociationType;

        private IDictionary<IRoleType, IDictionary<LocalStrategy, object>> changedRoleByAssociationByRoleType;
        private IDictionary<IAssociationType, IDictionary<LocalStrategy, object>> changedAssociationByRoleByAssociationType;

        public LocalSessionState()
        {
            this.roleByAssociationByRoleType = new Dictionary<IRoleType, IDictionary<LocalStrategy, object>>();
            this.associationByRoleByAssociationType = new Dictionary<IAssociationType, IDictionary<LocalStrategy, object>>();

            this.changedRoleByAssociationByRoleType = new Dictionary<IRoleType, IDictionary<LocalStrategy, object>>();
            this.changedAssociationByRoleByAssociationType = new Dictionary<IAssociationType, IDictionary<LocalStrategy, object>>();
        }

        public LocalSessionStateChangeSet Checkpoint()
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

            var changeSet = new LocalSessionStateChangeSet(this.changedRoleByAssociationByRoleType, this.changedAssociationByRoleByAssociationType);

            this.changedRoleByAssociationByRoleType = new Dictionary<IRoleType, IDictionary<LocalStrategy, object>>();
            this.changedAssociationByRoleByAssociationType = new Dictionary<IAssociationType, IDictionary<LocalStrategy, object>>();

            return changeSet;
        }

        public void GetRole(LocalStrategy association, IRoleType roleType, out object role)
        {
            if (this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation) &&
                changedRoleByAssociation.TryGetValue(association, out role))
            {
                return;
            }

            this.RoleByAssociation(roleType).TryGetValue(association, out role);
        }

        public void SetUnitRole(LocalStrategy association, IRoleType roleType, object role)
        {
            if (role == null)
            {
                this.RemoveRole(association, roleType);
                return;
            }

            var unitRole = roleType.NormalizeUnit(role);
            this.ChangedRoleByAssociation(roleType)[association] = unitRole;
        }

        public void SetCompositeRole(LocalStrategy association, IRoleType roleType, object role)
        {
            if (role == null)
            {
                this.RemoveRole(association, roleType);
                return;
            }

            var associationType = roleType.AssociationType;
            this.GetRole(association, roleType, out var previousRole);
            var roleIdentity = (LocalStrategy)role;
            this.GetAssociation(roleIdentity, associationType, out var previousAssociation);

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            changedRoleByAssociation[association] = roleIdentity;

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            if (associationType.IsOne)
            {
                // One to One
                var previousAssociationObject = (LocalStrategy)previousAssociation;
                if (previousAssociationObject != null)
                {
                    changedRoleByAssociation[previousAssociationObject] = null;
                }

                if (previousRole != null)
                {
                    var previousRoleObject = (LocalStrategy)previousRole;
                    changedAssociationByRole[previousRoleObject] = null;
                }

                changedAssociationByRole[roleIdentity] = association;
            }
            else
            {
                changedAssociationByRole[roleIdentity] = NullableSortableArraySet.Remove(previousAssociation, roleIdentity);
            }
        }

        public void SetCompositesRole(LocalStrategy association, IRoleType roleType, object role)
        {
            if (role == null)
            {
                this.RemoveRole(association, roleType);
                return;
            }

            this.GetRole(association, roleType, out var previousRole);
            var compositesRole = (LocalStrategy[])role;
            var previousRoles = (LocalStrategy[])previousRole ?? Array.Empty<LocalStrategy>();

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


        public void AddRole(LocalStrategy association, IRoleType roleType, LocalStrategy role)
        {
            var associationType = roleType.AssociationType;
            this.GetAssociation(role, associationType, out var previousAssociation);

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            this.GetRole(association, roleType, out var previousRole);
            var roleArray = (LocalStrategy[])previousRole;
            roleArray = NullableSortableArraySet.Add(roleArray, role);
            changedRoleByAssociation[association] = roleArray;

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            if (associationType.IsOne)
            {
                // One to Many
                var previousAssociationObject = (LocalStrategy)previousAssociation;
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

        public void RemoveRole(LocalStrategy association, IRoleType roleType, LocalStrategy role)
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

        public void RemoveRole(LocalStrategy association, IRoleType roleType)
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
                            var previousRoleObject = (LocalStrategy)previousRole;
                            changedAssociationByRole[previousRoleObject] = null;
                        }
                    }
                }
                else
                {
                    var previousRoles = (LocalStrategy[])previousRole ?? Array.Empty<LocalStrategy>();

                    // Use Diff (Remove)
                    foreach (var removeRole in previousRoles)
                    {
                        this.RemoveRole(association, roleType, removeRole);
                    }
                }
            }
        }

        public void GetAssociation(LocalStrategy role, IAssociationType associationType, out object association)
        {
            if (this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole) &&
                changedAssociationByRole.TryGetValue(role, out association))
            {
                return;
            }

            this.AssociationByRole(associationType).TryGetValue(role, out association);
        }

        private IDictionary<LocalStrategy, object> AssociationByRole(IAssociationType associationType)
        {
            if (!this.associationByRoleByAssociationType.TryGetValue(associationType, out var associationByRole))
            {
                associationByRole = new Dictionary<LocalStrategy, object>();
                this.associationByRoleByAssociationType[associationType] = associationByRole;
            }

            return associationByRole;
        }

        private IDictionary<LocalStrategy, object> RoleByAssociation(IRoleType roleType)
        {
            if (!this.roleByAssociationByRoleType.TryGetValue(roleType, out var roleByAssociation))
            {
                roleByAssociation = new Dictionary<LocalStrategy, object>();
                this.roleByAssociationByRoleType[roleType] = roleByAssociation;
            }

            return roleByAssociation;
        }

        private IDictionary<LocalStrategy, object> ChangedAssociationByRole(IAssociationType associationType)
        {
            if (!this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole))
            {
                changedAssociationByRole = new Dictionary<LocalStrategy, object>();
                this.changedAssociationByRoleByAssociationType[associationType] = changedAssociationByRole;
            }

            return changedAssociationByRole;
        }

        private IDictionary<LocalStrategy, object> ChangedRoleByAssociation(IRoleType roleType)
        {
            if (!this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation))
            {
                changedRoleByAssociation = new Dictionary<LocalStrategy, object>();
                this.changedRoleByAssociationByRoleType[roleType] = changedRoleByAssociation;
            }

            return changedRoleByAssociation;
        }

        public bool IsAssociationForRole(LocalStrategy association, IRoleType roleType, LocalStrategy forRole)
        {
            this.GetRole(association, roleType, out var role);
            return role?.Equals(forRole) == true;
        }
    }
}
