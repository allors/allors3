// <copyright file="VersionClock.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Concurrency
{
    using System.Threading;

    /// <summary>
    /// Thread-safe monotonically increasing version clock for MVCC.
    /// Provides unique, ordered version numbers for transaction commits.
    /// Uses lock-free atomic operations for high throughput.
    /// </summary>
    internal sealed class VersionClock
    {
        private long currentVersion;

        internal VersionClock()
        {
            this.currentVersion = 0;
        }

        /// <summary>
        /// Gets the current version without incrementing.
        /// </summary>
        internal long Current => Interlocked.Read(ref this.currentVersion);

        /// <summary>
        /// Atomically increments and returns the new version number.
        /// This is used when a transaction commits to get its commit version.
        /// </summary>
        internal long Next() => Interlocked.Increment(ref this.currentVersion);

        /// <summary>
        /// Updates the current version if the given version is higher.
        /// Used during database load/restore operations.
        /// </summary>
        internal void UpdateIfHigher(long version)
        {
            long current;
            do
            {
                current = Interlocked.Read(ref this.currentVersion);
                if (version <= current)
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref this.currentVersion, version, current) != current);
        }

        /// <summary>
        /// Resets the clock to zero. Used during database reset.
        /// </summary>
        internal void Reset() => Interlocked.Exchange(ref this.currentVersion, 0);
    }
}
