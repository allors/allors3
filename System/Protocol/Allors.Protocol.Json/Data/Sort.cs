// <copyright file="Sort.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System;
    using System.Text.Json.Serialization;

    public class Sort : IVisitable
    {
        [JsonPropertyName("roleType")]
        public Guid? RoleType { get; set; }

        [JsonPropertyName("descending")]
        public bool Descending { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitSort(this);
    }
}
