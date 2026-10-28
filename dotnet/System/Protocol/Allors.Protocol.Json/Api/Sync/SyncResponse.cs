// <copyright file="SyncResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using Security;

    public class SyncResponse
    {
        /// <summary>
        /// Objects
        /// </summary>
        public SyncResponseObject[] o { get; set; }

        /// <summary>
        /// Grants used to filter the objects. Null permission IDs mean the exact entry is cached;
        /// an empty array means the complete entry has no permissions.
        /// </summary>
        public AccessResponseGrant[] g { get; set; }

        /// <summary>
        /// Revocations used to filter the objects, with the same permission-content rules as grants.
        /// </summary>
        public AccessResponseRevocation[] r { get; set; }

        /// <summary>
        /// Definitions of every permission referenced by the included access-entry contents.
        /// </summary>
        public PermissionResponsePermission[] p { get; set; }
    }
}
