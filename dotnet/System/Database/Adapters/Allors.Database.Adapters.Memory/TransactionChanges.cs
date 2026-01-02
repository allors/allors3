// <copyright file="TransactionChanges.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System.Collections.Generic;

/// <summary>
/// Collected changes from a transaction for atomic commit.
/// Uses SlotSnapshot for efficient slot-based storage.
/// </summary>
internal sealed class TransactionChanges
{
    internal TransactionChanges()
    {
        this.NewSnapshots = new List<SlotSnapshot>();
        this.ModifiedSnapshots = new List<ModifiedSnapshot>();
        this.DeletedObjectIds = new HashSet<long>();
    }

    /// <summary>
    /// New object snapshots to be added.
    /// </summary>
    internal List<SlotSnapshot> NewSnapshots { get; }

    /// <summary>
    /// Modified object snapshots with original version for optimistic concurrency.
    /// </summary>
    internal List<ModifiedSnapshot> ModifiedSnapshots { get; }

    /// <summary>
    /// IDs of objects to be deleted.
    /// </summary>
    internal HashSet<long> DeletedObjectIds { get; }
}

/// <summary>
/// Represents a modified object's snapshot along with its original version
/// for optimistic concurrency checking.
/// </summary>
internal readonly struct ModifiedSnapshot
{
    public readonly SlotSnapshot Snapshot;
    public readonly long OriginalVersion;

    public ModifiedSnapshot(SlotSnapshot snapshot, long originalVersion)
    {
        this.Snapshot = snapshot;
        this.OriginalVersion = originalVersion;
    }
}
