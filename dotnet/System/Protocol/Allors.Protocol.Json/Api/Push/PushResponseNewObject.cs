// <copyright file="PushResponseNewObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Push
{
    public class PushResponseNewObject
    {
        /// <summary>
        /// WorkspaceId
        /// </summary>
        public long w { get; set; }

        /// <summary>
        /// DatabaseId
        /// </summary>
        public long d { get; set; }
    }
}
