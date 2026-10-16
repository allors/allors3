// <copyright file="DiscoverySuccess.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class DiscoverySuccess
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
        /// Response kind: discovery.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Likely pool and its provenance. Discovery contains no readable record bodies.
        /// </summary>
        public ReadRecordHeader[] p { get; set; }

        /// <summary>
        /// Security versions referenced by this discovery's pool; not claims of client possession.
        /// </summary>
        public ReadSecurityHeader[] s { get; set; }
    }
}
