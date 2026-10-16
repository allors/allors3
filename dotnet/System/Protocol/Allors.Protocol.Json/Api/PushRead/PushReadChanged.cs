// <copyright file="PushReadChanged.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    using Allors.Protocol.Json.Api.Read;

    public class PushReadChanged
    {
        /// <summary>
        /// Request correlation ID.
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// Protected post-commit snapshot scope.
        /// </summary>
        public ReadScope sc { get; set; }

        /// <summary>
        /// Read outcome kind: changed.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Full authorized records, without reuse references.
        /// </summary>
        public ReadRecord[] p { get; set; }

        /// <summary>
        /// Full security bodies required by these records.
        /// </summary>
        public ReadSecurity[] s { get; set; }

        /// <summary>
        /// Full permission descriptors required by these security bodies.
        /// </summary>
        public ReadPermission[] pm { get; set; }

        /// <summary>
        /// Eligible affected IDs unavailable in the post-commit snapshot.
        /// </summary>
        public long[] u { get; set; }
    }
}
