// <copyright file="SyncRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Sync
{
    using System.Text.Json.Serialization;

    public class SyncRequest
    {
        [JsonPropertyName("objects")]
        public string[] Objects { get; set; }
    }
}
