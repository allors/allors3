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
        public string RelationType { get; set; }

        [JsonPropertyName("s")]
        public string SetRole { get; set; }

        [JsonPropertyName("a")]
        public string[] AddRole { get; set; }

        [JsonPropertyName("r")]
        public string[] RemoveRole { get; set; }
    }
}
