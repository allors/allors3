// <copyright file="IClassBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;

    public partial interface IInterfaceBase : ICompositeBase, IInterface
    {
        void StructuralDeriveDirectSubtypes(HashSet<ICompositeBase> sharedCompositeTypes);

        void StructuralDeriveSubtypes(HashSet<ICompositeBase> sharedCompositeTypes);

        void StructuralDeriveSubclasses(HashSet<IClassBase> sharedClasses);

        void StructuralDeriveExclusiveSubclass();

        void StructuralDeriveSupertypesRecursively(IObjectTypeBase type, HashSet<IInterfaceBase> superTypes);

        void DeriveWorkspaceNames();
    }
}
