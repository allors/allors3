// <copyright file="IAssociationType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the AssociationType type.</summary>

namespace Allors.Workspace.Meta
{
    /// <summary>
    /// An association type defines the association side of a relation.
    /// This is also called the 'active', 'controlling' or 'owning' side.
    /// AssociationTypes can only have composite <see cref="ObjectType"/>s.
    /// </summary>
    public interface IAssociationType : IPropertyType
    {
        new IComposite ObjectType { get; }

        IRoleType RoleType { get; }

        IRelationType RelationType { get; }
    }
}
