// <copyright file="Invocation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Invoke
{
    using System.Text.Json.Serialization;

    public class Invocation
    {
        [JsonPropertyName("i")]
        public string Id { get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }

        [JsonPropertyName("m")]
        public string Method { get; set; }
    }
}
