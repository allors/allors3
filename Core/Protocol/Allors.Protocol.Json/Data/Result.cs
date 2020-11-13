// <copyright file="Result.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json
{
    using System;
    using System.Text.Json.Serialization;

    public class Result : IVisitable
    {
        [JsonPropertyName("fetchRef")]
        public Guid? FetchRef { get; set; }

        [JsonPropertyName("fetch")]
        public Fetch Fetch { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("skip")]
        public int? Skip { get; set; }

        [JsonPropertyName("take")]
        public int? Take { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitResult(this);
    }
}
