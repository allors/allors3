// <copyright file="Select.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{

    public class Select : IVisitable
    {
        /// <summary>
        /// Association Type
        /// </summary>
        public int? a { get; set; }

        /// <summary>
        /// RoleType
        /// </summary>
        public int? r { get; set; }

        /// <summary>
        /// Next
        /// </summary>
        public Select n { get; set; }

        /// <summary>
        /// Include
        /// </summary>
        public Node[] i { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitSelect(this);
    }
}
