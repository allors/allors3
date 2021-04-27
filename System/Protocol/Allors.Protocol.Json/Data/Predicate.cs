// <copyright file="Predicate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System.Text.Json.Serialization;

    public class Predicate : IVisitable
    {
        [JsonPropertyName("k")]
        public PredicateKind Kind { get; set; }

        [JsonPropertyName("a")]
        public int? AssociationType { get; set; }

        [JsonPropertyName("r")]
        public int? RoleType { get; set; }

        [JsonPropertyName("t")]
        public int? ObjectType { get; set; }

        [JsonPropertyName("p")]
        public string Parameter { get; set; }

        [JsonPropertyName("d")]
        public string[] Dependencies { get; set; }

        [JsonPropertyName("op")]
        public Predicate Operand { get; set; }

        [JsonPropertyName("ops")]
        public Predicate[] Operands { get; set; }

        [JsonPropertyName("ob")]
        public long? Object { get; set; }

        [JsonPropertyName("obs")]
        public long[] Objects { get; set; }

        [JsonPropertyName("v")]
        public string Value { get; set; }

        [JsonPropertyName("vs")]
        public string[] Values { get; set; }

        [JsonPropertyName("e")]
        public Extent Extent { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitPredicate(this);
    }
}
