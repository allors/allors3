// <copyright file="Extent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System;
    using System.Text.Json.Serialization;

    public class Extent : IVisitable
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("operands")]
        public Extent[] Operands { get; set; }

        [JsonPropertyName("objectType")]
        public Guid? ObjectType { get; set; }

        [JsonPropertyName("predicate")]
        public Predicate Predicate { get; set; }

        [JsonPropertyName("sorting")]
        public Sort[] Sorting { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitExtent(this);
    }
}
