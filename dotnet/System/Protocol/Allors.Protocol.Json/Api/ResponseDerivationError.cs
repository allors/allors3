// <copyright file="DerivationErrorResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api
{
    public class ResponseDerivationError
    {
        /// <summary>
        /// Message
        /// </summary>
        public string m { get; set; }

        /// <summary>
        /// Relations
        /// </summary>
        public DerivationRelation[] r { get; set; }
    }
}
