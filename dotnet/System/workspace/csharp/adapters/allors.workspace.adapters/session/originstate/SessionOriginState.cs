// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Threading;
    using Meta;
    using Ranges;

    public class SessionOriginState
    {
        private readonly IRanges ranges;
        private readonly PropertyByObjectByPropertyType propertyByObjectByPropertyType;

        public SessionOriginState(IRanges ranges)
        {
            this.ranges = ranges;
            this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType();
        }

        public void Checkpoint(ChangeSet changeSet) => changeSet.AddSessionStateChanges(this.propertyByObjectByPropertyType.Checkpoint());

        public object Get(long @object, IPropertyType propertyType) => this.propertyByObjectByPropertyType.Get(@object, propertyType);

        public void SetUnitRole(long association, IRoleType roleType, object role) => this.propertyByObjectByPropertyType.Set(association, roleType, role);

        public void SetCompositeRole(long association, IRoleType roleType, long? newRole)
        {
            if (newRole == default)
            {
                if (roleType.AssociationType.IsOne)
                {
                    this.RemoveCompositeRoleOne2One(association, roleType);
                }
                else
                {
                    this.RemoveCompositeRoleMany2One(association, roleType);
                }
            }
            else if (roleType.AssociationType.IsOne)
            {
                this.SetCompositeRoleOne2One(association, roleType, newRole.Value);
            }
            else
            {
                this.SetCompositeRoleMany2One(association, roleType, newRole.Value);
            }
        }

        public void SetCompositesRole(long association, IRoleType roleType, IRange newRole)
        {
            var previousRole = this.ranges.Unbox(this.Get(association, roleType));

            // Use Diff (Add/Remove)
            var addedRoles = this.ranges.Except(newRole, previousRole);
            var removedRoles = this.ranges.Except(previousRole, newRole);

            foreach (var addedRole in addedRoles)
            {
                this.AddCompositesRole(association, roleType, addedRole);
            }

            foreach (var removedRole in removedRoles)
            {
                this.RemoveCompositesRole(association, roleType, removedRole);
            }
        }

        public void AddCompositesRole(long association, IRoleType roleType, long item)
        {
            if (roleType.AssociationType.IsOne)
            {
                this.AddCompositesRoleOne2Many(association, roleType, item);
            }
            else
            {
                this.AddCompositesRoleMany2Many(association, roleType, item);
            }
        }

        public void RemoveCompositesRole(long association, IRoleType roleType, long item)
        {
            if (roleType.AssociationType.IsOne)
            {
                this.RemoveCompositesRoleOne2Many(association, roleType, item);
            }
            else
            {
                this.RemoveCompositesRoleMany2Many(association, roleType, item);
            }
        }

        private void SetCompositeRoleOne2One(long associationId, IRoleType roleType, long roleId)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var associationType = roleType.AssociationType;

            var previousRoleId = (long?)this.propertyByObjectByPropertyType.Get(associationId, roleType);

            // R = PR
            if (Equals(roleId, previousRoleId))
            {
                return;
            }

            // A --x-- PR
            if (previousRoleId != null)
            {
                this.RemoveCompositeRoleOne2One(associationId, roleType, previousRoleId.Value);
            }

            var roleAssociation = (long?)this.Get(roleId, roleType.AssociationType);

            // RA --x-- R
            if (roleAssociation != null)
            {
                this.RemoveCompositeRoleOne2One(roleAssociation.Value, roleType, roleId);
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(roleId, associationType, associationId);

            // A ----> R
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleId);
        }

        private void SetCompositeRoleMany2One(long association, IRoleType roleType, long newRole)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
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
                var previousAssociation = (long?)this.propertyByObjectByPropertyType.Get(newRole, associationType);
                if (previousAssociation.HasValue)
                {
                    this.propertyByObjectByPropertyType.Set(previousAssociation.Value, roleType, null);
                }
            }

            // Role
            this.propertyByObjectByPropertyType.Set(association, roleType, newRole);
        }

        private void RemoveCompositeRoleOne2One(long associationId, IRoleType roleType)
        {
            var roleId = (long?)this.Get(associationId, roleType);
            if (roleId == null)
            {
                return;
            }

            this.RemoveCompositeRoleOne2One(associationId, roleType, roleId.Value);
        }

        private void RemoveCompositeRoleOne2One(long associationId, IRoleType roleType, long roleId)
        {
            /*                        delete
            *
            *   A ----- R    ->     A       R  =   A       R 
            */

            // A <---- R
            this.propertyByObjectByPropertyType.Set(roleId, roleType.AssociationType, null);

            // A ----> R
            this.propertyByObjectByPropertyType.Set(associationId, roleType, null);
        }

        private void RemoveCompositeRoleMany2One(long associationId, IRoleType roleType)
        {
            var roleId = (long?)this.Get(associationId, roleType);
            if (roleId == null)
            {
                return;
            }

            this.RemoveCompositeRoleMany2One(associationId, roleType, roleId.Value);
        }

        private void RemoveCompositeRoleMany2One(long associationId, IRoleType roleType, long roleId)
        {
            /*                        delete
              *  RA --                                RA --
              *       -        ->                 =        -
              *   A ----- R           A --x-- R             -- R
              */
            var associationType = roleType.AssociationType;

            // A <---- R
            var roleAssociations = this.ranges.Unbox(this.Get(roleId, associationType));
            roleAssociations = this.ranges.Remove(roleAssociations, associationId);
            this.propertyByObjectByPropertyType.Set(roleId, associationType, roleAssociations);

            // A ----> R
            this.propertyByObjectByPropertyType.Set(associationId, roleType, null);
        }

        private void AddCompositesRoleOne2Many(long associationId, IRoleType roleType, long roleId)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */

            var associationType = roleType.AssociationType;
            var previousRoleIds = this.ranges.Unbox(this.Get(associationId, roleType));

            // R in PR 
            if (previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A --x-- PR
            var previousAssociationId = (long?)this.Get(roleId, associationType);
            if (previousAssociationId != default)
            {
                this.RemoveCompositesRoleOne2Many(previousAssociationId.Value, roleType, roleId);
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(roleId, associationType, roleId);

            // A ----> R
            var roleIds = this.ranges.Unbox(this.Get(associationId, roleType));
            roleIds = this.ranges.Add(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }

        private void AddCompositesRoleMany2Many(long associationId, IRoleType roleType, long roleId)
        {
            /*  [if exist]        [no remove]         set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A       PR       A --    PR       A --    PR
             */
            var associationType = roleType.AssociationType;
            var previousRoleIds = this.ranges.Unbox(this.Get(associationId, roleType));

            // R in PR 
            if (previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A <---- R
            var associationIds = this.ranges.Unbox(this.Get(roleId, associationType));
            associationIds = this.ranges.Add(associationIds, associationId);
            this.propertyByObjectByPropertyType.Set(roleId, associationType, associationIds);

            // A ----> R
            var roleIds = (IRange)this.Get(associationId, roleType);
            roleIds = this.ranges.Add(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }

        private void RemoveCompositesRoleOne2Many(long associationId, IRoleType roleType, long roleId)
        {
            var associationType = roleType.AssociationType;
            var previousRoleIds = this.ranges.Unbox(this.Get(associationId, roleType));

            // R not in PR 
            if (!previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(roleId, associationType, null);

            // A ----> R
            var roleIds = this.ranges.Unbox(this.Get(associationId, roleType));
            roleIds = this.ranges.Add(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }

        private void RemoveCompositesRoleMany2Many(long associationId, IRoleType roleType, long roleId)
        {
            var associationType = roleType.AssociationType;
            var previousRoleIds = (IRange)this.Get(associationId, roleType);

            // R not in PR 
            if (!previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A <---- R
            var associationIds = this.ranges.Unbox(this.Get(roleId, associationType));
            associationIds = this.ranges.Remove(associationIds, associationId);
            this.propertyByObjectByPropertyType.Set(roleId, associationType, associationIds);

            // A ----> R
            var roleIds = this.ranges.Unbox(this.Get(associationId, roleType));
            roleIds = this.ranges.Remove(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }
    }
}
