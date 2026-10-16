// <copyright file="PushReadRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    using Allors.Protocol.Json.Api.Read;

    public class PushReadRequest
    {
        /// <summary>
        /// Protocol revision 1.
        /// </summary>
        public int v { get; set; }

        /// <summary>
        /// Request correlation ID.
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// Required, already bound scope admitted for the write.
        /// </summary>
        public ReadScope sc { get; set; }

        /// <summary>
        /// Captured context; only the outer request carries it.
        /// </summary>
        public string x { get; set; }

        /// <summary>
        /// Changes to commit in one attempt.
        /// </summary>
        public PushReadChanges push { get; set; }

        /// <summary>
        /// Request full affected records; absent or null means false.
        /// </summary>
        public bool? returnChanged { get; set; }

        /// <summary>
        /// Optional query discovery after commit.
        /// </summary>
        public ReadPull discover { get; set; }
    }
}
