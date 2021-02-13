// <copyright file="IRoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;

    /// <summary>
    /// A <see cref="IRoleType"/> defines the role side of a relation.
    /// This is also called the 'passive' side.
    /// RoleTypes can have composite and unit <see cref="ObjectType"/>s.
    /// </summary>
    public interface IRoleType : IPropertyType, IComparable
    {
        IRoleDefault Default { get; }

        string SingularFullName { get; }

        string PluralFullName { get; }

        IAssociationType AssociationType { get; }

        IComposite AssociationTypeComposite { get; }

        IRelationType RelationType { get; }

        int? Size { get; }

        int? Precision { get; }

        int? Scale { get; }
    }
}
