// <copyright file="ICompositeBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;

    public partial interface ICompositeBase : IObjectTypeBase, IComposite
    {
        new IEnumerable<IInterfaceBase> Supertypes { get; }

        new IEnumerable<IClassBase> Classes { get; }

        new IEnumerable<IMethodTypeBase> MethodTypes { get; }

        IEnumerable<IAssociationType> AssociationTypes { get; }

        IEnumerable<IRoleType> RoleTypes { get; }

        void StructuralDeriveDirectSupertypes(HashSet<IInterfaceBase> sharedInterfaces);

        void StructuralDeriveSupertypes(HashSet<IInterfaceBase> sharedInterfaces);

        void StructuralDeriveRoleTypes(HashSet<IRoleTypeBase> sharedRoleTypes, Dictionary<ICompositeBase, HashSet<IRoleTypeBase>> roleTypesByAssociationTypeObjectType);

        void StructuralDeriveAssociationTypes(HashSet<IAssociationTypeBase> sharedAssociationTypes, Dictionary<IObjectTypeBase, HashSet<IAssociationTypeBase>> associationTypesByRoleTypeObjectType);

        void StructuralDeriveMethodTypes(HashSet<IMethodTypeBase> sharedMethodTypeList, Dictionary<ICompositeBase, HashSet<IMethodTypeBase>> methodTypeByClass);

        void DeriveIsRelationship();
    }
}
