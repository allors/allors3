// <copyright file="SyncResponseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using System.Diagnostics;
    using System.Text.Json.Serialization;

    [DebuggerDisplay("{Value} [{RoleType}]")]
    public class SyncResponseRole
    {
        [JsonPropertyName("t")]
        public int RoleType { get; set; }

        [JsonPropertyName("c")]
        public long[] Collection { get; set; }

        [JsonPropertyName("o")]
        public long? Object { get; set; }

        [JsonPropertyName("v")]
        public object Value { get; set; }
    }
}
