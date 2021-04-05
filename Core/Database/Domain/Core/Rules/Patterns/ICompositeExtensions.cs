// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using Antlr.Runtime.Misc;
    using Meta;

    public static class ICompositeExtensions
    {
        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> association, Func<T, IPropertyType> step = null) where T : IComposite => new AssociationPattern<T>(composite, association, step);

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> role, Func<T, IPropertyType> step = null) where T : IComposite => new RolePattern<T>(composite, role, step);
    }
}
