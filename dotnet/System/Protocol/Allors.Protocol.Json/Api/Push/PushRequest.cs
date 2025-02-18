// <copyright file="PushRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Push
{
    public class PushRequest : Request
    {
        /// <summary>
        /// New Objects
        /// </summary>
        public PushRequestNewObject[] n { get; set; }

        /// <summary>
        /// Objects
        /// </summary>
        public PushRequestObject[] o { get; set; }
    }
}
