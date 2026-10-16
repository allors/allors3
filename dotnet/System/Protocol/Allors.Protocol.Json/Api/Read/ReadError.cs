// <copyright file="ReadError.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadError
    {
        /// <summary>
        /// Error code.
        /// </summary>
        public string c { get; set; }

        /// <summary>
        /// Actionable diagnostic message.
        /// </summary>
        public string m { get; set; }

        /// <summary>
        /// Related object IDs, or null when not applicable.
        /// </summary>
        public long[] o { get; set; }

        /// <summary>
        /// Zero-based original selection index, or null when not applicable.
        /// </summary>
        public int? i { get; set; }
    }
}
