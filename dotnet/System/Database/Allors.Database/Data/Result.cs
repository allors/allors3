// <copyright file="Result.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    public class Result : IVisitable
    {
        public Select Select { get; set; }

        public Node[] Include { get; set; }

        public string Name { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }

        public override string ToString()
        {
            if (this.Name != null)
            {
                return $"Result: [Name: {this.Name}]";
            }

            return $"Result: [Select: {this.Select}]";
        }

        public void Accept(IVisitor visitor) => visitor.VisitResult(this);
    }
}
