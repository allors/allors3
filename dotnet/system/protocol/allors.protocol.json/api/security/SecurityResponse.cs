// <copyright file="SyncResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Security
{
    public class SecurityResponse
    {
        /// <summary>
        /// AccessControls
        /// </summary>
        public SecurityResponseAccessControl[] a { get; set; }

        /// <summary>
        /// AccessControls
        /// </summary>
        public SecurityResponseRestriction[] r { get; set; }

        /// <summary>
        /// Permissions
        /// </summary>
        public SecurityResponsePermission[] p { get; set; }
    }
}
