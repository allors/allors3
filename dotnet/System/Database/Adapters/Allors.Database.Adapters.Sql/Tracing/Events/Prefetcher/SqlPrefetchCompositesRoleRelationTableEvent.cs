// <copyright file="ITrace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;
    using Meta;

    public sealed class SqlPrefetchCompositesRoleRelationTableEvent : Event
    {
        public SqlPrefetchCompositesRoleRelationTableEvent(ITransaction transaction) : base(transaction)
        {
        }

        public Reference[] Associations { get; set; }

        public IRoleType RoleType { get; set; }

        public long[] NestedObjectIds { get; set; }

        public long[] Leafs { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append(this.RoleType.Name)
            .Append(" -> #")
            .Append(this.Associations.Length)
            .Append(" -> #")
            .Append(this.NestedObjectIds.Length)
            .Append(" -> #")
            .Append(this.Leafs.Length)
            .Append("] ");
    }
}
