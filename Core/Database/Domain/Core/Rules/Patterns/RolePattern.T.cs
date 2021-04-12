// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Data;
    using Meta;

    public class RolePattern<T> : RolePattern where T : IComposite
    {
        public RolePattern(T objectType, IRoleType role) : base(objectType, role) { }

        public RolePattern(T objectType, Func<T, IRoleType> role) : base(objectType, role(objectType)) { }

        public RolePattern(T objectType, Func<T, IRoleType> role, Expression<Func<T, IPropertyType>> step) : base(objectType, role(objectType)) => this.Path = new[] { step?.ToPath(objectType.MetaPopulation) };

        public RolePattern(T objectType, Func<T, IRoleType> role, Expression<Func<T, IComposite>> step) : base(objectType, role(objectType)) => this.Path = new[] { step?.ToPath(objectType.MetaPopulation) };
    }
}
