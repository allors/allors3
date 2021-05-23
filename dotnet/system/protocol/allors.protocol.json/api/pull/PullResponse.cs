// <copyright file="PullResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class PullResponse : Response
    {
        [JsonPropertyName("c")]
        public IDictionary<string, long[]> Collections { get; set; }

        [JsonPropertyName("o")]
        public IDictionary<string, long> Objects { get; set; }

        [JsonPropertyName("v")]
        public IDictionary<string, object> Values { get; set; }

        [JsonPropertyName("p")]
        public PullResponseObject[] Pool { get; set; }

        [JsonPropertyName("a")]
        public long[][] AccessControls { get; set; }
    }
}
