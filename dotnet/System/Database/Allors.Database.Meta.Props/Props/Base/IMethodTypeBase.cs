// <copyright file="IClassBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    public partial interface IMethodTypeBase : IMetaIdentifiableObjectBase, IOperandTypeBase, IMethodType
    {
        new ICompositeBase ObjectType { get; }

        void Validate(ValidationLog log);

        void DeriveWorkspaceNames();
    }
}
