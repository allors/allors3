// <copyright file="InvokeReadReceipt.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.InvokeRead
{
    using Allors.Protocol.Json.Api.Read;

    public class InvokeReadReceipt
    {
        public string i { get; set; }
        public ReadScope sc { get; set; }
        public InvokeReadOutcome invoke { get; set; }
    }
}
