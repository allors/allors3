// <copyright file="ChangeLog.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class ChangeLog
    {
        private readonly HashSet<IStrategy> created;
        private readonly HashSet<IStrategy> deleted;

        private readonly Dictionary<IStrategy, ISet<IRoleType>> roleTypesByAssociation;
        private readonly Dictionary<IStrategy, ISet<IAssociationType>> associationTypesByRole;

        private readonly Dictionary<IStrategy, Original> originalByStrategy;

        internal ChangeLog()
        {
            this.created = new HashSet<IStrategy>();
            this.deleted = new HashSet<IStrategy>();

            this.roleTypesByAssociation = new Dictionary<IStrategy, ISet<IRoleType>>();
            this.associationTypesByRole = new Dictionary<IStrategy, ISet<IAssociationType>>();

            this.originalByStrategy = new Dictionary<IStrategy, Original>();
        }

        public IEnumerable<IStrategy> Created => this.created;

        public IEnumerable<IStrategy> Deleted => this.deleted;

        public IEnumerable<IStrategy> Associations => this.roleTypesByAssociation.Keys;

        public IEnumerable<IStrategy> Roles => this.associationTypesByRole.Keys;

        public IDictionary<IStrategy, ISet<IRoleType>> RoleTypesByAssociation => this.roleTypesByAssociation;

        public IDictionary<IStrategy, ISet<IAssociationType>> AssociationTypesByRole => this.associationTypesByRole;

        internal void OnCreated(Strategy strategy) => this.created.Add(strategy);

        internal void OnDeleted(Strategy strategy) => this.deleted.Add(strategy);

        internal void OnChangingUnitRole(Strategy association, IRoleType roleType, object previousRole)
        {
            this.Original(association).OnChangingUnitRole(roleType, previousRole);

            _ = this.RoleTypes(association).Add(roleType);
        }

        internal void OnChangingCompositeRole(Strategy association, IRoleType roleType, Strategy previousRole, Strategy newRole)
        {
            this.Original(association).OnChangingCompositeRole(roleType, previousRole);

            if (previousRole != null)
            {
                _ = this.AssociationTypes(previousRole).Add(roleType.AssociationType);
            }

            if (newRole != null)
            {
                _ = this.AssociationTypes(newRole).Add(roleType.AssociationType);
            }

            _ = this.RoleTypes(association).Add(roleType);
        }

        internal void OnChangingCompositesRole(Strategy association, IRoleType roleType, Strategy changedRole, IEnumerable<Strategy> previousRoleStrategies)
        {
            this.Original(association).OnChangingCompositesRole(roleType, previousRoleStrategies);

            if (changedRole != null)
            {
                _ = this.AssociationTypes(changedRole).Add(roleType.AssociationType);
            }

            _ = this.RoleTypes(association).Add(roleType);
        }

        private ISet<IRoleType> RoleTypes(Strategy associationId)
        {
            if (!this.RoleTypesByAssociation.TryGetValue(associationId, out var roleTypes))
            {
                roleTypes = new HashSet<IRoleType>();
                this.RoleTypesByAssociation[associationId] = roleTypes;
            }

            return roleTypes;
        }

        private ISet<IAssociationType> AssociationTypes(Strategy roleId)
        {
            if (!this.AssociationTypesByRole.TryGetValue(roleId, out var associationTypes))
            {
                associationTypes = new HashSet<IAssociationType>();
                this.AssociationTypesByRole[roleId] = associationTypes;
            }

            return associationTypes;
        }

        private Original Original(Strategy association)
        {
            if (!this.originalByStrategy.TryGetValue(association, out var original))
            {
                original = new Original(association);
            }

            ;
            return original;
        }
    }
}
