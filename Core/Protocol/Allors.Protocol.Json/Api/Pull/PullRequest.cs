// <copyright file="PullRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Database.Pull
{
    using System.Text.Json.Serialization;
    using Json;

    public class PullRequest
    {
        [JsonPropertyName("p")]
        public Pull[] Pulls { get; set; }
    }
}
