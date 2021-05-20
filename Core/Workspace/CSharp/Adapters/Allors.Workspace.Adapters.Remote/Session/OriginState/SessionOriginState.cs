// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using Meta;
    using Numbers;

    public class SessionOriginState
    {
        private readonly INumbers numbers;
        private readonly PropertyByObjectByPropertyType propertyByObjectByPropertyType;

        internal SessionOriginState(INumbers numbers)
        {
            this.numbers = numbers;
            this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType(numbers);
        }

        internal void Checkpoint(ChangeSet changeSet) =>
            changeSet.AddSessionStateChanges(this.propertyByObjectByPropertyType.Checkpoint());

        internal object Get(long @object, IPropertyType propertyType) =>
            this.propertyByObjectByPropertyType.Get(@object, propertyType);

        internal void SetUnitRole(long association, IRoleType roleType, object role) =>
            this.propertyByObjectByPropertyType.Set(association, roleType, role);

        internal void SetCompositeRole(long association, IRoleType roleType, long? newRole)
        {
            if (newRole == null)
            {
                this.RemoveRole(association, roleType);
                return;
            }

            var associationType = roleType.AssociationType;

            // Association
            var previousRole = (long?)this.propertyByObjectByPropertyType.Get(association, roleType);
            if (previousRole.HasValue)
            {
                this.propertyByObjectByPropertyType.Set(previousRole.Value, associationType, null);
            }

            if (associationType.IsOne)
            {
                // OneToOne
                var previousAssociation =
                    (long?)this.propertyByObjectByPropertyType.Get(newRole.Value, associationType);
                if (previousAssociation.HasValue)
                {
                    this.propertyByObjectByPropertyType.Set(previousAssociation.Value, roleType, null);
                }
            }

            // Role
            this.propertyByObjectByPropertyType.Set(association, roleType, newRole);
        }

        internal void SetCompositesRole(long association, IRoleType roleType, object newRole)
        {
            if (newRole == null)
            {
                this.RemoveRole(association, roleType);
                return;
            }

            var previousRole = (long[])this.Get(association, roleType);

            // Use Diff (Add/Remove)
            var addedRoles = this.numbers.Except(newRole, previousRole);
            var removedRoles = this.numbers.Except(previousRole, newRole);

            foreach (var addedRole in this.numbers.Enumerate(addedRoles))
            {
                this.AddRole(association, roleType, addedRole);
            }

            foreach (var removedRole in this.numbers.Enumerate(removedRoles))
            {
                this.RemoveRole(association, roleType, removedRole);
            }
        }

        internal void AddRole(long association, IRoleType roleType, long roleToAdd)
        {
            var associationType = roleType.AssociationType;
            var previousRole = this.propertyByObjectByPropertyType.Get(association, roleType);

            if (this.numbers.Contains(previousRole, roleToAdd))
            {
                return;
            }

            // Role
            this.propertyByObjectByPropertyType.Set(association, roleType, this.numbers.Add(previousRole, roleToAdd));

            // Association
            if (associationType.IsOne)
            {
                var previousRoleAssociations =
                    this.propertyByObjectByPropertyType.Get((long)previousRole, associationType);
                this.propertyByObjectByPropertyType.Set((long)previousRole, associationType,
                    this.numbers.Remove(previousRoleAssociations, association));
            }

            var roleAssociations = this.propertyByObjectByPropertyType.Get(roleToAdd, associationType);
            this.propertyByObjectByPropertyType.Set(roleToAdd, associationType,
                this.numbers.Add(roleAssociations, association));
        }

        private void RemoveRole(long association, IRoleType roleType, long roleToRemove)
        {
            var associationType = roleType.AssociationType;

            var previousRole = this.propertyByObjectByPropertyType.Get(association, roleType);
            if (associationType.IsOne)
            {
                if ((long?)previousRole == roleToRemove)
                {
                    return;
                }

                // Role
                this.propertyByObjectByPropertyType.Set(association, roleType, null);

                // Association
                var removedRole = this.numbers.Remove(previousRole, roleToRemove);
                this.propertyByObjectByPropertyType.Set(roleToRemove, associationType, removedRole);
            }
            else
            {
                if (!this.numbers.Contains(previousRole, roleToRemove))
                {
                    return;
                }

                // Role
                var removedRole = this.numbers.Remove(previousRole, roleToRemove);
                this.propertyByObjectByPropertyType.Set(association, roleType, removedRole);

                // Association
                var previousAssociations = this.propertyByObjectByPropertyType.Get(roleToRemove, associationType);
                var removedAssociations = this.numbers.Remove(previousAssociations, association);
                this.propertyByObjectByPropertyType.Set(roleToRemove, associationType, removedAssociations);
            }
        }

        private void RemoveRole(long association, IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                // Role
                this.SetUnitRole(association, roleType, null);
            }
            else
            {
                var previousRole = this.Get(association, roleType);

                if (roleType.IsOne)
                {
                    if (previousRole != null)
                    {
                        this.RemoveRole(association, roleType, (long)previousRole);
                    }
                }
                else
                {
                    foreach (var removeRole in this.numbers.Enumerate(previousRole))
                    {
                        this.RemoveRole(association, roleType, removeRole);
                    }
                }
            }
        }
    }
}
