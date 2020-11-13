// <copyright file="PushResponseNewObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Push
{
    using System.Text.Json.Serialization;

    public class PushResponseNewObject
    {
        [JsonPropertyName("wi")]
        public string WorkspaceId { get; set; }

        [JsonPropertyName("i")]
        public string DatabaseId { get; set; }
    }
}
