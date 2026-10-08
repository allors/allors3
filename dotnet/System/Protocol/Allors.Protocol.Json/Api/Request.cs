// <copyright file="ErrorResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api
{
    public abstract class Request
    {
        /// <summary>
        /// A tracing string the server appends to its events.
        /// </summary>
        public string x { get; set; }

        /// <summary>
        /// The name of the workspace the client is built for; the server refuses a name that
        /// differs from the one it serves. Null when the client does not say.
        /// </summary>
        public string _w { get; set; }

        /// <summary>
        /// The fingerprint of the client's workspace meta; the server refuses a fingerprint
        /// that differs from its own. Null when the client does not say.
        /// </summary>
        public string _f { get; set; }
    }
}
