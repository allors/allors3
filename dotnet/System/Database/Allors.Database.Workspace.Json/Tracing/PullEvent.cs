// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Text;
    using Allors.Protocol.Json.Api.Pull;

    public class PullEvent : Event
    {
        public PullEvent(ITransaction transaction) : base(transaction) { }

        public PullRequest PullRequest { get; set; }

        public PullResponse PullResponse { get; set; }

        protected override void ToString(StringBuilder builder) => builder
            .Append(this.PullRequest.x);
    }
}
