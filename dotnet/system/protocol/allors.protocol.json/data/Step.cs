// <copyright file="Step.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System.Text.Json.Serialization;

    public class Step : IVisitable
    {
        [JsonPropertyName("a")]
        public int? AssociationType { get; set; }

        [JsonPropertyName("r")]
        public int? RoleType { get; set; }

        [JsonPropertyName("n")]
        public Step Next { get; set; }

        [JsonPropertyName("i")]
        public Node[] Include { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitStep(this);
    }
}
