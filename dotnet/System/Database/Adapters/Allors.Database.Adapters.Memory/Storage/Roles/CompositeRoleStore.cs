// <copyright file="CompositeRoleStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Storage.Roles
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Column-oriented store for single composite role values (one-to-one, many-to-one).
    /// Stores the target object ID rather than object references for better memory efficiency.
    /// Maintains a reverse index for efficient association lookups.
    /// </summary>
    internal sealed class CompositeRoleStore : ICompositeRoleStore
    {
        // Forward index: source object ID -> target object ID
        private readonly ConcurrentDictionary<long, long> targetBySourceId;

        // Reverse index: target object ID -> set of source object IDs
        // This enables efficient "who points to this object?" queries
        private readonly ConcurrentDictionary<long, ConcurrentDictionary<long, byte>> sourceIdsByTargetId;

        internal CompositeRoleStore(IRoleType roleType)
        {
            this.RoleType = roleType;
            this.targetBySourceId = new ConcurrentDictionary<long, long>();
            this.sourceIdsByTargetId = new ConcurrentDictionary<long, ConcurrentDictionary<long, byte>>();
        }

        /// <inheritdoc />
        public IRoleType RoleType { get; }

        /// <inheritdoc />
        public int Count => this.targetBySourceId.Count;

        /// <inheritdoc />
        public bool HasValue(long objectId) => this.targetBySourceId.ContainsKey(objectId);

        /// <inheritdoc />
        public long? GetValue(long objectId) =>
            this.targetBySourceId.TryGetValue(objectId, out var targetId) ? targetId : null;

        /// <inheritdoc />
        public void SetValue(long objectId, long? targetId)
        {
            if (targetId == null)
            {
                this.RemoveValue(objectId);
                return;
            }

            // Get old target for reverse index update
            var hadOldTarget = this.targetBySourceId.TryGetValue(objectId, out var oldTargetId);

            // Set new target
            this.targetBySourceId[objectId] = targetId.Value;

            // Update reverse index
            if (hadOldTarget && oldTargetId != targetId.Value)
            {
                this.RemoveFromReverseIndex(objectId, oldTargetId);
            }

            this.AddToReverseIndex(objectId, targetId.Value);
        }

        /// <inheritdoc />
        public void RemoveValue(long objectId)
        {
            if (this.targetBySourceId.TryRemove(objectId, out var oldTargetId))
            {
                this.RemoveFromReverseIndex(objectId, oldTargetId);
            }
        }

        /// <inheritdoc />
        public IEnumerable<long> GetObjectIdsPointingTo(long targetId)
        {
            if (this.sourceIdsByTargetId.TryGetValue(targetId, out var sourceIds))
            {
                return sourceIds.Keys;
            }

            return [];
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<long, long>> GetAll() => this.targetBySourceId;

        /// <inheritdoc />
        public void Clear()
        {
            this.targetBySourceId.Clear();
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
