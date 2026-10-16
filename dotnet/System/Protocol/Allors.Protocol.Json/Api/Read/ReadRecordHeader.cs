// <copyright file="ReadRecordHeader.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadRecordHeader
    {
        /// <summary>
        /// Positive database object ID.
        /// </summary>
        public long i { get; set; }

        /// <summary>
        /// Class tag.
        /// </summary>
        public string c { get; set; }

        /// <summary>
        /// Committed data version.
        /// </summary>
        public long v { get; set; }

        /// <summary>
        /// Complete grant membership, as zero-based indexes in the containing message's security table. An empty set is explicit.
        /// </summary>
        public int[] g { get; set; }

        /// <summary>
        /// Complete revocation membership, as zero-based indexes in the containing message's security table. An empty set is explicit.
        /// </summary>
        public int[] r { get; set; }
    }
}
