// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Ranges;

    public class SessionOriginState
    {
        private readonly PropertyByObjectByPropertyType propertyByObjectByPropertyType;

        public SessionOriginState() => this.propertyByObjectByPropertyType = new PropertyByObjectByPropertyType();

        public void Checkpoint(ChangeSet changeSet) => changeSet.AddSessionStateChanges(this.propertyByObjectByPropertyType.Checkpoint());

        public object GetUnitRole(Strategy association, IPropertyType propertyType) => this.propertyByObjectByPropertyType.Get(association, propertyType);

        public void SetUnitRole(Strategy association, IRoleType roleType, object role) => this.propertyByObjectByPropertyType.Set(association, roleType, role);

        public Strategy GetCompositeRole(Strategy association, IRoleType propertyType) => this.GetCompositeProperty(association, propertyType);

        public void SetCompositeRole(Strategy association, IRoleType roleType, Strategy newRole)
        {
            if (newRole == null)
            {
                var role = this.GetCompositeProperty(association, roleType);
                if (role == null)
                {
                    return;
                }

                if (roleType.AssociationType.IsOne)
                {
                    this.RemoveCompositeRoleOne2One(association, roleType, role);
                }
                else
                {

                    this.RemoveCompositeRoleMany2One(association, roleType, role);
                }
            }
            else if (roleType.AssociationType.IsOne)
            {
                this.SetCompositeRoleOne2One(association, roleType, newRole);
            }
            else
            {
                this.SetCompositeRoleMany2One(association, roleType, newRole);
            }
        }

        public IEnumerable<Strategy> GetCompositesRole(Strategy association, IRoleType propertyType) => this.GetCompositesProperty(association, propertyType) ?? (IEnumerable<Strategy>)Array.Empty<Strategy>();

        public void AddCompositesRole(Strategy association, IRoleType roleType, Strategy item)
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

        public void RemoveCompositesRole(Strategy association, IRoleType roleType, Strategy item)
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

        public void SetCompositesRole(Strategy association, IRoleType roleType, IEnumerable<Strategy> newRole)
        {
            var roles = newRole as ICollection<Strategy> ?? newRole?.ToArray();
            var previousRole = this.GetCompositesProperty(association, roleType);

            if (previousRole == null)
            {
                if (!(roles?.Count > 0))
                {
                    return;
                }

                foreach (var role in roles)
                {
                    this.AddCompositesRole(association, roleType, role);
                }

                return;
            }

            if (!(roles?.Count > 0))
            {
                foreach (var roleToRemove in previousRole.ToArray())
                {
                    this.RemoveCompositesRole(association, roleType, roleToRemove);
                }

                return;
            }

            foreach (var role in roles)
            {
                if (!previousRole.Contains(role))
                {
                    this.AddCompositesRole(association, roleType, role);
                }
            }

            foreach (var removedRole in previousRole.Except(roles))
            {
                this.RemoveCompositesRole(association, roleType, removedRole);
            }
        }

        public Strategy GetCompositeAssociation(Strategy association, IAssociationType propertyType) => this.GetCompositeProperty(association, propertyType);

        public IEnumerable<Strategy> GetCompositesAssociation(Strategy role, IAssociationType propertyType) => this.GetCompositesProperty(role, propertyType) ?? (IEnumerable<Strategy>)Array.Empty<Strategy>();

        private void SetCompositeRoleOne2One(Strategy associationId, IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var associationType = roleType.AssociationType;
            var previousRole = (Strategy)this.propertyByObjectByPropertyType.Get(associationId, roleType);

            // R = PR
            if (Equals(role, previousRole))
            {
                return;
            }

            // A --x-- PR
            if (previousRole != null)
            {
                this.RemoveCompositeRoleOne2One(associationId, roleType, previousRole);
            }

            var roleAssociation = this.GetCompositeProperty(role, roleType.AssociationType);

            // RA --x-- R
            if (roleAssociation != null)
            {
                this.RemoveCompositeRoleOne2One(roleAssociation, roleType, role);
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(role, associationType, associationId);

            // A ----> R
            this.propertyByObjectByPropertyType.Set(associationId, roleType, role);
        }

        private void SetCompositeRoleMany2One(Strategy association, IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var associationType = roleType.AssociationType;
            var previousRole = (Strategy)this.propertyByObjectByPropertyType.Get(association, roleType);

            // R = PR
            if (Equals(role, previousRole))
            {
                return;
            }

            // A --x-- PR
            if (previousRole != null)
            {
                this.RemoveCompositeRoleMany2One(association, roleType, role);
            }

            // A <---- R
            var associations = this.EnsureCompositesProperty(role, associationType);
            associations.Add(association);

            // A ----> R
            this.propertyByObjectByPropertyType.Set(association, roleType, role);

            association.Session.ChangeSetTracker.OnSessionChanged(this);
        }

        private void RemoveCompositeRoleOne2One(Strategy associationId, IRoleType roleType, Strategy roleId)
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

        private void RemoveCompositeRoleMany2One(Strategy association, IRoleType roleType, Strategy roleId)
        {
            /*                        delete
              *  RA --                                RA --
              *       -        ->                 =        -
              *   A ----- R           A --x-- R             -- R
              */
            var associationType = roleType.AssociationType;

            // A <---- R
            var roleAssociations = this.EnsureCompositesProperty(roleId, associationType);
            roleAssociations.Remove(association);

            // A ----> R
            this.propertyByObjectByPropertyType.Set(association, roleType, null);
        }

        private void AddCompositesRoleOne2Many(Strategy association, IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */

            var associationType = roleType.AssociationType;
            var previousRoles = this.GetCompositesProperty(association, roleType);

            // R in PR 
            if (previousRoles?.Contains(role) == true)
            {
                return;
            }

            // A --x-- PR
            var previousAssociation = this.GetCompositeProperty(role, associationType);
            if (previousAssociation != null)
            {
                this.RemoveCompositesRoleOne2Many(previousAssociation, roleType, role);
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(role, associationType, association);

            // A ----> R
            var roles = this.EnsureCompositesProperty(association, roleType);
            roles.Add(role);
        }

        private void AddCompositesRoleMany2Many(Strategy association, IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [no remove]         set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A       PR       A --    PR       A ----- PR
             */
            var associationType = roleType.AssociationType;
            var previousRoles = this.GetCompositesProperty(association, roleType);

            // R in PR 
            if (previousRoles?.Contains(role) == true)
            {
                return;
            }

            // A <---- R
            var associations = this.EnsureCompositesProperty(role, associationType);
            associations.Add(association);

            // A ----> R
            var roles = this.EnsureCompositesProperty(association, roleType);
            roles.Add(role);
        }

        private void RemoveCompositesRoleOne2Many(Strategy association, IRoleType roleType, Strategy role)
        {
            var associationType = roleType.AssociationType;
            var roles = this.GetCompositesProperty(association, roleType);

            // R not in PR 
            if (roles == null || !roles.Contains(role))
            {
                return;
            }

            // A <---- R
            this.propertyByObjectByPropertyType.Set(role, associationType, null);

            // A ----> R
            roles.Remove(role);
        }

        private void RemoveCompositesRoleMany2Many(Strategy association, IRoleType roleType, Strategy role)
        {
            var associationType = roleType.AssociationType;
            var roles = this.GetCompositesProperty(association, roleType);

            // R not in PR 
            if (roles == null || !roles.Contains(role))
            {
                return;
            }

            // A <---- R
            var associations = this.GetCompositesProperty(role, associationType);
            associations.Remove(association);

            // A ----> R
            roles.Remove(role);
        }

        private Strategy GetCompositeProperty(Strategy association, IPropertyType propertyType) => (Strategy)this.propertyByObjectByPropertyType.Get(association, propertyType);

        private ISet<Strategy> GetCompositesProperty(Strategy @object, IPropertyType propertyType) => (ISet<Strategy>)this.propertyByObjectByPropertyType.Get(@object, propertyType);

        private ISet<Strategy> EnsureCompositesProperty(Strategy @object, IPropertyType propertyType)
        {
            var properties = (ISet<Strategy>)this.propertyByObjectByPropertyType.Get(@object, propertyType);
            if (properties == null)
            {
                properties = new HashSet<Strategy>();
                this.propertyByObjectByPropertyType.Set(@object, propertyType, properties);
            }

            return properties;
        }
    }
}
