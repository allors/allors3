// <copyright file="ISession.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using System;

    using Allors.Meta;

    /// <summary>
    /// Notifications for access to objects (Get and Exist on Strategy and Extent).
    /// </summary>
    public interface IOnAccess 
    {
        /// <summary>
        /// An OnAccessUnitRole receives notifications for get operations for a unit role.
        /// </summary>
        Action<IStrategy, IRoleType> OnAccessUnitRole { get; set; }

        /// <summary>
        /// An OnAccessCompositeRole receives notifications for get operations for a object role.
        /// </summary>
        Action<IStrategy, IRoleType> OnAccessCompositeRole { get; set; }

        /// <summary>
        /// An OnAccessCompositesRole receives notifications for get operations for objects role.
        /// </summary>
        Action<IStrategy, IRoleType> OnAccessCompositesRole { get; set; }

        /// <summary>
        /// An OnAccessCompositeAssociation receives notifications for get operations for an object association.
        /// </summary>
        Action<IStrategy, IAssociationType> OnAccessCompositeAssociation { get; set; }

        /// <summary>
        /// An OnAccessCompositesAssociation receives notifications for get operations for objects association.
        /// </summary>
        Action<IStrategy, IAssociationType> OnAccessCompositesAssociation { get; set; }
    }
}
