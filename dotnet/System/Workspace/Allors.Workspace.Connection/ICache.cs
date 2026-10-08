// <copyright file="ICache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;

    /// <summary>
    /// What a connection keeps of the user's view of the database: records, grants, revocations
    /// and permissions, by id. A connection that is given no cache creates a private one. A
    /// connection is used from one thread at a time, so the private cache is a plain dictionary.
    /// A cache shared between the connections of one user builds on this interface.
    /// </summary>
    public interface ICache
    {
        IRecord GetRecord(long id);

        void SetRecord(IRecord record);

        Grant GetGrant(long id);

        void SetGrant(Grant grant);

        Revocation GetRevocation(long id);

        void SetRevocation(Revocation revocation);

        bool HasPermission(long id);

        void SetPermission(Permission permission);

        /// <summary>
        /// The id of the permission for the operation on the operand type of the class, or 0
        /// when the cache holds no such permission.
        /// </summary>
        long GetPermission(IClass @class, IOperandType operandType, Operations operation);
    }
}
