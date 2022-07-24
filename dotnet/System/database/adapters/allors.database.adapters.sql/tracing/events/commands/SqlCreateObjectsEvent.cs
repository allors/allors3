// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;
    using Meta;

    public sealed class SqlCreatesObjectEvent : Event
    {
        public SqlCreatesObjectEvent(ITransaction transaction) : base(transaction)
        {
        }

        public IClass Class { get; set; }

        public int? Count { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append(this.Class.Name)
            .Append(" -> #")
            .Append(this.Count)
            .Append("] ");
    }
}
