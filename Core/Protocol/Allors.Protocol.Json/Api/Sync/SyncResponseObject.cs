// <copyright file="SyncResponseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Sync
{
    using System.Text.Json.Serialization;

    public class SyncResponseObject
    {
        [JsonPropertyName("i")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the object type.
        /// Format is a mapping ":{key}:{value}" or a key "{key}".
        /// The key will be generated on first occurrence of the ObjectType
        /// and is local to this Sync.
        /// </summary>
        [JsonPropertyName("t")]
        public string ObjectTypeOrKey { get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }

        [JsonPropertyName("a")]
        public string AccessControls { get; set; }

        [JsonPropertyName("d")]
        public string DeniedPermissions { get; set; }

        [JsonPropertyName("r")]
        public SyncResponseRole[] Roles { get; set; }

        public override string ToString() => $"{this.ObjectTypeOrKey} [{this.Id}:{this.Version}]";
    }
}
