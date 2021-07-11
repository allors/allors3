// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
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

        public object GetUnitRole(long @object, IPropertyType propertyType) => this.propertyByObjectByPropertyType.Get(@object, propertyType);

        public void SetUnitRole(long association, IRoleType roleType, object role) => this.propertyByObjectByPropertyType.Set(association, roleType, role);

        public long? GetCompositeRole(long @object, IPropertyType propertyType) => (long?)this.propertyByObjectByPropertyType.Get(@object, propertyType);

        public void SetCompositeRole(long association, IRoleType roleType, long? newRole)
        {
            if (newRole == null)
            {
                if (roleType.AssociationType.IsOne)
                {
                    var roleId = this.GetCompositeRole(association, roleType);
                    if (roleId == null)
                    {
                        return;
                    }

                    this.RemoveCompositeRoleOne2One(association, roleType, roleId.Value);
                }
                else
                {
                    var roleId = this.GetCompositeRole(association, roleType);
                    if (roleId == null)
                    {
                        return;
                    }

                    this.RemoveCompositeRoleMany2One(association, roleType, roleId.Value);
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

        public IRange GetCompositesRole(long @object, IPropertyType propertyType) => this.ranges.Ensure(this.propertyByObjectByPropertyType.Get(@object, propertyType));

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

        public void SetCompositesRole(long association, IRoleType roleType, IRange newRole)
        {
            var previousRole = this.GetCompositesRole(association, roleType);

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

            var roleAssociation = this.GetCompositeRole(roleId, roleType.AssociationType);

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

            // Role
            this.propertyByObjectByPropertyType.Set(association, roleType, newRole);
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

        private void RemoveCompositeRoleMany2One(long associationId, IRoleType roleType, long roleId)
        {
            /*                        delete
              *  RA --                                RA --
              *       -        ->                 =        -
              *   A ----- R           A --x-- R             -- R
              */
            var associationType = roleType.AssociationType;

            // A <---- R
            var roleAssociations = this.GetCompositesRole(roleId, associationType);
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
            var previousRoleIds = this.GetCompositesRole(associationId, roleType);

            // R in PR 
            if (previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A --x-- PR
            var previousAssociationId = this.GetCompositeRole(roleId, associationType);
            if (previousAssociationId != null)
            {
                this.RemoveCompositesRoleOne2Many(previousAssociationId.Value, roleType, roleId);
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(roleId, associationType, roleId);

            // A ----> R
            var roleIds = this.GetCompositesRole(associationId, roleType);
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
            var previousRoleIds = this.GetCompositesRole(associationId, roleType);

            // R in PR 
            if (previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A <---- R
            var associationIds = this.GetCompositesRole(roleId, associationType);
            associationIds = this.ranges.Add(associationIds, associationId);
            this.propertyByObjectByPropertyType.Set(roleId, associationType, associationIds);

            // A ----> R
            var roleIds = this.GetCompositesRole(associationId, roleType);
            roleIds = this.ranges.Add(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }

        private void RemoveCompositesRoleOne2Many(long associationId, IRoleType roleType, long roleId)
        {
            var associationType = roleType.AssociationType;
            var previousRoleIds = this.GetCompositesRole(associationId, roleType);

            // R not in PR 
            if (!previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(roleId, associationType, null);

            // A ----> R
            var roleIds = this.GetCompositesRole(associationId, roleType);
            roleIds = this.ranges.Add(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }

        private void RemoveCompositesRoleMany2Many(long associationId, IRoleType roleType, long roleId)
        {
            var associationType = roleType.AssociationType;
            var previousRoleIds = this.GetCompositesRole(associationId, roleType);

            // R not in PR 
            if (!previousRoleIds.Contains(roleId))
            {
                return;
            }

            // A <---- R
            var associationIds = this.GetCompositesRole(roleId, associationType);
            associationIds = this.ranges.Remove(associationIds, associationId);
            this.propertyByObjectByPropertyType.Set(roleId, associationType, associationIds);

            // A ----> R
            var roleIds = this.GetCompositesRole(associationId, roleType);
            roleIds = this.ranges.Remove(roleIds, roleId);
            this.propertyByObjectByPropertyType.Set(associationId, roleType, roleIds);
        }
    }
}
