// <copyright file="ReadFailure.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadFailure
    {
        /// <summary>
        /// Request correlation ID.
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// Server-established scope, or null if unavailable.
        /// </summary>
        public ReadScope sc { get; set; }

        /// <summary>
        /// Response kind: error.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Actionable failures. Error responses carry no successful output or cache payload.
        /// </summary>
        public ReadError[] e { get; set; }
    }
}
