// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Workspace.Domain.Derivations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Meta;
    using Workspace.Data;

    public class AssociationPattern<T> : AssociationPattern where T : IComposite
    {
        public AssociationPattern(T objectType, IAssociationType associationType) : base(objectType, associationType)
        {
        }

        public AssociationPattern(T objectType, IAssociationType associationType, Func<T, Node> node) : base(objectType, associationType) => this.Tree = new[] { node(objectType) };

        public AssociationPattern(T objectType, IAssociationType associationType, Func<T, IEnumerable<Node>> tree) : base(objectType, associationType) => this.Tree = tree(objectType).ToArray();

        public AssociationPattern(T objectType, IAssociationType associationType, Expression<Func<T, IComposite>> path) : base(objectType, associationType) => this.Tree = new[] { path.Node(objectType.MetaPopulation) };

        public AssociationPattern(T objectType, IAssociationType associationType, Expression<Func<T, IPropertyType>> path) : base(objectType, associationType) => this.Tree = new[] { path.Node(objectType.MetaPopulation) };

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType) : base(objectType, associationType(objectType))
        {
        }

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType, Func<T, Node> node) : base(objectType, associationType(objectType)) => this.Tree = new[] { node(objectType) };

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType, Func<T, IEnumerable<Node>> tree) : base(objectType, associationType(objectType)) => this.Tree = tree(objectType).ToArray();

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType, Expression<Func<T, IComposite>> path) : base(objectType, associationType(objectType)) => this.Tree = new[] { path.Node(objectType.MetaPopulation) };

        public AssociationPattern(T objectType, Func<T, IAssociationType> associationType, Expression<Func<T, IPropertyType>> path) : base(objectType, associationType(objectType)) => this.Tree = new[] { path.Node(objectType.MetaPopulation) };
    }
}
