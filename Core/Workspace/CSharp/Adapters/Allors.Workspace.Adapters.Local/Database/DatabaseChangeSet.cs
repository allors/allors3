// <copyright file="ChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class DatabaseChangeSet
    {
        private static readonly HashSet<IRoleType> EmptyRoleTypeSet = new HashSet<IRoleType>();
        private static readonly HashSet<IAssociationType> EmptyAssociationTypeSet = new HashSet<IAssociationType>();

        private readonly HashSet<IStrategy> deleted;

        private readonly HashSet<long> associations;
        private readonly HashSet<long> roles;

        private readonly Dictionary<long, ISet<IRoleType>> roleTypesByAssociation;
        private readonly Dictionary<long, ISet<IAssociationType>> associationTypesByRole;

        private IDictionary<IRoleType, ISet<long>> associationsByRoleType;
        private IDictionary<IAssociationType, ISet<long>> rolesByAssociationType;

        internal DatabaseChangeSet()
        {
            this.deleted = new HashSet<IStrategy>();
            this.associations = new HashSet<long>();
            this.roles = new HashSet<long>();
            this.roleTypesByAssociation = new Dictionary<long, ISet<IRoleType>>();
            this.associationTypesByRole = new Dictionary<long, ISet<IAssociationType>>();
        }

        public ISet<IStrategy> Deleted => this.deleted;

        public ISet<long> Associations => this.associations;

        public ISet<long> Roles => this.roles;

        public IDictionary<long, ISet<IRoleType>> RoleTypesByAssociation => this.roleTypesByAssociation;

        public IDictionary<long, ISet<IAssociationType>> AssociationTypesByRole => this.associationTypesByRole;

        public IDictionary<IRoleType, ISet<long>> AssociationsByRoleType => this.associationsByRoleType ??=
            (from kvp in this.RoleTypesByAssociation
             from value in kvp.Value
             group kvp.Key by value)
                 .ToDictionary(grp => grp.Key, grp => new HashSet<long>(grp) as ISet<long>);

        public IDictionary<IAssociationType, ISet<long>> RolesByAssociationType => this.rolesByAssociationType ??=
            (from kvp in this.AssociationTypesByRole
             from value in kvp.Value
             group kvp.Key by value)
                   .ToDictionary(grp => grp.Key, grp => new HashSet<long>(grp) as ISet<long>);

        internal void OnDeleted(IStrategy strategy) => this.deleted.Add(strategy);

        internal void OnChangingUnitRole(long association, IRoleType roleType)
        {
            this.associations.Add(association);

            this.RoleTypes(association).Add(roleType);
        }

        internal void OnChangingCompositeRole(long association, IRoleType roleType, long? previousRole, long? newRole)
        {
            this.associations.Add(association);

            if (previousRole != null)
            {
                this.roles.Add(previousRole.Value);
                this.AssociationTypes(previousRole.Value).Add(roleType.AssociationType);
            }

            if (newRole != null)
            {
                this.roles.Add(newRole.Value);
                this.AssociationTypes(newRole.Value).Add(roleType.AssociationType);
            }

            this.RoleTypes(association).Add(roleType);
        }

        internal void OnChangingCompositesRole(long association, IRoleType roleType, Strategy changedRole)
        {
            this.associations.Add(association);

            if (changedRole != null)
            {
                this.roles.Add(changedRole.WorkspaceId);
                this.AssociationTypes(changedRole.WorkspaceId).Add(roleType.AssociationType);
            }

            this.RoleTypes(association).Add(roleType);
        }

        private ISet<IRoleType> RoleTypes(long associationId)
        {
            if (!this.RoleTypesByAssociation.TryGetValue(associationId, out var roleTypes))
            {
                roleTypes = new HashSet<IRoleType>();
                this.RoleTypesByAssociation[associationId] = roleTypes;
            }

            return roleTypes;
        }

        private ISet<IAssociationType> AssociationTypes(long roleId)
        {
            if (!this.AssociationTypesByRole.TryGetValue(roleId, out var associationTypes))
            {
                associationTypes = new HashSet<IAssociationType>();
                this.AssociationTypesByRole[roleId] = associationTypes;
            }

            return associationTypes;
        }
    }
}
