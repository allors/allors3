// <copyright file="Union.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System.Collections.Generic;
    using System.Linq;

    using Meta;

    public class Union : IExtentOperator
    {
        public Union(params IExtent[] operands) => this.Operands = operands;

        public IComposite ObjectType => this.Operands?[0].ObjectType;

        public IExtent[] Operands { get; set; }

        public Sort[] Sorting { get; set; }

        bool IExtent.HasMissingArguments(IDictionary<string, string> parameters) => this.Operands.Any(v => v.HasMissingArguments(parameters));

        public Database.Extent Build(ITransaction transaction, IDictionary<string, string> parameters = null)
        {
            var extent = transaction.Union(this.Operands[0].Build(transaction, parameters), this.Operands[1].Build(transaction, parameters));
            foreach (var sort in this.Sorting)
            {
                sort.Build(extent);
            }

            return extent;
        }

        public void Accept(IVisitor visitor) => visitor.VisitUnion(this);
    }
}
