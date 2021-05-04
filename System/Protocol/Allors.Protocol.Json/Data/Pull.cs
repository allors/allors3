// <copyright file="Pull.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class Pull : IVisitable
    {
        [JsonPropertyName("er")]
        public Guid? ExtentRef { get; set; }

        [JsonPropertyName("e")]
        public Extent Extent { get; set; }

        [JsonPropertyName("t")]
        public int? ObjectType { get; set; }

        [JsonPropertyName("o")]
        public long? Object { get; set; }

        [JsonPropertyName("r")]
        public Result[] Results { get; set; }

        [JsonPropertyName("a")]
        public IDictionary<string, object> Arguments { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitPull(this);
    }
}
