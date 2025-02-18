// <copyright file="InvokeRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Invoke
{
    public class InvokeRequest : Request
    {
        /// <summary>
        ///  List of Invocations
        /// </summary>
        public Invocation[] l { get; set; }

        /// <summary>
        /// Options
        /// </summary>
        public InvokeOptions o { get; set; }
    }
}
