// <copyright file="PullRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    public class PullDependency
    {
        /// <summary>
        /// ObjectType
        /// </summary>
        public string o { get; set; }

        /// <summary>
        /// AssociationType
        /// </summary>
        public string a { get; set; }

        /// <summary>
        /// RoleType
        /// </summary>
        public string r { get; set; }
    }
}
