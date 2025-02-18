// <copyright file="AssociationType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the AssociationType type.</summary>

namespace Allors.Workspace.Meta
{
    public interface IRelationTypeInternals : IRelationType
    {
        IAssociationTypeInternals AssociationType { get; }

        IRoleTypeInternals RoleType { get; }
    }
}
