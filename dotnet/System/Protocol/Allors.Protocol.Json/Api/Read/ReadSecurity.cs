// <copyright file="ReadSecurity.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadSecurity : ReadSecurityHeader
    {
        /// <summary>
        /// Permission-ID body or a reuse reference to the request's security inventory.
        /// </summary>
        public ReadSecurityPayload p { get; set; }
    }
}
