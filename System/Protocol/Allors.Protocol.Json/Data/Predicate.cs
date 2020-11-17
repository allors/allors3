// <copyright file="Predicate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System;
    using System.Text.Json.Serialization;

    public class Predicate : IVisitable
    {
        [JsonPropertyName("kind")]
        public PredicateKind Kind { get; set; }

        [JsonPropertyName("associationType")]
        public Guid? AssociationType { get; set; }

        [JsonPropertyName("roleType")]
        public Guid? RoleType { get; set; }

        [JsonPropertyName("objectType")]
        public Guid? ObjectType { get; set; }

        [JsonPropertyName("parameter")]
        public string Parameter { get; set; }

        [JsonPropertyName("dependencies")]
        public string[] Dependencies { get; set; }

        [JsonPropertyName("operand")]
        public Predicate Operand { get; set; }

        [JsonPropertyName("operands")]
        public Predicate[] Operands { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("objects")]
        public string[] Objects { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("values")]
        public string[] Values { get; set; }

        [JsonPropertyName("extent")]
        public Extent Extent { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitPredicate(this);
    }
}
