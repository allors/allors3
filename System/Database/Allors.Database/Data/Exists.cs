// <copyright file="Exists.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data
{
    using System;
    using System.Collections.Generic;
    using Meta;
    
    public class Exists : IPropertyPredicate
    {
        public string[] Dependencies { get; set; }

        public Exists(IPropertyType propertyType = null) => this.PropertyType = propertyType;

        public string Parameter { get; set; }

        public IPropertyType PropertyType { get; set; }
        
        bool IPredicate.ShouldTreeShake(IDictionary<string, string> parameters) => this.HasMissingDependencies(parameters) || ((IPredicate)this).HasMissingArguments(parameters);

        bool IPredicate.HasMissingArguments(IDictionary<string, string> parameters) => this.Parameter != null && (parameters == null || !parameters.ContainsKey(this.Parameter));

        void IPredicate.Build(ITransaction transaction, IDictionary<string, string> parameters, Database.ICompositePredicate compositePredicate)
        {
            var parameter = this.Parameter != null ? parameters[this.Parameter] : null;
            var propertyType = Guid.TryParse(parameter, out var metaObjectId) ? (IPropertyType)transaction.GetMetaObject(metaObjectId) : this.PropertyType;

            if (propertyType != null)
            {
                if (propertyType is IRoleType roleType)
                {
                    compositePredicate.AddExists(roleType);
                }
                else
                {
                    var associationType = (IAssociationType)propertyType;
                    compositePredicate.AddExists(associationType);
                }
            }
        }

        public void Accept(IVisitor visitor) => visitor.VisitExists(this);
    }
}
