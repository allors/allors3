// <copyright file="Result.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System;
    using System.Text.Json.Serialization;

    public class Result : IVisitable
    {
        [JsonPropertyName("r")]
        public Guid? SelectRef { get; set; }

        [JsonPropertyName("s")]
        public Select Select { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("k")]
        public int? Skip { get; set; }

        [JsonPropertyName("t")]
        public int? Take { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitResult(this);
    }
}
