// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    using System.Collections.Generic;
    using System.Text;
    using Adapters.Tracing;

    public sealed class SqlGetVersionsEvent : Event
    {
        public SqlGetVersionsEvent(ITransaction transaction) : base(transaction)
        {
        }

        public ISet<Reference> References { get; set; }

        protected override void ToString(StringBuilder builder) => _ = builder
            .Append('[')
            .Append("#")
            .Append(this.References.Count)
            .Append("] ");
    }
}
