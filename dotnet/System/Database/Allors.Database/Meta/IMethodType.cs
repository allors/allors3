// <copyright file="IMethodType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Database.Meta
{
    public interface IMethodType : IMetaIdentifiableObject, IOperandType
    {
        IComposite ObjectType { get; }

        string Name { get; }

        string FullName { get; }
    }
}
