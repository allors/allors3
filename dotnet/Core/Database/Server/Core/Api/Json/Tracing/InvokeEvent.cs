// <copyright file="DatabaseController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Text;
    using Allors.Protocol.Json.Api.Invoke;

    public class InvokeEvent : Event
    {
        public InvokeEvent(ITransaction transaction) : base(transaction) { }

        public InvokeRequest InvokeRequest { get; set; }

        public InvokeResponse InvokeResponse { get; set; }

        protected override void ToString(StringBuilder builder) => builder
            .Append(this.InvokeRequest.x);
    }
}
