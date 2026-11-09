// <copyright file="Snapshot.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System.Collections.Immutable;
    using Meta;

    /// <summary>
    /// The committed state of a database at one point in time. A snapshot is immutable: a commit
    /// publishes a new snapshot that shares its unchanged structure with the previous one.
    /// </summary>
    internal sealed class Snapshot
    {
        internal Snapshot(long epoch, ImmutableDictionary<long, Record> recordById, ImmutableDictionary<IClass, ImmutableSortedSet<long>> objectIdsByClass)
        {
            this.Epoch = epoch;
            this.RecordById = recordById;
            this.ObjectIdsByClass = objectIdsByClass;
        }

        /// <summary>
        /// Gets the epoch, which changes every time the database is initialised or loaded. A transaction
        /// that started in an earlier epoch can not commit.
        /// </summary>
        internal long Epoch { get; }

        internal ImmutableDictionary<long, Record> RecordById { get; }

        internal ImmutableDictionary<IClass, ImmutableSortedSet<long>> ObjectIdsByClass { get; }

        internal static Snapshot Empty(long epoch) =>
            new Snapshot(epoch, ImmutableDictionary<long, Record>.Empty, ImmutableDictionary<IClass, ImmutableSortedSet<long>>.Empty);
    }
}
