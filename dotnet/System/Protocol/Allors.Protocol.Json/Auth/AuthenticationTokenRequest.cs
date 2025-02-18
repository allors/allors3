// <copyright file="AuthenticationTokenRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Auth
{
    public class AuthenticationTokenRequest
    {
        /// <summary>
        /// Login
        /// </summary>
        public string l { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string p { get; set; }
    }
}
