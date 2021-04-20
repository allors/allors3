// <copyright file="PullRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class PullArgs
    {
        [JsonPropertyName("c")]
        public Dictionary<string, long[]> Collections { get; set; }

        [JsonPropertyName("o")]
        public Dictionary<string, long> Objects { get; set; }

        [JsonPropertyName("v")]
        public Dictionary<string, object> Values { get; set; }

        [JsonPropertyName("p")]
        public long[][] Pool { get; set; }
    }
}
