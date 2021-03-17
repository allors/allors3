// <copyright file="Select.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System.Text.Json.Serialization;

    public class Select : IVisitable
    {
        [JsonPropertyName("step")]
        public Step Step { get; set; }

        [JsonPropertyName("include")]
        public Node[] Include { get; set; }
        
        public void Accept(IVisitor visitor) => visitor.VisitSelect(this);
    }
}
