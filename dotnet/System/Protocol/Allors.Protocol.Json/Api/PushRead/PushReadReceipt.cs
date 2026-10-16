// <copyright file="PushReadReceipt.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    using Allors.Protocol.Json.Api.Read;

    public class PushReadReceipt
    {
        /// <summary>
        /// Request correlation ID.
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// Scope admitted for the write; required for committed outcomes.
        /// </summary>
        public ReadScope sc { get; set; }

        /// <summary>
        /// Independently validated mutation receipt.
        /// </summary>
        public PushReadOutcome push { get; set; }
    }
}
