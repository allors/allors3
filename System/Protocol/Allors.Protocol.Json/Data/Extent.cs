// <copyright file="Extent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System.Text.Json.Serialization;

    public class Extent : IVisitable
    {
        [JsonPropertyName("k")]
        public ExtentKind Kind { get; set; }

        [JsonPropertyName("o")]
        public Extent[] Operands { get; set; }

        [JsonPropertyName("t")]
        public int? ObjectType { get; set; }

        [JsonPropertyName("p")]
        public Predicate Predicate { get; set; }

        [JsonPropertyName("s")]
        public Sort[] Sorting { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitExtent(this);
    }
}
