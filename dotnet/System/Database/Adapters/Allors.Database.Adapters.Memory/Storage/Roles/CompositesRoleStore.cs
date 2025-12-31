// <copyright file="CompositesRoleStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Storage.Roles
{
    using System.Collections.Concurrent;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    /// <summary>
    /// Column-oriented store for multiple composite role values (one-to-many, many-to-many).
    /// Uses FrozenSet for immutable storage of target object ID sets.
    /// Maintains a reverse index for efficient association lookups.
    /// </summary>
    internal sealed class CompositesRoleStore : ICompositesRoleStore
    {
        private static readonly FrozenSet<long> EmptySet = FrozenSet<long>.Empty;

        // Forward index: source object ID -> set of target object IDs
        private readonly ConcurrentDictionary<long, FrozenSet<long>> targetsBySourceId;

        // Reverse index: target object ID -> set of source object IDs
        // This enables efficient "who points to this object?" queries
        private readonly ConcurrentDictionary<long, ConcurrentDictionary<long, byte>> sourceIdsByTargetId;

        internal CompositesRoleStore(IRoleType roleType)
        {
            this.RoleType = roleType;
            this.targetsBySourceId = new ConcurrentDictionary<long, FrozenSet<long>>();
            this.sourceIdsByTargetId = new ConcurrentDictionary<long, ConcurrentDictionary<long, byte>>();
        }

        /// <inheritdoc />
        public IRoleType RoleType { get; }

        /// <inheritdoc />
        public int Count => this.targetsBySourceId.Count;

        /// <inheritdoc />
        public bool HasValue(long objectId) => this.targetsBySourceId.ContainsKey(objectId);

        /// <inheritdoc />
        public FrozenSet<long> GetValues(long objectId) =>
            this.targetsBySourceId.TryGetValue(objectId, out var targets) ? targets : EmptySet;

        /// <inheritdoc />
        public void AddValue(long objectId, long targetId)
        {
            var current = this.GetValues(objectId);
            if (current.Contains(targetId))
            {
                return; // Already present
            }

            // Create new set with added target
            var newSet = current.ToHashSet();
            newSet.Add(targetId);
            this.targetsBySourceId[objectId] = newSet.ToFrozenSet();

            // Update reverse index
            this.AddToReverseIndex(objectId, targetId);
        }

        /// <inheritdoc />
        public void RemoveTargetValue(long objectId, long targetId)
        {
            if (!this.targetsBySourceId.TryGetValue(objectId, out var current))
            {
                return;
            }

            if (!current.Contains(targetId))
            {
                return;
            }

            // Update reverse index
            this.RemoveFromReverseIndex(objectId, targetId);

            // Create new set without removed target
            if (current.Count == 1)
            {
                this.targetsBySourceId.TryRemove(objectId, out _);
            }
            else
            {
                var newSet = current.ToHashSet();
                newSet.Remove(targetId);
                this.targetsBySourceId[objectId] = newSet.ToFrozenSet();
            }
        }

        /// <inheritdoc />
        public void SetValues(long objectId, IEnumerable<long> targetIds)
        {
            var newTargets = targetIds?.ToHashSet() ?? [];

            if (newTargets.Count == 0)
            {
                this.RemoveValue(objectId);
                return;
            }

            // Get old targets for reverse index update
            var oldTargets = this.GetValues(objectId);

            // Set new targets
            this.targetsBySourceId[objectId] = newTargets.ToFrozenSet();

            // Update reverse index: remove old, add new
            foreach (var oldTarget in oldTargets)
            {
                if (!newTargets.Contains(oldTarget))
                {
                    this.RemoveFromReverseIndex(objectId, oldTarget);
                }
            }

            foreach (var newTarget in newTargets)
            {
                if (!oldTargets.Contains(newTarget))
                {
                    this.AddToReverseIndex(objectId, newTarget);
                }
            }
        }

        /// <inheritdoc />
        public void RemoveValue(long objectId)
        {
            if (this.targetsBySourceId.TryRemove(objectId, out var oldTargets))
            {
                foreach (var targetId in oldTargets)
                {
                    this.RemoveFromReverseIndex(objectId, targetId);
                }
            }
        }

        /// <summary>
        /// Gets all source object IDs that point to the specified target.
        /// Used for association queries.
        /// </summary>
        internal IEnumerable<long> GetObjectIdsPointingTo(long targetId)
        {
            if (this.sourceIdsByTargetId.TryGetValue(targetId, out var sourceIds))
            {
                return sourceIds.Keys;
            }

            return [];
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<long, FrozenSet<long>>> GetAll() => this.targetsBySourceId;

        /// <inheritdoc />
        public void Clear()
        {
            this.targetsBySourceId.Clear();
            this.sourceIdsByTargetId.Clear();
        }

        private void AddToReverseIndex(long sourceId, long targetId)
        {
            var sourceIds = this.sourceIdsByTargetId.GetOrAdd(targetId, _ => new ConcurrentDictionary<long, byte>());
            sourceIds[sourceId] = 0;
        }

        private void RemoveFromReverseIndex(long sourceId, long targetId)
        {
            if (this.sourceIdsByTargetId.TryGetValue(targetId, out var sourceIds))
            {
                sourceIds.TryRemove(sourceId, out _);

                // Clean up empty sets
                if (sourceIds.IsEmpty)
                {
                    this.sourceIdsByTargetId.TryRemove(targetId, out _);
                }
            }
        }
    }
}
