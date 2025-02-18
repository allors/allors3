// <copyright file="PullResponseObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    public class PullResponseObject
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
        /// Sorted Grants
        /// </summary>
        public long[] g { get; set; }

        /// <summary>
        /// Sorted Revocations
        /// </summary>
        public long[] r { get; set; }
    }
}
