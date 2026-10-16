// <copyright file="ReadSecurityPayload.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadSecurityPayload
    {
        /// <summary>
        /// Complete permission-ID body. An empty array is a body; null means reuse. Exactly one of p and r is non-null.
        /// </summary>
        public long[] p { get; set; }

        /// <summary>
        /// Zero-based request security index for exact reuse, or null when p supplies the body. The advertised row must have h true.
        /// </summary>
        public int? r { get; set; }
    }
}
