// <copyright file="DerivationErrorResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api
{
    public class ResponseDerivationError
    {
        /// <summary>
        /// ErrorMessage
        /// </summary>
        public string e { get; set; }

        /// <summary>
        /// Roles
        /// [][AssociationId, RelationTypeId]
        /// </summary>
        public long[][] r { get; set; }
    }
}
