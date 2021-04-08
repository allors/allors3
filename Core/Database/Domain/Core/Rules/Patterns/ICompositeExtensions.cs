// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using System;
    using System.Linq.Expressions;
    using Meta;

    public static class ICompositeExtensions
    {
        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> association) where T : IComposite => new AssociationPattern<T>(composite, association, null);

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> association, IComposite ofType) where T : IComposite => new AssociationPattern<T>(composite, association, ofType);

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> association, Expression<Func<T, IPropertyType>> step, IComposite ofType = null) where T : IComposite => new AssociationPattern<T>(composite, association, step, ofType);

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> association, Expression<Func<T, IComposite>> step, IComposite ofType = null) where T : IComposite => new AssociationPattern<T>(composite, association, step, ofType);

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> role) where T : IComposite => new RolePattern<T>(composite, role, null);

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> role, IComposite ofType) where T : IComposite => new RolePattern<T>(composite, role, ofType);

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> role, Expression<Func<T, IPropertyType>> step, IComposite ofType = null) where T : IComposite => new RolePattern<T>(composite, role, step, ofType);

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> role, Expression<Func<T, IComposite>> step, IComposite ofType = null) where T : IComposite => new RolePattern<T>(composite, role, step, ofType);
    }
}
