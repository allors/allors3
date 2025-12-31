// <copyright file="IRoleStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Storage
{
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Base interface for column-oriented role storage.
    /// Stores all values of a specific role type across all objects.
    /// </summary>
    internal interface IRoleStore
    {
        /// <summary>
        /// Gets the role type this store manages.
        /// </summary>
        IRoleType RoleType { get; }

        /// <summary>
        /// Gets the number of objects that have a value for this role.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Checks if the specified object has a value for this role.
        /// </summary>
        bool HasValue(long objectId);

        /// <summary>
        /// Removes the value for the specified object.
        /// </summary>
        void RemoveValue(long objectId);

        /// <summary>
        /// Clears all values from this store.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Store for unit role values (string, int, DateTime, etc.).
    /// </summary>
    internal interface IUnitRoleStore : IRoleStore
    {
        /// <summary>
        /// Gets the value for the specified object, or null if not set.
        /// </summary>
        object GetValue(long objectId);

        /// <summary>
        /// Sets the value for the specified object.
        /// </summary>
        void SetValue(long objectId, object value);

        /// <summary>
        /// Gets all object IDs that have the specified value.
        /// Used for index-based queries.
        /// </summary>
        IEnumerable<long> GetObjectIdsWithValue(object value);

        /// <summary>
        /// Gets all (objectId, value) pairs in this store.
        /// Used for bulk operations like serialization.
        /// </summary>
        IEnumerable<KeyValuePair<long, object>> GetAll();
    }

    /// <summary>
    /// Store for single composite role values (one-to-one, many-to-one).
    /// Stores the target object ID rather than the object reference.
    /// </summary>
    internal interface ICompositeRoleStore : IRoleStore
    {
        /// <summary>
        /// Gets the target object ID for the specified object, or null if not set.
        /// </summary>
        long? GetValue(long objectId);

        /// <summary>
        /// Sets the target object ID for the specified object.
        /// </summary>
        void SetValue(long objectId, long? targetId);

        /// <summary>
        /// Gets all object IDs that reference the specified target.
        /// Used for reverse lookups (association queries).
        /// </summary>
        IEnumerable<long> GetObjectIdsPointingTo(long targetId);

        /// <summary>
        /// Gets all (objectId, targetId) pairs in this store.
        /// Used for bulk operations like serialization.
        /// </summary>
        IEnumerable<KeyValuePair<long, long>> GetAll();
    }

    /// <summary>
    /// Store for multiple composite role values (one-to-many, many-to-many).
    /// Stores sets of target object IDs.
    /// </summary>
    internal interface ICompositesRoleStore : IRoleStore
    {
        /// <summary>
        /// Gets the target object IDs for the specified object.
        /// Returns empty set if not set.
        /// </summary>
        FrozenSet<long> GetValues(long objectId);

        /// <summary>
        /// Adds a target object ID for the specified object.
        /// </summary>
        void AddValue(long objectId, long targetId);

        /// <summary>
        /// Removes a target object ID for the specified object.
        /// </summary>
        void RemoveTargetValue(long objectId, long targetId);

        /// <summary>
        /// Sets all target object IDs for the specified object.
        /// </summary>
        void SetValues(long objectId, IEnumerable<long> targetIds);

        /// <summary>
        /// Gets all (objectId, targetIds) pairs in this store.
        /// Used for bulk operations like serialization.
        /// </summary>
        IEnumerable<KeyValuePair<long, FrozenSet<long>>> GetAll();
    }
}
