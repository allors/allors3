// <copyright file="Except.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Data
{
    using System.Collections.Generic;
    using System.Linq;

    using Allors.Meta;
    

    public class Except : IExtentOperator
    {
        public Except(params IExtent[] operands) => this.Operands = operands;

        public IComposite ObjectType => this.Operands?[0].ObjectType;

        public IExtent[] Operands { get; set; }

        public Sort[] Sorting { get; set; }
        
        bool IExtent.HasMissingArguments(IDictionary<string, string> parameters) => this.Operands.Any(v => v.HasMissingArguments(parameters));

        Allors.Extent IExtent.Build(ISession session, IDictionary<string, string> parameters)
        {
            var extent = session.Except(this.Operands[0].Build(session, parameters), this.Operands[1].Build(session, parameters));
            foreach (var sort in this.Sorting)
            {
                sort.Build(extent);
            }

            return extent;
        }

        public void Accept(IVisitor visitor) => visitor.VisitExcept(this);
    }
}
