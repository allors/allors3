// <copyright file="IProfile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Threading.Tasks;
    using Allors;
    using Allors.Workspace;
    using Allors.Workspace.Connection;
    using Allors.Workspace.Meta;
    using Xunit;

    public interface IProfile : IAsyncLifetime
    {
        IWorkspace CreateExclusiveWorkspace();

        IWorkspace CreateWorkspace();

        /// <summary>
        /// The connection of the signed-in user that <see cref="Workspace"/> is built on.
        /// </summary>
        IDatabaseConnection DatabaseConnection { get; }

        IWorkspace Workspace { get; }

        Task Login(string userName);

        /// <summary>
        /// A transport of its own, authenticated as the user, for a connection the test builds
        /// itself.
        /// </summary>
        ITransport CreateTransport(string userName);

        /// <summary>
        /// Takes the permission for the operation on the role type away from the Administrator
        /// role, in the database, so that the grant of the administrators changes version.
        /// </summary>
        Task RemoveAdministratorPermission(IRoleType roleType, Operations operation);

        /// <summary>
        /// Adds the permission for the operation on the role type to the revocation that the
        /// Denied objects carry, in the database, so that the revocation changes version.
        /// </summary>
        Task DenyPermission(IRoleType roleType, Operations operation);
    }
}
