// <copyright file="PushRequestObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Push
{
    using System.Text.Json.Serialization;

    /// <summary>
    ///  New objects require NI and T.
    ///  Existing objects require I and V.
    /// </summary>
    public class PushRequestObject
    {
        [JsonPropertyName("d")]
        public long DatabaseId { get; set; }

        [JsonPropertyName("v")]
        public long Version { get; set; }

        [JsonPropertyName("r")]
        public PushRequestRole[] Roles { get; set; }
    }
}
