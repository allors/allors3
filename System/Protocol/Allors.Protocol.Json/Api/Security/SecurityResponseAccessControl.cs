// <copyright file="SyncResponseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Security
{
    using System.Text.Json.Serialization;

    public class SecurityResponseAccessControl
    {
        [JsonPropertyName("i")]
        public string Id { get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }

        [JsonPropertyName("p")]
        public string PermissionIds { get; set; }
    }
}
