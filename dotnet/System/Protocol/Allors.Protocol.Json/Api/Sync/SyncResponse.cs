// <copyright file="SyncResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using Security;

    public class SyncResponse : Response
    {
        public SyncResponseResult result { get; set; }

        /// <summary>
        /// Objects
        /// </summary>
        public SyncResponseObject[] o { get; set; }

        public AccessResponseGrant[] g { get; set; }

        public AccessResponseRevocation[] r { get; set; }

        public PermissionResponsePermission[] p { get; set; }
    }
}
