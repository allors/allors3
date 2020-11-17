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
        [JsonPropertyName("extentRef")]
        public Guid? ExtentRef { get; set; }

        [JsonPropertyName("extent")]
        public Extent Extent { get; set; }

        [JsonPropertyName("objectType")]
        public Guid? ObjectType { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("results")]
        public Result[] Results { get; set; }

        [JsonPropertyName("parameters")]
        public IDictionary<string, string> Parameters { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitPull(this);
    }
}
