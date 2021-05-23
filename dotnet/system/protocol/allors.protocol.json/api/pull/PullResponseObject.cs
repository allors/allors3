// <copyright file="PullResponse.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    using System.Text.Json.Serialization;

    public class PullResponseObject
    {
        [JsonPropertyName("i")]
        public long Id { get; set; }

        [JsonPropertyName("v")]
        public long Version { get; set; }

        [JsonPropertyName("a")]
        public long[] AccessControls { get; set; }

        [JsonPropertyName("d")]
        public long[] DeniedPermissions { get; set; }
    }
}
