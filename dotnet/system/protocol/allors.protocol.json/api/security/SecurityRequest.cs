// <copyright file="SyncRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Security
{
    public class SecurityRequest
    {
        /// <summary>
        /// Grants
        /// </summary>
        public long[] a { get; set; }

        /// <summary>
        /// Revocations
        /// </summary>
        public long[] r { get; set; }

        /// <summary>
        /// Permissions
        /// </summary>
        public long[] p { get; set; }
    }
}
