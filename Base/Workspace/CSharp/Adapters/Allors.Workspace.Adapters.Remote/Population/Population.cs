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
    using Allors.Workspace.Meta;

    internal class Population
    {
        private readonly Dictionary<IRoleType, Dictionary<long, object>> roleByAssociationByRoleType;
        private readonly Dictionary<IAssociationType, Dictionary<long, object>> associationByRoleByAssociationType;

        private Dictionary<IRoleType, Dictionary<long, object>> changedRoleByAssociationByRoleType;
        private Dictionary<IAssociationType, Dictionary<long, object>> changedAssociationByRoleByAssociationType;

        internal Population()
        {
            this.roleByAssociationByRoleType = new Dictionary<IRoleType, Dictionary<long, object>>();
            this.associationByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<long, object>>();

            this.changedRoleByAssociationByRoleType = new Dictionary<IRoleType, Dictionary<long, object>>();
            this.changedAssociationByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<long, object>>();
        }

        internal ChangeSet Snapshot()
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

            var snapshot = new ChangeSet(this.changedRoleByAssociationByRoleType, this.changedAssociationByRoleByAssociationType);

            this.changedRoleByAssociationByRoleType = new Dictionary<IRoleType, Dictionary<long, object>>();
            this.changedAssociationByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<long, object>>();

            return snapshot;
        }

        internal void GetRole(long association, IRoleType roleType, out object role)
        {
            if (this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation) &&
                changedRoleByAssociation.TryGetValue(association, out role))
            {
                return;
            }

            this.RoleByAssociation(roleType).TryGetValue(association, out role);
        }

        internal void SetRole(long association, IRoleType roleType, object role)
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
                    var compositeRole = roleType.NormalizeComposite(role);
                    this.GetAssociation(compositeRole, associationType, out var previousAssociation);

                    // Role
                    var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                    changedRoleByAssociation[association] = compositeRole;

                    // Association
                    var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
                    if (associationType.IsOne)
                    {
                        // One to One
                        var previousAssociationObject = (long?)previousAssociation;
                        if (previousAssociationObject != null)
                        {
                            changedRoleByAssociation[previousAssociationObject.Value] = null;
                        }

                        if (previousRole != null)
                        {
                            var previousRoleObject = (long)previousRole;
                            changedAssociationByRole[previousRoleObject] = null;
                        }

                        changedAssociationByRole[compositeRole] = association;
                    }
                    else
                    {
                        changedAssociationByRole[compositeRole] = NullableArrayList.Remove(previousAssociation, compositeRole);
                    }
                }
                else
                {
                    var compositesRole = roleType.NormalizeComposites(role);
                    var previousRoles = (long[])previousRole ?? Array.Empty<long>();

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

        internal void AddRole(long association, IRoleType roleType, long role)
        {
            var associationType = roleType.AssociationType;
            this.GetAssociation(role, associationType, out var previousAssociation);

            // Role
            var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
            this.GetRole(association, roleType, out var previousRole);
            var roleArray = (long?[])previousRole;
            roleArray = NullableArrayList.Add(roleArray, role);
            changedRoleByAssociation[association] = roleArray;

            // Association
            var changedAssociationByRole = this.ChangedAssociationByRole(associationType);
            if (associationType.IsOne)
            {
                // One to Many
                var previousAssociationObject = (long?)previousAssociation;
                if (previousAssociationObject != null)
                {
                    this.GetRole(previousAssociationObject.Value, roleType, out var previousAssociationRole);
                    changedRoleByAssociation[previousAssociationObject.Value] = NullableArrayList.Remove(previousAssociationRole, role);
                }

                changedAssociationByRole[role] = association;
            }
            else
            {
                // Many to Many
                changedAssociationByRole[role] = NullableArrayList.Add(previousAssociation, association);
            }
        }

        internal void RemoveRole(long association, IRoleType roleType, long role)
        {
            var associationType = roleType.AssociationType;
            this.GetAssociation(role, associationType, out var previousAssociation);

            this.GetRole(association, roleType, out var previousRole);
            if (previousRole != null)
            {
                // Role
                var changedRoleByAssociation = this.ChangedRoleByAssociation(roleType);
                changedRoleByAssociation[association] = NullableArrayList.Remove(previousRole, role);

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
                    changedAssociationByRole[role] = NullableArrayList.Add(previousAssociation, association);
                }
            }
        }

        internal void RemoveRole(long association, IRoleType roleType)
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
                            var previousRoleObject = (long)previousRole;
                            changedAssociationByRole[previousRoleObject] = null;
                        }
                    }
                }
                else
                {
                    var previousRoles = (long[])previousRole ?? Array.Empty<long>();

                    // Use Diff (Remove)
                    foreach (var removeRole in previousRoles)
                    {
                        this.RemoveRole(association, roleType, removeRole);
                    }
                }
            }
        }

        internal void GetAssociation(long role, IAssociationType associationType, out object association)
        {
            if (this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole) &&
                changedAssociationByRole.TryGetValue(role, out association))
            {
                return;
            }

            this.AssociationByRole(associationType).TryGetValue(role, out association);
        }

        private Dictionary<long, object> AssociationByRole(IAssociationType asscociationType)
        {
            if (!this.associationByRoleByAssociationType.TryGetValue(asscociationType, out var associationByRole))
            {
                associationByRole = new Dictionary<long, object>();
                this.associationByRoleByAssociationType[asscociationType] = associationByRole;
            }

            return associationByRole;
        }

        private Dictionary<long, object> RoleByAssociation(IRoleType roleType)
        {
            if (!this.roleByAssociationByRoleType.TryGetValue(roleType, out var roleByAssociation))
            {
                roleByAssociation = new Dictionary<long, object>();
                this.roleByAssociationByRoleType[roleType] = roleByAssociation;
            }

            return roleByAssociation;
        }

        private Dictionary<long, object> ChangedAssociationByRole(IAssociationType associationType)
        {
            if (!this.changedAssociationByRoleByAssociationType.TryGetValue(associationType, out var changedAssociationByRole))
            {
                changedAssociationByRole = new Dictionary<long, object>();
                this.changedAssociationByRoleByAssociationType[associationType] = changedAssociationByRole;
            }

            return changedAssociationByRole;
        }

        private Dictionary<long, object> ChangedRoleByAssociation(IRoleType roleType)
        {
            if (!this.changedRoleByAssociationByRoleType.TryGetValue(roleType, out var changedRoleByAssociation))
            {
                changedRoleByAssociation = new Dictionary<long, object>();
                this.changedRoleByAssociationByRoleType[roleType] = changedRoleByAssociation;
            }

            return changedRoleByAssociation;
        }
    }
}
