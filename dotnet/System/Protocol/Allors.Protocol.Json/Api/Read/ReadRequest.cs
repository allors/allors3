// <copyright file="ReadRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Read
{
    public class ReadRequest
    {
        /// <summary>
        /// Read protocol revision.
        /// </summary>
        public int v { get; set; }

        /// <summary>
        /// Request correlation ID, echoed by every response.
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// Expected server scope. May be omitted or null only when no cache hints are sent.
        /// </summary>
        public ReadScope sc { get; set; }

        /// <summary>
        /// Captured request context; null when absent.
        /// </summary>
        public string x { get; set; }

        /// <summary>
        /// Phase: discover or read.
        /// </summary>
        public string k { get; set; }

        /// <summary>
        /// Original declarative selections and configured dependencies.
        /// </summary>
        public ReadPull p { get; set; }

        /// <summary>
        /// Actually retained cache candidates, or null for a cold request.
        /// </summary>
        public ReadCache h { get; set; }
    }
}
