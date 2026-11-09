// <copyright file="RestorePoint.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    /// <summary>
    /// The committed state of a <see cref="Database"/> at the moment the restore point was taken, with the
    /// position of its id sequence. <see cref="Database.Restore"/> brings the database back to it.
    /// </summary>
    public sealed class RestorePoint
    {
        internal RestorePoint(Database database, Snapshot snapshot, long nextId)
        {
            this.Database = database;
            this.Snapshot = snapshot;
            this.NextId = nextId;
        }

        internal Database Database { get; }

        internal Snapshot Snapshot { get; }

        internal long NextId { get; }
    }
}
