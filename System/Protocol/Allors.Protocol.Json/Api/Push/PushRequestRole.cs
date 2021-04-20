// <copyright file="PushRequestRole.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Push
{
    using System.Text.Json.Serialization;

    public class PushRequestRole
    {
        [JsonPropertyName("t")]
        public int RelationType { get; set; }

        [JsonPropertyName("u")]
        public string SetUnitRole { get; set; }

        [JsonPropertyName("c")]
        public long? SetCompositeRole { get; set; }

        [JsonPropertyName("a")]
        public long[] AddCompositesRole { get; set; }

        [JsonPropertyName("r")]
        public long[] RemoveCompositesRole { get; set; }
    }
}
