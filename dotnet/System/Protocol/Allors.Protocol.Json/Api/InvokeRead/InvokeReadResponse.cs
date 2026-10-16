// <copyright file="InvokeReadResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.InvokeRead
{
    public class InvokeReadResponse : InvokeReadReceipt
    {
        /// <summary>Requested full affected records or an independent ReadFailure.</summary>
        public object changed { get; set; }
    }
}
