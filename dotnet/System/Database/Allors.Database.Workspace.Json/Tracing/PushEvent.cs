// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Text;
    using Allors.Protocol.Json.Api.Push;

    public class PushEvent : Event
    {
        public PushEvent(ITransaction transaction) : base(transaction) { }

        public PushRequest PushRequest { get; set; }

        public PushResponse PushResponse { get; set; }

        protected override void ToString(StringBuilder builder) => builder
            .Append(this.PushRequest.x);
    }
}
