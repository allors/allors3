// <copyright file="TransactionWriteSet.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Concurrency
{
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Tracks all changes made by a transaction for MVCC conflict detection.
    /// Contains the objects that were read, created, modified, or deleted.
    /// </summary>
    internal sealed class TransactionWriteSet
    {
        // Objects created in this transaction
        private readonly HashSet<long> createdObjectIds;

        // Objects modified in this transaction (with their read versions)
        private readonly Dictionary<long, long> modifiedObjectVersions;

        // Objects deleted in this transaction (with their read versions)
        private readonly Dictionary<long, long> deletedObjectVersions;

        // Objects read in this transaction (for optimistic locking validation)
        private readonly Dictionary<long, long> readObjectVersions;

        internal TransactionWriteSet(long startVersion)
        {
            this.StartVersion = startVersion;
            this.createdObjectIds = [];
            this.modifiedObjectVersions = [];
            this.deletedObjectVersions = [];
            this.readObjectVersions = [];
        }

        /// <summary>
        /// Gets the version when the transaction started.
        /// Used for snapshot isolation - only see changes up to this version.
        /// </summary>
        internal long StartVersion { get; }

        /// <summary>
        /// Gets all object IDs in this write set (created, modified, or deleted).
        /// Used for lock acquisition during commit.
        /// </summary>
        internal IEnumerable<long> AllAffectedObjectIds
        {
            get
            {
                foreach (var id in this.createdObjectIds)
                {
                    yield return id;
                }

                foreach (var id in this.modifiedObjectVersions.Keys)
                {
                    yield return id;
                }

                foreach (var id in this.deletedObjectVersions.Keys)
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// Gets the created object IDs.
        /// </summary>
        internal IReadOnlyCollection<long> CreatedObjectIds => this.createdObjectIds;

        /// <summary>
        /// Gets the modified objects with their original versions.
        /// </summary>
        internal IReadOnlyDictionary<long, long> ModifiedObjectVersions => this.modifiedObjectVersions;

        /// <summary>
        /// Gets the deleted objects with their original versions.
        /// </summary>
        internal IReadOnlyDictionary<long, long> DeletedObjectVersions => this.deletedObjectVersions;

        /// <summary>
        /// Gets whether the write set is empty (no changes).
        /// </summary>
        internal bool IsEmpty =>
            this.createdObjectIds.Count == 0 &&
            this.modifiedObjectVersions.Count == 0 &&
            this.deletedObjectVersions.Count == 0;

        /// <summary>
        /// Records that an object was created in this transaction.
        /// </summary>
        internal void RecordCreated(long objectId)
        {
            this.createdObjectIds.Add(objectId);
        }

        /// <summary>
        /// Records that an object was modified in this transaction.
        /// </summary>
        internal void RecordModified(long objectId, long originalVersion)
        {
            // Don't track modifications to objects we created
            if (this.createdObjectIds.Contains(objectId))
            {
                return;
            }

            // Only record first modification (original version)
            this.modifiedObjectVersions.TryAdd(objectId, originalVersion);
        }

        /// <summary>
        /// Records that an object was deleted in this transaction.
        /// </summary>
        internal void RecordDeleted(long objectId, long originalVersion)
        {
            // If we created it, just remove from created set
            if (this.createdObjectIds.Remove(objectId))
            {
                return;
            }

            // Remove from modified if we were just modifying it
            this.modifiedObjectVersions.Remove(objectId);

            // Record the deletion
            this.deletedObjectVersions.TryAdd(objectId, originalVersion);
        }

        /// <summary>
        /// Records that an object was read in this transaction (for optimistic read validation).
        /// </summary>
        internal void RecordRead(long objectId, long version)
        {
            // Only record first read
            this.readObjectVersions.TryAdd(objectId, version);
        }

        /// <summary>
        /// Checks if an object was created in this transaction.
        /// </summary>
        internal bool WasCreated(long objectId) => this.createdObjectIds.Contains(objectId);

        /// <summary>
        /// Checks if an object was modified in this transaction.
        /// </summary>
        internal bool WasModified(long objectId) =>
            this.modifiedObjectVersions.ContainsKey(objectId) ||
            this.createdObjectIds.Contains(objectId);

        /// <summary>
        /// Checks if an object was deleted in this transaction.
        /// </summary>
        internal bool WasDeleted(long objectId) => this.deletedObjectVersions.ContainsKey(objectId);

        /// <summary>
        /// Gets the original version of a modified object, or null if not modified.
        /// </summary>
        internal long? GetOriginalVersion(long objectId)
        {
            if (this.modifiedObjectVersions.TryGetValue(objectId, out var version))
            {
                return version;
            }

            if (this.deletedObjectVersions.TryGetValue(objectId, out version))
            {
                return version;
            }

            return null;
        }

        /// <summary>
        /// Clears all tracked changes.
        /// </summary>
        internal void Clear()
        {
            this.createdObjectIds.Clear();
            this.modifiedObjectVersions.Clear();
            this.deletedObjectVersions.Clear();
            this.readObjectVersions.Clear();
        }
    }
}
