// <copyright file="Contains.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System.Collections.Generic;
    using Allors.Database.Meta;
    

    public class Contains : IPropertyPredicate
    {
        public string[] Dependencies { get; set; }

        public Contains(IPropertyType propertyType = null) => this.PropertyType = propertyType;

        public IPropertyType PropertyType { get; set; }

        public IObject Object { get; set; }

        public string Parameter { get; set; }

        bool IPredicate.ShouldTreeShake(IDictionary<string, string> parameters) => this.HasMissingDependencies(parameters) || ((IPredicate)this).HasMissingArguments(parameters);

        bool IPredicate.HasMissingArguments(IDictionary<string, string> parameters) => this.Parameter != null && (parameters == null || !parameters.ContainsKey(this.Parameter));

        void IPredicate.Build(ISession session, IDictionary<string, string> parameters, Database.ICompositePredicate compositePredicate)
        {
            var containedObject = this.Parameter != null ? session.GetObject(parameters[this.Parameter]) : this.Object;

            if (this.PropertyType is IRoleType roleType)
            {
                compositePredicate.AddContains(roleType, containedObject);
            }
            else
            {
                var associationType = (IAssociationType)this.PropertyType;
                compositePredicate.AddContains(associationType, containedObject);
            }
        }

        public void Accept(IVisitor visitor) => visitor.VisitContains(this);
    }
}
