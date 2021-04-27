// <copyright file="Sort.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System.Text.Json.Serialization;

    public class Sort : IVisitable
    {
        [JsonPropertyName("r")]
        public int? RoleType { get; set; }

        [JsonPropertyName("d")]
        public SortDirection SortDirection { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitSort(this);
    }
}
