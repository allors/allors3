// <copyright file="SyncResponseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    public class SyncResponseObject
    {
        /// <summary>
        /// Id
        /// </summary>
        public long i { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public long v { get; set; }

        /// <summary>
        /// ObjectType
        /// </summary>
        public int t { get; set; }

        /// <summary>
        /// AccessControls
        /// </summary>
        public long[] a { get; set; }

        /// <summary>
        /// DeniedPermissions
        /// </summary>
        public long[] d { get; set; }

        /// <summary>
        /// Roles
        /// </summary>
        public SyncResponseRole[] r { get; set; }

        public override string ToString() => $"{this.t} [{this.i}:{this.v}]";
    }
}
