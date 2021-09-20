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

    public class RolePattern<T> : RolePattern where T : IComposite
    {
        public RolePattern(T objectType, IRoleType roleType) : base(objectType, roleType) { }

        public RolePattern(T objectType, IRoleType roleType, Func<T, Node> path) : base(objectType, roleType) => this.Tree = new[] { path(objectType) };

        public RolePattern(T objectType, IRoleType roleType, Func<T, IEnumerable<Node>> path) : base(objectType, roleType) => this.Tree = path(objectType).ToArray();

        public RolePattern(T objectType, IRoleType roleType, Expression<Func<T, IPropertyType>> step) : base(objectType, roleType) => this.Tree = new[] { step?.Node(objectType.MetaPopulation) };

        public RolePattern(T objectType, IRoleType roleType, Expression<Func<T, IComposite>> step) : base(objectType, roleType) => this.Tree = new[] { step?.Node(objectType.MetaPopulation) };

        public RolePattern(T objectType, Func<T, IRoleType> role) : base(objectType, role(objectType)) { }

        public RolePattern(T objectType, Func<T, IRoleType> role, Func<T, Node> path) : base(objectType, role(objectType)) => this.Tree = new[] { path(objectType) };

        public RolePattern(T objectType, Func<T, IRoleType> role, Func<T, IEnumerable<Node>> path) : base(objectType, role(objectType)) => this.Tree = path(objectType).ToArray();

        public RolePattern(T objectType, Func<T, IRoleType> role, Expression<Func<T, IPropertyType>> step) : base(objectType, role(objectType)) => this.Tree = new[] { step?.Node(objectType.MetaPopulation) };

        public RolePattern(T objectType, Func<T, IRoleType> role, Expression<Func<T, IComposite>> step) : base(objectType, role(objectType)) => this.Tree = new[] { step?.Node(objectType.MetaPopulation) };
    }
}
