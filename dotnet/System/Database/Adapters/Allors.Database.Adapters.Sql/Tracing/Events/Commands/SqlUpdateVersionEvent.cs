// <copyright file="ITrace.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;

    public sealed class SqlUpdateVersionEvent : Event
    {
        public SqlUpdateVersionEvent(ITransaction transaction) : base(transaction)
        {
        }

        public long[] ObjectIds { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append("#")
            .Append(this.ObjectIds.Length)
            .Append("] ");
    }
}
