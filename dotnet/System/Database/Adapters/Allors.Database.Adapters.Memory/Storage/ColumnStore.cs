// <copyright file="ColumnStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Storage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using Meta;
    using Roles;

    /// <summary>
    /// Central column-oriented store that manages all role data.
    /// Instead of storing dictionaries per object, stores all values of a specific role
    /// across all objects in one data structure (columnar layout).
    /// This provides better cache locality and reduced per-object overhead.
    /// </summary>
    internal sealed class ColumnStore
    {
        // Object metadata registry
        private readonly ObjectRegistry objectRegistry;

        // Role stores by role type (the "columns")
        private readonly ConcurrentDictionary<IRoleType, IRoleStore> roleStores;

        // Indexed role types (for reverse lookups)
        private readonly HashSet<IRoleType> indexedRoleTypes;

        internal ColumnStore()
        {
            this.objectRegistry = new ObjectRegistry();
            this.roleStores = new ConcurrentDictionary<IRoleType, IRoleStore>();
            this.indexedRoleTypes = [];
        }

        /// <summary>
        /// Gets the object registry for type and version management.
        /// </summary>
        internal ObjectRegistry ObjectRegistry => this.objectRegistry;

        /// <summary>
        /// Gets the total number of objects in the store.
        /// </summary>
        internal int ObjectCount => this.objectRegistry.Count;

        /// <summary>
        /// Registers a role type for indexing.
        /// Indexed roles support reverse lookups (e.g., find all objects with value X).
        /// </summary>
        internal void RegisterIndexedRole(IRoleType roleType)
        {
            this.indexedRoleTypes.Add(roleType);
        }

        /// <summary>
        /// Registers a new object in the store.
        /// </summary>
        internal void RegisterObject(long objectId, IClass objectType, long version)
        {
            this.objectRegistry.Register(objectId, objectType, version);
        }

        /// <summary>
        /// Unregisters an object and removes all its role values.
        /// </summary>
        internal void UnregisterObject(long objectId)
        {
            // Remove all role values for this object
            foreach (var roleStore in this.roleStores.Values)
            {
                roleStore.RemoveValue(objectId);
            }

            this.objectRegistry.Unregister(objectId);
        }

        /// <summary>
        /// Gets the object type for an object.
        /// </summary>
        internal IClass GetObjectType(long objectId) =>
            this.objectRegistry.GetObjectType(objectId);

        /// <summary>
        /// Gets the version for an object.
        /// </summary>
        internal long GetVersion(long objectId) =>
            this.objectRegistry.GetVersion(objectId);

        /// <summary>
        /// Updates the version for an object.
        /// </summary>
        internal void UpdateVersion(long objectId, long newVersion) =>
            this.objectRegistry.UpdateVersion(objectId, newVersion);

        /// <summary>
        /// Checks if an object exists in the store.
        /// </summary>
        internal bool ObjectExists(long objectId) =>
            this.objectRegistry.Contains(objectId);

        /// <summary>
        /// Gets all object IDs for a specific type.
        /// </summary>
        internal IEnumerable<long> GetObjectIdsForType(IObjectType type) =>
            this.objectRegistry.GetObjectIdsForType(type);

        /// <summary>
        /// Gets all object IDs for a type as a snapshot HashSet.
        /// </summary>
        internal HashSet<long> GetObjectIdsForTypeSnapshot(IObjectType type) =>
            this.objectRegistry.GetObjectIdsForTypeSnapshot(type);

        #region Unit Roles

        /// <summary>
        /// Gets a unit role value for an object.
        /// </summary>
        internal object GetUnitRole(long objectId, IRoleType roleType)
        {
            var store = this.GetOrCreateUnitRoleStore(roleType);
            return store.GetValue(objectId);
        }

        /// <summary>
        /// Sets a unit role value for an object.
        /// </summary>
        internal void SetUnitRole(long objectId, IRoleType roleType, object value)
        {
            var store = this.GetOrCreateUnitRoleStore(roleType);
            store.SetValue(objectId, value);
        }

        /// <summary>
        /// Gets all unit role values for an object as a dictionary.
        /// Used for serialization/migration.
        /// </summary>
        internal Dictionary<IRoleType, object> GetAllUnitRoles(long objectId)
        {
            var result = new Dictionary<IRoleType, object>();
            foreach (var kvp in this.roleStores)
            {
                if (kvp.Value is IUnitRoleStore unitStore)
                {
                    var value = unitStore.GetValue(objectId);
                    if (value != null)
                    {
                        result[kvp.Key] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all object IDs with a specific unit role value.
        /// Used for index-based queries.
        /// </summary>
        internal IEnumerable<long> GetObjectIdsWithUnitRoleValue(IRoleType roleType, object value)
        {
            var store = this.GetOrCreateUnitRoleStore(roleType);
            return store.GetObjectIdsWithValue(value);
        }

        #endregion

        #region Composite Roles (One-to-One, Many-to-One)

        /// <summary>
        /// Gets a composite role value for an object.
        /// </summary>
        internal long? GetCompositeRole(long objectId, IRoleType roleType)
        {
            var store = this.GetOrCreateCompositeRoleStore(roleType);
            return store.GetValue(objectId);
        }

        /// <summary>
        /// Sets a composite role value for an object.
        /// </summary>
        internal void SetCompositeRole(long objectId, IRoleType roleType, long? targetId)
        {
            var store = this.GetOrCreateCompositeRoleStore(roleType);
            store.SetValue(objectId, targetId);
        }

        /// <summary>
        /// Gets all composite role values for an object as a dictionary.
        /// Used for serialization/migration.
        /// </summary>
        internal Dictionary<IRoleType, long> GetAllCompositeRoles(long objectId)
        {
            var result = new Dictionary<IRoleType, long>();
            foreach (var kvp in this.roleStores)
            {
                if (kvp.Value is ICompositeRoleStore compositeStore)
                {
                    var value = compositeStore.GetValue(objectId);
                    if (value.HasValue)
                    {
                        result[kvp.Key] = value.Value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all object IDs that point to a specific target (association query).
        /// </summary>
        internal IEnumerable<long> GetObjectIdsPointingToComposite(IRoleType roleType, long targetId)
        {
            var store = this.GetOrCreateCompositeRoleStore(roleType);
            return store.GetObjectIdsPointingTo(targetId);
        }

        #endregion

        #region Composites Roles (One-to-Many, Many-to-Many)

        /// <summary>
        /// Gets a composites role value for an object.
        /// </summary>
        internal FrozenSet<long> GetCompositesRole(long objectId, IRoleType roleType)
        {
            var store = this.GetOrCreateCompositesRoleStore(roleType);
            return store.GetValues(objectId);
        }

        /// <summary>
        /// Adds a target to a composites role for an object.
        /// </summary>
        internal void AddCompositesRole(long objectId, IRoleType roleType, long targetId)
        {
            var store = this.GetOrCreateCompositesRoleStore(roleType);
            store.AddValue(objectId, targetId);
        }

        /// <summary>
        /// Removes a target from a composites role for an object.
        /// </summary>
        internal void RemoveCompositesRole(long objectId, IRoleType roleType, long targetId)
        {
            var store = this.GetOrCreateCompositesRoleStore(roleType);
            store.RemoveTargetValue(objectId, targetId);
        }

        /// <summary>
        /// Sets the composites role values for an object.
        /// </summary>
        internal void SetCompositesRole(long objectId, IRoleType roleType, IEnumerable<long> targetIds)
        {
            var store = this.GetOrCreateCompositesRoleStore(roleType);
            store.SetValues(objectId, targetIds);
        }

        /// <summary>
        /// Gets all composites role values for an object as a dictionary.
        /// Used for serialization/migration.
        /// </summary>
        internal Dictionary<IRoleType, FrozenSet<long>> GetAllCompositesRoles(long objectId)
        {
            var result = new Dictionary<IRoleType, FrozenSet<long>>();
            foreach (var kvp in this.roleStores)
            {
                if (kvp.Value is ICompositesRoleStore compositesStore)
                {
                    var values = compositesStore.GetValues(objectId);
                    if (values.Count > 0)
                    {
                        result[kvp.Key] = values;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all object IDs that contain a specific target in their composites role (association query).
        /// </summary>
        internal IEnumerable<long> GetObjectIdsContainingComposite(IRoleType roleType, long targetId)
        {
            var store = this.GetOrCreateCompositesRoleStore(roleType);
            return ((CompositesRoleStore)store).GetObjectIdsPointingTo(targetId);
        }

        #endregion

        /// <summary>
        /// Clears all data from the store.
        /// </summary>
        internal void Clear()
        {
            this.objectRegistry.Clear();
            foreach (var store in this.roleStores.Values)
            {
                store.Clear();
            }

            this.roleStores.Clear();
        }

        private IUnitRoleStore GetOrCreateUnitRoleStore(IRoleType roleType)
        {
            var store = this.roleStores.GetOrAdd(roleType, rt =>
            {
                var isIndexed = this.indexedRoleTypes.Contains(rt);
                return new UnitRoleStore(rt, isIndexed);
            });

            if (store is not IUnitRoleStore unitStore)
            {
                throw new InvalidOperationException($"Role type {roleType} is not a unit role type");
            }

            return unitStore;
        }

        private ICompositeRoleStore GetOrCreateCompositeRoleStore(IRoleType roleType)
        {
            var store = this.roleStores.GetOrAdd(roleType, rt => new CompositeRoleStore(rt));

            if (store is not ICompositeRoleStore compositeStore)
            {
                throw new InvalidOperationException($"Role type {roleType} is not a composite role type");
            }

            return compositeStore;
        }

        private ICompositesRoleStore GetOrCreateCompositesRoleStore(IRoleType roleType)
        {
            var store = this.roleStores.GetOrAdd(roleType, rt => new CompositesRoleStore(rt));

            if (store is not ICompositesRoleStore compositesStore)
            {
                throw new InvalidOperationException($"Role type {roleType} is not a composites role type");
            }

            return compositesStore;
        }
    }
}
