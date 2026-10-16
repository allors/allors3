// <copyright file="ReadRecordPayload.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    using Allors.Protocol.Json.Api.Sync;

    public class ReadRecordPayload
    {
        /// <summary>
        /// Complete readable role body. An empty array is a body; null means reuse. Exactly one of ro and r is non-null.
        /// </summary>
        public SyncResponseRole[] ro { get; set; }

        /// <summary>
        /// Zero-based request record index for exact reuse, or null when ro supplies the body. Zero is a valid index.
        /// </summary>
        public int? r { get; set; }
    }
}
