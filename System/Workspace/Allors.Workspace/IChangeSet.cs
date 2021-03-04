// <copyright file="IChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IChangeSet type.</summary>

namespace Allors.Workspace
{
    using System.Collections.Generic;

    using Meta;

    /// <summary>
    /// A change set is created during a checkpoint
    /// and contains all changes that have
    /// occurred in a <see cref="ISession"/> either starting
    /// from the beginning of the transaction or from a
    /// previous checkpoint.
    /// </summary>
    public interface IChangeSet
    {
        /// <summary>
        /// Gets the session.
        /// </summary>
        ISession Session { get; }

        /// <summary>
        /// Gets the created objects.
        /// </summary>
        ISet<IStrategy> Created { get; }

        /// <summary>
        /// Gets the deleted objects.
        /// </summary>
        ISet<IStrategy> Instantiated { get; }

        /// <summary>
        /// Gets the changed associations.
        /// </summary>
        ISet<Identity> Associations { get; }

        /// <summary>
        /// Gets the changed roles.
        /// </summary>
        ISet<Identity> Roles { get; }

        /// <summary>
        /// Gets the changed role types by association.
        /// </summary>
        IDictionary<Identity, ISet<IRoleType>> RoleTypesByAssociation { get; }

        /// <summary>
        /// Gets the changed association types by role.
        /// </summary>
        IDictionary<Identity, ISet<IAssociationType>> AssociationTypesByRole { get; }

        /// <summary>
        /// Gets the changed associations by role type.
        /// </summary>
        IDictionary<IRoleType, ISet<Identity>> AssociationsByRoleType { get; }

        /// <summary>
        /// Gets the changed roles by association type.
        /// </summary>
        IDictionary<IAssociationType, ISet<Identity>> RolesByAssociationType { get; }
    }
}
