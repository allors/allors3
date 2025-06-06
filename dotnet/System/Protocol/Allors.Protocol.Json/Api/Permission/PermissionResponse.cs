// <copyright file="SyncResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Security
{
    public class PermissionResponse
    {
        /// <summary>
        /// Permissions
        /// </summary>
        public PermissionResponsePermission[] p { get; set; }
    }
}
