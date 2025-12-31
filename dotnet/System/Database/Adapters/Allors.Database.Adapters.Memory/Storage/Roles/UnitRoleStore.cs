// <copyright file="UnitRoleStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Storage.Roles
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Column-oriented store for unit role values (string, int, DateTime, etc.).
    /// Uses a dictionary-based sparse storage which is efficient for most workloads.
    /// </summary>
    internal sealed class UnitRoleStore : IUnitRoleStore
    {
        // Sparse storage: only objects with values are stored
        private readonly ConcurrentDictionary<long, object> valuesByObjectId;

        // Reverse index for value lookups (used by indexed queries)
        private readonly ConcurrentDictionary<object, ConcurrentDictionary<long, byte>> objectIdsByValue;

        // Whether to maintain reverse index (only for indexed roles)
        private readonly bool maintainReverseIndex;

        internal UnitRoleStore(IRoleType roleType, bool maintainReverseIndex = false)
        {
            this.RoleType = roleType;
            this.maintainReverseIndex = maintainReverseIndex;
            this.valuesByObjectId = new ConcurrentDictionary<long, object>();
            this.objectIdsByValue = maintainReverseIndex
                ? new ConcurrentDictionary<object, ConcurrentDictionary<long, byte>>()
                : null;
        }

        /// <inheritdoc />
        public IRoleType RoleType { get; }

        /// <inheritdoc />
        public int Count => this.valuesByObjectId.Count;

        /// <inheritdoc />
        public bool HasValue(long objectId) => this.valuesByObjectId.ContainsKey(objectId);

        /// <inheritdoc />
        public object GetValue(long objectId) =>
            this.valuesByObjectId.TryGetValue(objectId, out var value) ? value : null;

        /// <inheritdoc />
        public void SetValue(long objectId, object value)
        {
            if (value == null)
            {
                this.RemoveValue(objectId);
                return;
            }

            // Get old value for reverse index update
            object oldValue = null;
            if (this.maintainReverseIndex)
            {
                this.valuesByObjectId.TryGetValue(objectId, out oldValue);
            }

            // Set new value
            this.valuesByObjectId[objectId] = value;

            // Update reverse index
            if (this.maintainReverseIndex)
            {
                this.UpdateReverseIndex(objectId, oldValue, value);
            }
        }

        /// <inheritdoc />
        public void RemoveValue(long objectId)
        {
            if (this.valuesByObjectId.TryRemove(objectId, out var oldValue))
            {
                if (this.maintainReverseIndex && oldValue != null)
                {
                    this.RemoveFromReverseIndex(objectId, oldValue);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<long> GetObjectIdsWithValue(object value)
        {
            if (!this.maintainReverseIndex)
            {
                // Fall back to linear scan if no reverse index
                foreach (var kvp in this.valuesByObjectId)
                {
                    if (Equals(kvp.Value, value))
                    {
                        yield return kvp.Key;
                    }
                }
            }
            else if (this.objectIdsByValue.TryGetValue(value, out var objectIds))
            {
                foreach (var objectId in objectIds.Keys)
                {
                    yield return objectId;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<long, object>> GetAll() => this.valuesByObjectId;

        /// <inheritdoc />
        public void Clear()
        {
            this.valuesByObjectId.Clear();
            this.objectIdsByValue?.Clear();
        }

        private void UpdateReverseIndex(long objectId, object oldValue, object newValue)
        {
            // Remove from old value's set
            if (oldValue != null && !Equals(oldValue, newValue))
            {
                this.RemoveFromReverseIndex(objectId, oldValue);
            }

            // Add to new value's set
            if (newValue != null)
            {
                var objectIds = this.objectIdsByValue.GetOrAdd(newValue, _ => new ConcurrentDictionary<long, byte>());
                objectIds[objectId] = 0;
            }
        }

        private void RemoveFromReverseIndex(long objectId, object value)
        {
            if (this.objectIdsByValue.TryGetValue(value, out var objectIds))
            {
                objectIds.TryRemove(objectId, out _);

                // Clean up empty sets
                if (objectIds.IsEmpty)
                {
                    this.objectIdsByValue.TryRemove(value, out _);
                }
            }
        }
    }
}
