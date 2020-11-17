// <copyright file="TreeNode.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    using System;
    using System.Text.Json.Serialization;

    public class Node : IVisitable
    {
        [JsonPropertyName("associationType")]
        public Guid? AssociationType { get; set; }

        [JsonPropertyName("roleType")]
        public Guid? RoleType { get; set; }

        [JsonPropertyName("nodes")]
        public Node[] Nodes { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitNode(this);
    }
}
