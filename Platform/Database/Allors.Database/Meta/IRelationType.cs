// <copyright file="IRelationType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Meta
{
    using System;

    /// <summary>
    /// A relation type defines the instance and behavior for
    /// a set of association types and role types.
    /// </summary>
    public interface IRelationType : IMetaObject, IMetaIdentity, IComparable
    {
        IAssociationType AssociationType { get; }

        IRoleType RoleType { get; }

        Multiplicity Multiplicity { get; }

        bool ExistExclusiveDatabaseClasses { get; }

        bool IsIndexed { get; }

        bool IsDerived { get; }

        bool IsSynced { get; }
    }
}
