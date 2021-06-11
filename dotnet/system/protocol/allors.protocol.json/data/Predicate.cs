// <copyright file="Predicate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Data
{
    public class Predicate : IVisitable
    {
        /// <summary>
        /// Kind
        /// </summary>
        public PredicateKind k { get; set; }

        /// <summary>
        /// Association Type
        /// </summary>
        public int? a { get; set; }

        /// <summary>
        /// Role Type
        /// </summary>
        public int? r { get; set; }

        /// <summary>
        /// Object Type
        /// </summary>
        public int? o { get; set; }

        /// <summary>
        /// Parameter
        /// </summary>
        public string p { get; set; }

        /// <summary>
        /// Dependencies
        /// </summary>
        public string[] d { get; set; }

        /// <summary>
        /// Operand
        /// </summary>
        public Predicate op { get; set; }

        /// <summary>
        /// Operands
        /// </summary>
        public Predicate[] ops { get; set; }

        /// <summary>
        /// Object
        /// </summary>
        public long? ob { get; set; }

        /// <summary>
        /// Objects
        /// </summary>
        public long[] obs { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public object v { get; set; }

        /// <summary>
        /// Values
        /// </summary>
        public object[] vs { get; set; }

        /// <summary>
        /// Path
        /// </summary>
        public int? pa { get; set; }

        /// <summary>
        /// Paths
        /// </summary>
        public int[] pas { get; set; }

        /// <summary>
        /// Extent
        /// </summary>
        public Extent e { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitPredicate(this);
    }
}
