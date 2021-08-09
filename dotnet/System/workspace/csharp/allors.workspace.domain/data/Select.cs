// <copyright file="Select.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{
    using System.Collections.Generic;
    using Meta;

    public class Select : IVisitable
    {
        public Select()
        {
        }

        public Select(params IPropertyType[] propertyTypes)
        {
            if (propertyTypes.Length > 0)
            {
                this.Step = new Step(propertyTypes, 0);
            }
        }

        public IEnumerable<Node> Include { get; set; }

        public Step Step { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitSelect(this);
    }
}
