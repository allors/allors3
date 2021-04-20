// <copyright file="PullRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Pull
{
    using System.Text.Json.Serialization;
    using Json;
    using Data;

    public class PullRequest
    {
        [JsonPropertyName("l")]
        public Pull[] List { get; set; }

        [JsonPropertyName("p")]
        public Procedure Procedure { get; set; }
    }
}
