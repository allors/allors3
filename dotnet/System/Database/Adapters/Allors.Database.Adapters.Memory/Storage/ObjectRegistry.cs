// <copyright file="ObjectRegistry.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Storage
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Registry that tracks object metadata (type, version) separately from role data.
    /// Provides O(1) lookup of object information and type-based enumeration.
    /// </summary>
    internal sealed class ObjectRegistry
    {
        // Object metadata by ID
        private readonly ConcurrentDictionary<long, ObjectEntry> entriesById;

        // Type index: type -> set of object IDs
        private readonly ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>> objectIdsByType;

        internal ObjectRegistry()
        {
            this.entriesById = new ConcurrentDictionary<long, ObjectEntry>();
            this.objectIdsByType = new ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>>();
        }

        /// <summary>
        /// Gets the total number of registered objects.
        /// </summary>
        internal int Count => this.entriesById.Count;

        /// <summary>
        /// Registers a new object in the registry.
        /// </summary>
        internal void Register(long objectId, IClass objectType, long version)
        {
            var entry = new ObjectEntry(objectType, version);
            this.entriesById[objectId] = entry;

            var typeSet = this.objectIdsByType.GetOrAdd(objectType, _ => new ConcurrentDictionary<long, byte>());
            typeSet[objectId] = 0;
        }

        /// <summary>
        /// Unregisters an object from the registry.
        /// </summary>
        internal bool Unregister(long objectId)
        {
            if (this.entriesById.TryRemove(objectId, out var entry))
            {
                if (this.objectIdsByType.TryGetValue(entry.ObjectType, out var typeSet))
                {
                    typeSet.TryRemove(objectId, out _);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an object is registered.
        /// </summary>
        internal bool Contains(long objectId) => this.entriesById.ContainsKey(objectId);

        /// <summary>
        /// Tries to get the entry for an object.
        /// </summary>
        internal bool TryGetEntry(long objectId, out ObjectEntry entry) =>
            this.entriesById.TryGetValue(objectId, out entry);

        /// <summary>
        /// Gets the object type for an object, or null if not found.
        /// </summary>
        internal IClass GetObjectType(long objectId) =>
            this.entriesById.TryGetValue(objectId, out var entry) ? entry.ObjectType : null;

        /// <summary>
        /// Gets the version for an object, or -1 if not found.
        /// </summary>
        internal long GetVersion(long objectId) =>
            this.entriesById.TryGetValue(objectId, out var entry) ? entry.Version : -1;

        /// <summary>
        /// Updates the version for an object.
        /// </summary>
        internal void UpdateVersion(long objectId, long newVersion)
        {
            if (this.entriesById.TryGetValue(objectId, out var entry))
            {
                this.entriesById[objectId] = entry.WithVersion(newVersion);
            }
        }

        /// <summary>
        /// Gets all object IDs for a specific type (concrete class only).
        /// </summary>
        internal IEnumerable<long> GetObjectIdsForType(IObjectType type)
        {
            if (this.objectIdsByType.TryGetValue(type, out var typeSet))
            {
                return typeSet.Keys;
            }

            return [];
        }

        /// <summary>
        /// Gets a snapshot of all object IDs for a type as a HashSet.
        /// </summary>
        internal HashSet<long> GetObjectIdsForTypeSnapshot(IObjectType type)
        {
            if (this.objectIdsByType.TryGetValue(type, out var typeSet))
            {
                return new HashSet<long>(typeSet.Keys);
            }

            return [];
        }

        /// <summary>
        /// Gets all registered object IDs.
        /// </summary>
        internal IEnumerable<long> GetAllObjectIds() => this.entriesById.Keys;

        /// <summary>
        /// Clears all entries from the registry.
        /// </summary>
        internal void Clear()
        {
            this.entriesById.Clear();
            this.objectIdsByType.Clear();
        }
    }

    /// <summary>
    /// Immutable entry storing object metadata.
    /// Using a struct reduces allocations compared to a class.
    /// </summary>
    internal readonly struct ObjectEntry
    {
        internal ObjectEntry(IClass objectType, long version)
        {
            this.ObjectType = objectType;
            this.Version = version;
        }

        /// <summary>
        /// Gets the concrete class type of the object.
        /// </summary>
        internal IClass ObjectType { get; }

        /// <summary>
        /// Gets the current version of the object.
        /// </summary>
        internal long Version { get; }

        /// <summary>
        /// Creates a new entry with an updated version.
        /// </summary>
        internal ObjectEntry WithVersion(long newVersion) => new(this.ObjectType, newVersion);
    }
}
