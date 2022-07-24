// <copyright file="Prefetcher.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Tracing;
    using Meta;
    using Tracing;

    internal sealed class TraceablePrefetcher : Prefetcher
    {
        private readonly ISink sink;

        public TraceablePrefetcher(Transaction transaction) : base(transaction) => this.sink = transaction.Database.Sink;

        internal override void PrefetchUnitRoles(IClass @class, HashSet<Reference> associations, IRoleType anyRoleType)
        {
            var @event = new SqlPrefetchUnitRolesEvent(this.Transaction) { Class = @class, Associations = associations?.ToArray(), RoleType = anyRoleType };
            this.sink.OnBefore(@event);

            base.PrefetchUnitRoles(@class, associations, anyRoleType);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositeRoleObjectTable(HashSet<Reference> associations, IRoleType roleType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositeRoleObjectTableEvent(this.Transaction) { Associations = associations?.ToArray(), RoleType = roleType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositeRoleObjectTable(associations, roleType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositeRoleRelationTable(HashSet<Reference> associations, IRoleType roleType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositeRoleRelationTableEvent(this.Transaction) { Associations = associations?.ToArray(), RoleType = roleType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositeRoleRelationTable(associations, roleType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositesRoleObjectTable(HashSet<Reference> associations, IRoleType roleType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositesRoleObjectTableEvent(this.Transaction) { Associations = associations?.ToArray(), RoleType = roleType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositesRoleObjectTable(associations, roleType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositesRoleRelationTable(HashSet<Reference> associations, IRoleType roleType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositesRoleRelationTableEvent(this.Transaction) { Associations = associations?.ToArray(), RoleType = roleType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositesRoleRelationTable(associations, roleType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }


        internal override void PrefetchCompositeAssociationObjectTable(HashSet<Reference> roles, IAssociationType associationType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositeAssociationObjectTableEvent(this.Transaction) { Roles = roles?.ToArray(), AssociationType = associationType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositeAssociationObjectTable(roles, associationType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositeAssociationRelationTable(HashSet<Reference> roles, IAssociationType associationType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositeAssociationRelationTableEvent(this.Transaction) { Roles = roles?.ToArray(), AssociationType = associationType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositeAssociationRelationTable(roles, associationType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositesAssociationObjectTable(HashSet<Reference> roles, IAssociationType associationType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositesAssociationObjectTableEvent(this.Transaction) { Roles = roles?.ToArray(), AssociationType = associationType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositesAssociationObjectTable(roles, associationType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }

        internal override void PrefetchCompositesAssociationRelationTable(HashSet<Reference> roles, IAssociationType associationType, HashSet<long> nestedObjectIds, HashSet<long> leafs)
        {
            var @event = new SqlPrefetchCompositesAssociationRelationTableEvent(this.Transaction) { Roles = roles?.ToArray(), AssociationType = associationType, NestedObjectIds = nestedObjectIds?.ToArray(), Leafs = leafs?.ToArray() };
            this.sink.OnBefore(@event);

            base.PrefetchCompositesAssociationRelationTable(roles, associationType, nestedObjectIds, leafs);

            this.sink.OnAfter(@event);
        }
    }
}
