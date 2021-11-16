// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;
    using Meta;

    public sealed class SqlGetCompositesRoleEvent : Event
    {
        public SqlGetCompositesRoleEvent(ITransaction transaction) : base(transaction)
        {
        }

        public Strategy Strategy { get; set; }

        public IRoleType RoleType { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append(this.Strategy.ObjectId)
            .Append(" : ")
            .Append(this.RoleType.Name)
            .Append("] ");
    }
}
