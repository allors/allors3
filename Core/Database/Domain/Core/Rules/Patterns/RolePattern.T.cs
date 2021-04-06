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
    using Meta;

    public class RolePattern<T> : RolePattern where T : IComposite
    {
        public RolePattern(T objectType, Func<T, IRoleType> role, Expression<Func<T, IPropertyType>> step = null, IComposite ofType = null) : base(objectType, role(objectType))
        {
            this.Steps = step?.ToPropertyTypes(objectType.MetaPopulation);
            this.OfType = ofType;
        }
    }
}
