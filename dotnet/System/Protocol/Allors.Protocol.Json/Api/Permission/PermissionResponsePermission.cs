// <copyright file="SyncResponseObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Security
{
    public class PermissionResponsePermission
    {
        /// <summary>
        /// Id
        /// </summary>
        public long i { get; set; }

        /// <summary>
        /// Class Tag
        /// </summary>
        public string c { get; set; }

        /// <summary>
        /// Operand Type Tag
        /// </summary>
        public string t { get; set; }

        /// <summary>
        /// Operation
        /// </summary>
        public long o { get; set; }
    }
}
