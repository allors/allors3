// <copyright file="SyncRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    public class SyncRequest : Request
    {
        /// <summary>
        /// Objects
        /// </summary>
        public long[] o { get; set; }
    }
}
