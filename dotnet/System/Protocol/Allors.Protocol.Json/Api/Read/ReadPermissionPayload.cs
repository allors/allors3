// <copyright file="ReadPermissionPayload.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadPermissionPayload
    {
        /// <summary>
        /// Class tag of a full descriptor; null for reuse.
        /// </summary>
        public string c { get; set; }

        /// <summary>
        /// Operation code of a full descriptor: Create 1, Read 2, Write 4 or Execute 8. Null for reuse.
        /// </summary>
        public long? o { get; set; }

        /// <summary>
        /// Relation tag for Read/Write or method tag for Execute. Null for Create and reuse.
        /// </summary>
        public string t { get; set; }

        /// <summary>
        /// Zero-based request permission index for exact reuse. Null for a full descriptor; when present c, o and t are null.
        /// </summary>
        public int? r { get; set; }
    }
}
