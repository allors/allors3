// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Meta;

    public sealed class Event
    {
        public Event(Transaction transaction, EventKind kind)
        {
            this.Transaction = transaction;
            this.Kind = kind;
            this.Started = DateTime.Now;
        }

        public Transaction Transaction { get; }

        public EventKind Kind { get; }

        public EventSource Source
        {
            get
            {
                switch (this.Kind)
                {
                    case EventKind.CommandsDeleteObject:
                    case EventKind.CommandsGetUnitRoles:
                    case EventKind.CommandsSetUnitRoles:
                    case EventKind.CommandsGetCompositeRole:
                    case EventKind.CommandsSetCompositeRole:
                    case EventKind.CommandsGetCompositesRole:
                    case EventKind.CommandsAddCompositeRole:
                    case EventKind.CommandsRemoveCompositeRole:
                    case EventKind.CommandsClearCompositeAndCompositesRole:
                    case EventKind.CommandsGetCompositeAssociation:
                    case EventKind.CommandsGetCompositesAssociation:
                    case EventKind.CommandsCreateObject:
                    case EventKind.CommandsCreateObjects:
                    case EventKind.CommandsInstantiateObject:
                    case EventKind.CommandsInstantiateReferences:
                    case EventKind.CommandsGetVersions:
                    case EventKind.CommandsUpdateVersion:
                        return EventSource.Commands;

                    case EventKind.PrefetcherPrefetchUnitRoles:
                    case EventKind.PrefetcherPrefetchCompositeRoleObjectTable:
                    case EventKind.PrefetcherPrefetchCompositeRoleRelationTable:
                    case EventKind.PrefetcherPrefetchCompositesRoleObjectTable:
                    case EventKind.PrefetcherPrefetchCompositesRoleRelationTable:
                    case EventKind.PrefetcherPrefetchCompositeAssociationObjectTable:
                    case EventKind.PrefetcherPrefetchCompositeAssociationRelationTable:
                    case EventKind.PrefetcherPrefetchCompositesAssociationObjectTable:
                    case EventKind.PrefetcherPrefetchCompositesAssociationRelationTable:
                        return EventSource.Prefetcher;

                    default:
                        throw new Exception($"{this.Kind} has an unknown source");
                }
            }
        }

        public DateTime Started { get; }

        public DateTime Stopped { get; private set; }

        public TimeSpan Duration => this.Stopped - this.Started;

        public IRoleType RoleType { get; set; }

        public IRoleType[] RoleTypes { get; set; }

        public IAssociationType AssociationType { get; set; }

        public IClass Class { get; set; }

        public Strategy Strategy { get; set; }

        public CompositeRelation[] Relations { get; set; }

        public Reference[] Associations { get; set; }

        public long[] AssociationIds { get; set; }

        public Reference[] Roles { get; set; }

        public Reference Role { get; set; }

        public int? Count { get; set; }

        public long? ObjectId { get; set; }

        public long[] ObjectIds { get; set; }

        public ISet<Reference> References { get; set; }

        public long[] NestedObjectIds { get; set; }

        public long[] Leafs { get; set; }

        public Event Stop()
        {
            this.Stopped = DateTime.Now;
            return this;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append('[');

            if (this.AssociationType != null)
            {
                builder.Append(this.AssociationType.Name);
            }

            if (this.RoleType != null)
            {
                builder.Append(this.RoleType.Name);
            }

            if (this.RoleTypes?.Length > 0)
            {
                builder.Append(string.Join(", ", this.RoleTypes.Select(v => v.Name)));
            }

            builder
                .Append("] -> ")
                .Append(this.Kind);


            if (this.Duration > new TimeSpan(0, 0, 0, 0, 1))
            {
                builder.Append(" (")
                    .Append(this.Duration.ToString("s\\.fff"))
                    .Append("s)");
            }

            return builder.ToString();
        }
    }
}
