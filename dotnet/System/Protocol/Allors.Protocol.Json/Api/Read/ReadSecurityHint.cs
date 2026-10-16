// <copyright file="ReadSecurityHint.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadSecurityHint : ReadSecurityHeader
    {
        /// <summary>
        /// Whether the client retains and pins this exact version's permission-ID body. False still allows provenance references.
        /// </summary>
        public bool h { get; set; }
    }
}
