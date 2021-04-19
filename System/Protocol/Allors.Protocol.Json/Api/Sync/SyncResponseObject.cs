// <copyright file="SyncResponseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using System.Text.Json.Serialization;

    public class SyncResponseObject
    {
        [JsonPropertyName("i")]
        public string Id { get; set; }

        [JsonPropertyName("t")]
        public int ObjectType { get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }

        [JsonPropertyName("a")]
        public string AccessControls { get; set; }

        [JsonPropertyName("d")]
        public string DeniedPermissions { get; set; }

        [JsonPropertyName("r")]
        public SyncResponseRole[] Roles { get; set; }

        public override string ToString() => $"{this.ObjectType} [{this.Id}:{this.Version}]";
    }
}
