// <copyright file="PullRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    using Data;

    public class PullRequest : Request
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        public PullDependency[] d { get; set; }

        /// <summary>
        /// List of Pulls
        /// </summary>
        public Pull[] l { get; set; }

        /// <summary>
        /// Procedure
        /// </summary>
        public Procedure p { get; set; }
    }
}
