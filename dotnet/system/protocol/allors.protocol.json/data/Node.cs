// <copyright file="Node.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{

    public class Node : IVisitable
    {
        /// <summary>
        /// Association Type
        /// </summary>
        public int? a { get; set; }

        /// <summary>
        /// Role Type
        /// </summary>
        public int? r { get; set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public Node[] n { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitNode(this);
    }
}
