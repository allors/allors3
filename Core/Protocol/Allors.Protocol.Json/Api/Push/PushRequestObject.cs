// <copyright file="PushRequestObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Push
{
    using System.Text.Json.Serialization;

    /// <summary>
    ///  New objects require NI and T.
    ///  Existing objects require I and V.
    /// </summary>
    public class PushRequestObject
    {
        [JsonPropertyName("i")]
        public string DatabaseId { get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }

        [JsonPropertyName("roles")]
        public PushRequestRole[] Roles { get; set; }
    }
}
