// <copyright file="ReadSecurityHeader.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadSecurityHeader
    {
        /// <summary>
        /// Security kind: grant or revocation.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Positive security object ID.
        /// </summary>
        public long i { get; set; }

        /// <summary>
        /// Committed security version.
        /// </summary>
        public long v { get; set; }
    }
}
