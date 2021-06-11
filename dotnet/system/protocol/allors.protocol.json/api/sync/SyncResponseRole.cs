// <copyright file="SyncResponseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using System.Diagnostics;
    
    [DebuggerDisplay("{v} [{t}]")]
    public class SyncResponseRole
    {
        /// <summary>
        /// RoleType
        /// </summary>
        public int t { get; set; }

        /// <summary>
        /// Collection
        /// </summary>
        public long[] c { get; set; }

        /// <summary>
        /// Object
        /// </summary>
        public long? o { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public object v { get; set; }
    }
}
