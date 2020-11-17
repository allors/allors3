// <copyright file="PushRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Push
{
    using System.Text.Json.Serialization;

    public class PushRequest
    {
        [JsonPropertyName("newObjects")]
        public PushRequestNewObject[] NewObjects { get; set; }

        [JsonPropertyName("objects")]
        public PushRequestObject[] Objects { get; set; }
    }
}
