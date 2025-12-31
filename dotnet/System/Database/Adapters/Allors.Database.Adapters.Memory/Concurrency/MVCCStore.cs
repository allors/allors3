// <copyright file="MVCCStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Concurrency
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Multi-Version Concurrency Control (MVCC) coordinator.
    /// Provides snapshot isolation and parallel commits for non-conflicting transactions.
    /// Uses lock striping to allow multiple transactions to commit in parallel when
    /// they touch different objects.
    /// </summary>
    internal sealed class MVCCStore
    {
        // Version clock for monotonic version numbers
        private readonly VersionClock versionClock;

        // Lock striping for parallel commits
        private readonly LockStripe lockStripe;

        // Committed object versions (objectId -> version at last commit)
        private readonly ConcurrentDictionary<long, long> committedVersions;

        internal MVCCStore()
        {
            this.versionClock = new VersionClock();
            this.lockStripe = new LockStripe();
            this.committedVersions = new ConcurrentDictionary<long, long>();
        }

        /// <summary>
        /// Gets the current global version.
        /// </summary>
        internal long CurrentVersion => this.versionClock.Current;

        /// <summary>
        /// Creates a new write set for a transaction starting now.
        /// </summary>
        internal TransactionWriteSet CreateWriteSet()
        {
            return new TransactionWriteSet(this.versionClock.Current);
        }

        /// <summary>
        /// Gets the committed version of an object.
        /// </summary>
        internal long GetCommittedVersion(long objectId)
        {
            return this.committedVersions.TryGetValue(objectId, out var version) ? version : 0;
        }

        /// <summary>
        /// Validates that no conflicts exist for the write set.
        /// Returns true if validation passes, false if there are conflicts.
        /// </summary>
        internal bool ValidateWriteSet(TransactionWriteSet writeSet)
        {
            // Check modified objects haven't changed since we read them
            foreach (var (objectId, originalVersion) in writeSet.ModifiedObjectVersions)
            {
                if (this.committedVersions.TryGetValue(objectId, out var currentVersion))
                {
                    if (currentVersion != originalVersion)
                    {
                        return false; // Conflict - object was modified by another transaction
                    }
                }
            }

            // Check deleted objects haven't changed since we read them
            foreach (var (objectId, originalVersion) in writeSet.DeletedObjectVersions)
            {
                if (this.committedVersions.TryGetValue(objectId, out var currentVersion))
                {
                    if (currentVersion != originalVersion)
                    {
                        return false; // Conflict - object was modified by another transaction
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Attempts to commit a transaction's changes with parallel commit support.
        /// Uses lock striping to allow non-conflicting transactions to commit in parallel.
        /// </summary>
        /// <param name="writeSet">The write set containing changes to commit.</param>
        /// <param name="applyChanges">Action to apply the changes while locks are held.</param>
        /// <returns>The commit result containing the new version or conflict information.</returns>
        internal CommitResult TryCommit(TransactionWriteSet writeSet, Action applyChanges)
        {
            if (writeSet.IsEmpty)
            {
                return CommitResult.Success(this.versionClock.Current);
            }

            // Get sorted stripe indices to prevent deadlocks
            var affectedIds = new List<long>(writeSet.AllAffectedObjectIds);
            var stripeIndices = this.lockStripe.GetSortedStripeIndices(affectedIds);

            // Acquire locks for affected stripes
            using var locks = this.lockStripe.AcquireLocks(stripeIndices);

            // Validate no conflicts occurred
            if (!this.ValidateWriteSet(writeSet))
            {
                return CommitResult.Conflict("Version conflict detected");
            }

            // Get new version for this commit
            var commitVersion = this.versionClock.Next();

            // Apply changes while locks are held
            applyChanges();

            // Update committed versions
            foreach (var objectId in writeSet.CreatedObjectIds)
            {
                this.committedVersions[objectId] = commitVersion;
            }

            foreach (var objectId in writeSet.ModifiedObjectVersions.Keys)
            {
                this.committedVersions[objectId] = commitVersion;
            }

            foreach (var objectId in writeSet.DeletedObjectVersions.Keys)
            {
                this.committedVersions.TryRemove(objectId, out _);
            }

            return CommitResult.Success(commitVersion);
        }

        /// <summary>
        /// Attempts to commit with a timeout for lock acquisition.
        /// </summary>
        internal CommitResult TryCommit(TransactionWriteSet writeSet, Action applyChanges, TimeSpan timeout)
        {
            if (writeSet.IsEmpty)
            {
                return CommitResult.Success(this.versionClock.Current);
            }

            // Get sorted stripe indices to prevent deadlocks
            var affectedIds = new List<long>(writeSet.AllAffectedObjectIds);
            var stripeIndices = this.lockStripe.GetSortedStripeIndices(affectedIds);

            // Try to acquire locks with timeout
            using var locks = this.lockStripe.TryAcquireLocks(stripeIndices, timeout);
            if (locks == null)
            {
                return CommitResult.Timeout("Could not acquire locks within timeout");
            }

            // Validate no conflicts occurred
            if (!this.ValidateWriteSet(writeSet))
            {
                return CommitResult.Conflict("Version conflict detected");
            }

            // Get new version for this commit
            var commitVersion = this.versionClock.Next();

            // Apply changes while locks are held
            applyChanges();

            // Update committed versions
            foreach (var objectId in writeSet.CreatedObjectIds)
            {
                this.committedVersions[objectId] = commitVersion;
            }

            foreach (var objectId in writeSet.ModifiedObjectVersions.Keys)
            {
                this.committedVersions[objectId] = commitVersion;
            }

            foreach (var objectId in writeSet.DeletedObjectVersions.Keys)
            {
                this.committedVersions.TryRemove(objectId, out _);
            }

            return CommitResult.Success(commitVersion);
        }

        /// <summary>
        /// Records that an object was created (for external use during restore/load).
        /// </summary>
        internal void RecordCreated(long objectId, long version)
        {
            this.committedVersions[objectId] = version;
            this.versionClock.UpdateIfHigher(version);
        }

        /// <summary>
        /// Resets the MVCC store.
        /// </summary>
        internal void Reset()
        {
            this.versionClock.Reset();
            this.committedVersions.Clear();
        }
    }

    /// <summary>
    /// Result of a commit operation.
    /// </summary>
    internal readonly struct CommitResult
    {
        private CommitResult(bool succeeded, long version, string errorMessage)
        {
            this.Succeeded = succeeded;
            this.Version = version;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets whether the commit succeeded.
        /// </summary>
        internal bool Succeeded { get; }

        /// <summary>
        /// Gets the commit version (valid only if succeeded).
        /// </summary>
        internal long Version { get; }

        /// <summary>
        /// Gets the error message (valid only if failed).
        /// </summary>
        internal string ErrorMessage { get; }

        /// <summary>
        /// Gets whether the failure was due to a conflict.
        /// </summary>
        internal bool IsConflict => !this.Succeeded && this.ErrorMessage?.Contains("conflict", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Gets whether the failure was due to a timeout.
        /// </summary>
        internal bool IsTimeout => !this.Succeeded && this.ErrorMessage?.Contains("timeout", StringComparison.OrdinalIgnoreCase) == true;

        internal static CommitResult Success(long version) => new(true, version, null);
        internal static CommitResult Conflict(string message) => new(false, 0, message);
        internal static CommitResult Timeout(string message) => new(false, 0, message);
    }
}
