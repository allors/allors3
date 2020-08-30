// <copyright file="DerivationChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain.Derivations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors;
    using Allors.Meta;
    using Object = Domain.Object;

    public class AccumulatedChangeSet : IAccumulatedChangeSet
    {
        private IDictionary<IRoleType, ISet<long>> associationsByRoleType;

        private IDictionary<IAssociationType, ISet<long>> rolesByAssociationType;

        internal AccumulatedChangeSet()
        {
            this.Created = new HashSet<IStrategy>();
            this.Deleted = new HashSet<IStrategy>();
            this.Associations = new HashSet<long>();
            this.Roles = new HashSet<long>();
            this.RoleTypesByAssociation = new Dictionary<long, ISet<IRoleType>>();
            this.AssociationTypesByRole = new Dictionary<long, ISet<IAssociationType>>();
        }

        public ISet<IStrategy> Created { get; }

        public ISet<IStrategy> Deleted { get; }

        public ISet<long> Associations { get; }

        public ISet<long> Roles { get; }

        public IDictionary<long, ISet<IRoleType>> RoleTypesByAssociation { get; }

        public IDictionary<long, ISet<IAssociationType>> AssociationTypesByRole { get; }
        
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
        
        public void Add(IChangeSet changeSet)
        {
            this.Created.UnionWith(changeSet.Created);
            this.Deleted.UnionWith(changeSet.Deleted);
            this.Associations.UnionWith(changeSet.Associations);
            this.Roles.UnionWith(changeSet.Roles);

            foreach (var kvp in changeSet.RoleTypesByAssociation)
            {
                if (this.RoleTypesByAssociation.TryGetValue(kvp.Key, out var roleTypes))
                {
                    roleTypes.UnionWith(kvp.Value);
                }
                else
                {
                    this.RoleTypesByAssociation[kvp.Key] = new HashSet<IRoleType>(changeSet.RoleTypesByAssociation[kvp.Key]);
                }
            }

            foreach (var kvp in changeSet.AssociationTypesByRole)
            {
                if (this.AssociationTypesByRole.TryGetValue(kvp.Key, out var associationTypes))
                {
                    associationTypes.UnionWith(kvp.Value);
                }
                else
                {
                    this.AssociationTypesByRole[kvp.Key] = new HashSet<IAssociationType>(changeSet.AssociationTypesByRole[kvp.Key]);
                }
            }

            this.associationsByRoleType = null;
            this.rolesByAssociationType = null;
        }
    }
}
