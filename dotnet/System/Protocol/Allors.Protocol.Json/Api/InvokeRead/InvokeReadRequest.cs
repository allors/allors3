// <copyright file="InvokeReadRequest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.InvokeRead
{
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Read;

    /// <summary>One protected atomic method batch with an explicit refresh choice.</summary>
    public class InvokeReadRequest
    {
        public int v { get; set; }
        public string i { get; set; }
        public ReadScope sc { get; set; }
        public string x { get; set; }
        public Invocation[] l { get; set; }
        public bool refresh { get; set; }
    }
}
