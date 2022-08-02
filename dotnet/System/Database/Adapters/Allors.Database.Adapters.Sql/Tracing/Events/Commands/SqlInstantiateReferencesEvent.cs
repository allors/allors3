// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Text;
    using Adapters.Tracing;

    public sealed class SqlInstantiateReferencesEvent : Event
    {
        public SqlInstantiateReferencesEvent(ITransaction transaction) : base(transaction)
        {
        }

        public long[] ObjectIds { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append('#')
            .Append(this.ObjectIds.Length)
            .Append("] ");
    }
}
