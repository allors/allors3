// <copyright file="ReadSuccess.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    using System.Collections.Generic;

    public class ReadSuccess
    {
        /// <summary>
        /// Request correlation ID.
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// Server-established read scope.
        /// </summary>
        public ReadScope sc { get; set; }

        /// <summary>
        /// Response kind: complete.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Outcome for each original selection.
        /// </summary>
        public ReadInput[] l { get; set; }

        /// <summary>
        /// Named collections, including declared empty collections.
        /// </summary>
        public IDictionary<string, long[]> c { get; set; }

        /// <summary>
        /// Named objects. An unavailable named object is explicitly null.
        /// </summary>
        public IDictionary<string, long?> o { get; set; }

        /// <summary>
        /// Final pool. Every record supplies its readable roles or confirms an exact request candidate.
        /// </summary>
        public ReadRecord[] p { get; set; }

        /// <summary>
        /// Complete security closure of the pool, with full bodies or exact request reuse references.
        /// </summary>
        public ReadSecurity[] s { get; set; }

        /// <summary>
        /// Permission descriptors required by the security closure, with full bodies or exact request reuse references.
        /// </summary>
        public ReadPermission[] pm { get; set; }
    }
}
