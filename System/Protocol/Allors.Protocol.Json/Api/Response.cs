// <copyright file="ErrorResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api
{
    using System.Text.Json.Serialization;

    public abstract class Response
    {
        public bool HasErrors => this.VersionErrors?.Length > 0 || this.AccessErrors?.Length > 0 || this.MissingErrors?.Length > 0 || this.DerivationErrors?.Length > 0 || !string.IsNullOrWhiteSpace(this.ErrorMessage);

        [JsonPropertyName("_e")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("_v")]
        public long[] VersionErrors { get; set; }

        [JsonPropertyName("_a")]
        public long[] AccessErrors { get; set; }

        [JsonPropertyName("_m")]
        public long[] MissingErrors { get; set; }

        [JsonPropertyName("_d")]
        public ResponseDerivationError[] DerivationErrors { get; set; }
    }
}
