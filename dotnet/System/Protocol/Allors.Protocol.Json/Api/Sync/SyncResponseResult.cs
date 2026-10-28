// <copyright file="SyncResponseResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Api.Sync
{
    using System.Collections.Generic;

    public class SyncResponseResult
    {
        public IDictionary<string, long> o { get; set; }

        public IDictionary<string, long[]> c { get; set; }

        public IDictionary<string, object> v { get; set; }
    }
}
