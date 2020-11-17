// <copyright file="InvokeOptions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Invoke
{
    using System.Text.Json.Serialization;

    public class InvokeOptions
    {
        [JsonPropertyName("i")]
        public bool Isolated { get; set; }

        [JsonPropertyName("c")]
        public bool ContinueOnError { get; set; }
    }
}
