// <copyright file="SyncRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Security
{
    using System.Text.Json.Serialization;

    public class SecurityRequest
    {
        [JsonPropertyName("accessControls")]
        public string[] AccessControls { get; set; }

        [JsonPropertyName("permissions")]
        public string[] Permissions { get; set; }
    }
}
