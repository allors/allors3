// <copyright file="Instanceof.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{

    using Meta;

    public class Instanceof : IPropertyPredicate
    {
        public string[] Dependencies { get; set; }

        public Instanceof(IPropertyType propertyType = null) => this.PropertyType = propertyType;

        public string Parameter { get; set; }

        public IComposite ObjectType { get; set; }

        public IPropertyType PropertyType { get; set; }

        public void Accept(IVisitor visitor) => visitor.VisitInstanceOf(this);
    }
}
