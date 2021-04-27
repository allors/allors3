// <copyright file="AuthenticationTokenRequest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Auth
{
    using System.Text.Json.Serialization;

    public class AuthenticationTokenRequest
    {
        [JsonPropertyName("l")]
        public string Login { get; set; }

        [JsonPropertyName("p")]
        public string Password { get; set; }
    }
}
