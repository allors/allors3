// <copyright file="ITrace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;
    using Meta;

    public sealed class SqlRemoveCompositeRoleEvent : Event
    {
        public SqlRemoveCompositeRoleEvent(ITransaction transaction) : base(transaction)
        {
        }

        public IRoleType RoleType { get; set; }

        public CompositeRelation[] Relations { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append(this.RoleType.Name)
            .Append(" -> #")
            .Append(this.Relations.Length)
            .Append("] ");
    }
}
