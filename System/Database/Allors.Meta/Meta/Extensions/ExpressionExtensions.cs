// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Meta;

    internal class MemberExpressionsVisitor : ExpressionVisitor
    {
        public MemberExpressionsVisitor() => this.MemberExpressions = new List<MemberExpression>();

        public IList<MemberExpression> MemberExpressions { get; }

        protected override Expression VisitMember(MemberExpression node)
        {
            this.MemberExpressions.Insert(0, node);
            return base.VisitMember(node);
        }
    }

    public static class ExpressionExtensions
    {
        public static IPropertyType[] ToPropertyTypes<T>(this Expression<Func<T, IPropertyType>> @this, IMetaPopulation metaPopulation) where T : IComposite
        {
            var visitor = new MemberExpressionsVisitor();
            _ = visitor.Visit(@this);

            var propertyTypes = new List<IPropertyType>();

            var root = visitor.MemberExpressions[0].Member.DeclaringType;
            var composite = (IComposite)metaPopulation.FindDatabaseCompositeByName(root.Name);

            foreach (var memberExpression in visitor.MemberExpressions)
            {
                if (memberExpression.Type.GetInterfaces().Contains(typeof(IComposite)))
                {
                    var name = memberExpression.Member.Name;
                    composite = (IComposite)metaPopulation.FindDatabaseCompositeByName(name);
                }

                if (memberExpression.Type.GetInterfaces().Contains(typeof(IRoleType)))
                {
                    var name = memberExpression.Member.Name;
                    var propertyType = composite.DatabaseRoleTypes.First(v => v.Name.Equals(name));
                    propertyTypes.Add(propertyType);
                }

                if (memberExpression.Type.GetInterfaces().Contains(typeof(IAssociationType)))
                {
                    var name = memberExpression.Member.Name;
                    var propertyType = composite.DatabaseAssociationTypes.First(v => v.Name.Equals(name));
                    propertyTypes.Add(propertyType);
                }
            }

            return propertyTypes.ToArray();
        }
    }
}
