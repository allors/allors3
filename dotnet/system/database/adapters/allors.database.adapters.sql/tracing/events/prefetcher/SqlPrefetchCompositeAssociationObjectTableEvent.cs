// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;
    using Meta;

    public sealed class SqlPrefetchCompositeAssociationObjectTableEvent : Event
    {
        public SqlPrefetchCompositeAssociationObjectTableEvent(ITransaction transaction) : base(transaction)
        {
        }

        public Reference[] Roles { get; set; }

        public IAssociationType AssociationType { get; set; }

        public long[] NestedObjectIds { get; set; }

        public long[] Leafs { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append(this.AssociationType.Name)
            .Append(" -> #")
            .Append(this.Roles.Length)
            .Append(" -> #")
            .Append(this.NestedObjectIds.Length)
            .Append(" -> #")
            .Append(this.Leafs.Length)
            .Append("] ");
    }
}
