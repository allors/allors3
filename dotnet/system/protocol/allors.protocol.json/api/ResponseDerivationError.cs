// <copyright file="DerivationErrorResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api
{
    using System.Text.Json.Serialization;

    public class ResponseDerivationError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonPropertyName("e")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// [][AssociationId, RelationTypeId]
        /// </summary>
        [JsonPropertyName("r")]
        public long[][] Roles { get; set; }
    }
}
