// <copyright file="ReadScope.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadScope
    {
        /// <summary>
        /// Opaque database population incarnation. Restoring or reseeding a population retires it.
        /// </summary>
        public string p { get; set; }

        /// <summary>
        /// Authenticated user ID. The server derives this identity; request fields are not authority.
        /// </summary>
        public long u { get; set; }

        /// <summary>
        /// Workspace name.
        /// </summary>
        public string w { get; set; }

        /// <summary>
        /// Opaque schema and representation revision, including tags, encodings and projection policy.
        /// </summary>
        public string r { get; set; }

        /// <summary>
        /// Context affecting selection or projection; null when absent.
        /// </summary>
        public string x { get; set; }
    }
}
