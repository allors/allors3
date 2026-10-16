// <copyright file="PushReadResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.PushRead
{
    public class PushReadResponse : PushReadReceipt
    {
        /// <summary>
        /// Requested changed-object outcome: PushReadChanged or ReadFailure.
        /// </summary>
        public object changed { get; set; }

        /// <summary>
        /// Requested discovery outcome: DiscoverySuccess or ReadFailure.
        /// </summary>
        public object discovery { get; set; }
    }
}
