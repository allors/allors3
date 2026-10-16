// <copyright file="ReadInput.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadInput
    {
        /// <summary>
        /// Zero-based index of the original selection.
        /// </summary>
        public int i { get; set; }

        /// <summary>
        /// Explicit target ID, or null when the selection has no explicit target.
        /// </summary>
        public long? o { get; set; }

        /// <summary>
        /// Input status: resolved or unavailable. Resolved may still produce an empty result.
        /// </summary>
        public string s { get; set; }
    }
}
