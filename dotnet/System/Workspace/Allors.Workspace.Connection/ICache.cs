// <copyright file="ICache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using Meta;

    /// <summary>
    /// What a connection keeps of the user's view of the database: records, grants, revocations
    /// and permissions, by id. A connection that is given no cache creates a
    /// <see cref="MemoryCache"/> of its own. A cache may be shared by the connections of one
    /// user to one workspace name and one meta population: the cache checks the name and the
    /// meta population when a connection takes it, and the user when the first response of a
    /// connection binds its <see cref="Key"/>.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Raised when a record the cache held is replaced, at the moment it is: the grants,
        /// revocations and permissions the new record names may not be in yet. A connection
        /// raises its own <see cref="IDatabaseConnection.RecordChanged"/> once they are.
        /// </summary>
        event EventHandler<RecordChangedEventArgs> RecordChanged;

        /// <summary>
        /// The name of the workspace whose records the cache holds.
        /// </summary>
        string WorkspaceName { get; }

        /// <summary>
        /// The meta population the records are typed by.
        /// </summary>
        IMetaPopulation MetaPopulation { get; }

        /// <summary>
        /// The database, user, workspace name and meta fingerprint whose view the cache holds,
        /// bound by the first response of a connection; null until then and after
        /// <see cref="Clear"/>.
        /// </summary>
        CacheKey Key { get; }

        /// <summary>
        /// Binds the cache to the key, or checks it against the key it is bound to: a cache
        /// serves one user of one database.
        /// </summary>
        /// <exception cref="InvalidOperationException">The cache is bound to another key.</exception>
        void Bind(CacheKey key);

        IRecord GetRecord(long id);

        /// <summary>
        /// Keeps the record unless the cache holds a newer version of the object, and answers
        /// whether it was kept. A record of the same version replaces the one held: the grants
        /// and revocations of an object change without its version.
        /// </summary>
        bool SetRecord(IRecord record);

        /// <summary>
        /// Forgets the record of the object, if the cache holds one.
        /// </summary>
        void RemoveRecord(long id);

        /// <summary>
        /// Forgets everything, the key included.
        /// </summary>
        void Clear();

        Grant GetGrant(long id);

        /// <summary>
        /// Keeps the grant unless the cache holds a newer version of it.
        /// </summary>
        void SetGrant(Grant grant);

        Revocation GetRevocation(long id);

        /// <summary>
        /// Keeps the revocation unless the cache holds a newer version of it.
        /// </summary>
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
