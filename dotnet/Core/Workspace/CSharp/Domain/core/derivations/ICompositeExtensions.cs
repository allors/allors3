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

    public static class ICompositeExtensions
    {
        // AssociationType
        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> associationType) where T : IComposite => new AssociationPattern<T>(composite, associationType);

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> associationType, IComposite ofType) where T : IComposite => new AssociationPattern<T>(composite, associationType) { OfType = ofType };

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> associationType, Func<T, Node> node, IComposite ofType = null) where T : IComposite => new AssociationPattern<T>(composite, associationType, node) { OfType = ofType };

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> associationType, Func<T, IEnumerable<Node>> nodes, IComposite ofType = null) where T : IComposite => new AssociationPattern<T>(composite, associationType, nodes) { OfType = ofType };

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> associationType, Expression<Func<T, IPropertyType>> path, IComposite ofType = null) where T : IComposite => new AssociationPattern<T>(composite, associationType, path) { OfType = ofType };

        public static AssociationPattern<T> AssociationPattern<T>(this T composite, Func<T, IAssociationType> associationType, Expression<Func<T, IComposite>> path, IComposite ofType = null) where T : IComposite => new AssociationPattern<T>(composite, associationType, path) { OfType = ofType };

        // AssociationTypes
        public static IEnumerable<AssociationPattern<T>> AssociationPattern<T>(this T composite, Func<T, IEnumerable<IAssociationType>> associationTypes) where T : IComposite => associationTypes(composite).Select(v => new AssociationPattern<T>(composite, v));

        public static IEnumerable<AssociationPattern<T>> AssociationPattern<T>(this T composite, Func<T, IEnumerable<IAssociationType>> associationTypes, IComposite ofType) where T : IComposite => associationTypes(composite).Select(v => new AssociationPattern<T>(composite, v) { OfType = ofType });

        public static IEnumerable<AssociationPattern<T>> AssociationPattern<T>(this T composite, Func<T, IEnumerable<IAssociationType>> associationTypes, Func<T, Node> node, IComposite ofType = null) where T : IComposite => associationTypes(composite).Select(v => new AssociationPattern<T>(composite, v, node) { OfType = ofType });

        public static IEnumerable<AssociationPattern<T>> AssociationPattern<T>(this T composite, Func<T, IEnumerable<IAssociationType>> associationTypes, Func<T, IEnumerable<Node>> nodes, IComposite ofType = null) where T : IComposite => associationTypes(composite).Select(v => new AssociationPattern<T>(composite, v, nodes) { OfType = ofType });

        public static IEnumerable<AssociationPattern<T>> AssociationPattern<T>(this T composite, Func<T, IEnumerable<IAssociationType>> associationTypes, Expression<Func<T, IPropertyType>> path, IComposite ofType = null) where T : IComposite => associationTypes(composite).Select(v => new AssociationPattern<T>(composite, v, path) { OfType = ofType });

        public static IEnumerable<AssociationPattern<T>> AssociationPattern<T>(this T composite, Func<T, IEnumerable<IAssociationType>> associationTypes, Expression<Func<T, IComposite>> path, IComposite ofType = null) where T : IComposite => associationTypes(composite).Select(v => new AssociationPattern<T>(composite, v, path) { OfType = ofType });

        // RoleType
        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> roleType) where T : IComposite => new RolePattern<T>(composite, roleType);

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> roleType, IComposite ofType) where T : IComposite => new RolePattern<T>(composite, roleType) { OfType = ofType };

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> roleType, Func<T, Node> node, IComposite ofType = null) where T : IComposite => new RolePattern<T>(composite, roleType, node) { OfType = ofType };

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> roleType, Func<T, IEnumerable<Node>> nodes, IComposite ofType = null) where T : IComposite => new RolePattern<T>(composite, roleType, nodes) { OfType = ofType };

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> roleType, Expression<Func<T, IPropertyType>> path, IComposite ofType = null) where T : IComposite => new RolePattern<T>(composite, roleType, path) { OfType = ofType };

        public static RolePattern<T> RolePattern<T>(this T composite, Func<T, IRoleType> roleType, Expression<Func<T, IComposite>> path, IComposite ofType = null) where T : IComposite => new RolePattern<T>(composite, roleType, path) { OfType = ofType };

        // RoleTypes
        public static IEnumerable<RolePattern<T>> RolePattern<T>(this T composite, Func<T, IEnumerable<IRoleType>> roleTypes) where T : IComposite => roleTypes(composite).Select(v => new RolePattern<T>(composite, v));

        public static IEnumerable<RolePattern<T>> RolePattern<T>(this T composite, Func<T, IEnumerable<IRoleType>> roleTypes, IComposite ofType) where T : IComposite => roleTypes(composite).Select(v => new RolePattern<T>(composite, v) { OfType = ofType });

        public static IEnumerable<RolePattern<T>> RolePattern<T>(this T composite, Func<T, IEnumerable<IRoleType>> roleTypes, Func<T, Node> node, IComposite ofType = null) where T : IComposite => roleTypes(composite).Select(v => new RolePattern<T>(composite, v, node) { OfType = ofType });

        public static IEnumerable<RolePattern<T>> RolePattern<T>(this T composite, Func<T, IEnumerable<IRoleType>> roleTypes, Func<T, IEnumerable<Node>> nodes, IComposite ofType = null) where T : IComposite => roleTypes(composite).Select(v => new RolePattern<T>(composite, v, nodes) { OfType = ofType });

        public static IEnumerable<RolePattern<T>> RolePattern<T>(this T composite, Func<T, IEnumerable<IRoleType>> roleTypes, Expression<Func<T, IPropertyType>> path, IComposite ofType = null) where T : IComposite => roleTypes(composite).Select(v => new RolePattern<T>(composite, v, path) { OfType = ofType });

        public static IEnumerable<RolePattern<T>> RolePattern<T>(this T composite, Func<T, IEnumerable<IRoleType>> roleTypes, Expression<Func<T, IComposite>> path, IComposite ofType = null) where T : IComposite => roleTypes(composite).Select(v => new RolePattern<T>(composite, v, path) { OfType = ofType });
    }
}
