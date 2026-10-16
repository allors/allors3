// <copyright file="ReadCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadCache
    {
        /// <summary>
        /// Security provenance and body possession, indexed by records in this request. Different historical versions have separate rows.
        /// </summary>
        public ReadSecurityHint[] s { get; set; }

        /// <summary>
        /// Retained and pinned record candidates. Their security indexes refer to this request's security table.
        /// </summary>
        public ReadRecordHeader[] r { get; set; }

        /// <summary>
        /// IDs of retained and pinned permission descriptors.
        /// </summary>
        public long[] p { get; set; }
    }
}
