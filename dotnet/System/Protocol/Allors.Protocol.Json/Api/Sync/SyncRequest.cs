// <copyright file="SyncRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using Pull;

    public class SyncRequest : Request
    {
        /// <summary>
        /// The original Pull request, including arguments and dependencies.
        /// </summary>
        public PullRequest pull { get; set; }

        /// <summary>
        /// Complete cached objects matching the first Pull response.
        /// </summary>
        public PullResponseObject[] o { get; set; }

        /// <summary>
        /// Complete cached grants [id, version], with one version per id.
        /// </summary>
        public long[][] g { get; set; }

        /// <summary>
        /// Complete cached revocations [id, version], with one version per id.
        /// </summary>
        public long[][] r { get; set; }
    }
}
