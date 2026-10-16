// <copyright file="ReadPermission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadPermission
    {
        /// <summary>
        /// Positive permission ID.
        /// </summary>
        public long i { get; set; }

        /// <summary>
        /// Permission descriptor or a reuse reference to the request's permission inventory.
        /// </summary>
        public ReadPermissionPayload p { get; set; }
    }
}
