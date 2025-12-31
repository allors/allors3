// <copyright file="LockStripe.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Concurrency
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// Lock striping for parallel commits. Uses multiple locks (stripes) to reduce contention.
    /// Objects are assigned to stripes based on their ID, allowing non-conflicting
    /// transactions to commit in parallel.
    /// Uses .NET 10 Lock class for better performance than Monitor.
    /// </summary>
    internal sealed class LockStripe
    {
        // Default to 64 stripes (power of 2 for efficient modulo via bitwise AND)
        private const int DefaultStripeCount = 64;
        private const int StripeMask = DefaultStripeCount - 1;

        private readonly Lock[] stripes;

        internal LockStripe() : this(DefaultStripeCount)
        {
        }

        internal LockStripe(int stripeCount)
        {
            // Ensure stripe count is power of 2
            if ((stripeCount & (stripeCount - 1)) != 0)
            {
                throw new ArgumentException("Stripe count must be a power of 2", nameof(stripeCount));
            }

            this.stripes = new Lock[stripeCount];
            for (var i = 0; i < stripeCount; i++)
            {
                this.stripes[i] = new Lock();
            }
        }

        /// <summary>
        /// Gets the stripe index for an object ID.
        /// Uses fast bitwise AND for power-of-2 stripe count.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetStripeIndex(long objectId) => (int)(objectId & StripeMask);

        /// <summary>
        /// Gets the lock for a specific stripe.
        /// </summary>
        internal Lock GetStripe(int stripeIndex) => this.stripes[stripeIndex];

        /// <summary>
        /// Gets the lock for a specific object ID.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Lock GetStripeForObject(long objectId) => this.stripes[this.GetStripeIndex(objectId)];

        /// <summary>
        /// Gets the unique stripe indices for a set of object IDs, sorted to prevent deadlocks.
        /// </summary>
        internal int[] GetSortedStripeIndices(IEnumerable<long> objectIds)
        {
            var stripeIndices = new SortedSet<int>();
            foreach (var objectId in objectIds)
            {
                stripeIndices.Add(this.GetStripeIndex(objectId));
            }

            var result = new int[stripeIndices.Count];
            stripeIndices.CopyTo(result);
            return result;
        }

        /// <summary>
        /// Acquires locks for all specified stripe indices.
        /// Indices MUST be sorted to prevent deadlocks.
        /// Returns a disposable that releases all locks when disposed.
        /// </summary>
        internal LockAcquisition AcquireLocks(int[] sortedStripeIndices)
        {
            // Acquire locks in sorted order
            var acquiredCount = 0;
            try
            {
                for (var i = 0; i < sortedStripeIndices.Length; i++)
                {
                    this.stripes[sortedStripeIndices[i]].Enter();
                    acquiredCount++;
                }

                return new LockAcquisition(this.stripes, sortedStripeIndices);
            }
            catch
            {
                // Release any locks acquired before failure (in reverse order)
                for (var i = acquiredCount - 1; i >= 0; i--)
                {
                    this.stripes[sortedStripeIndices[i]].Exit();
                }

                throw;
            }
        }

        /// <summary>
        /// Attempts to acquire locks for all specified stripe indices without blocking.
        /// Indices MUST be sorted to prevent deadlocks.
        /// Returns null if any lock cannot be acquired immediately.
        /// </summary>
        internal LockAcquisition TryAcquireLocks(int[] sortedStripeIndices, TimeSpan timeout)
        {
            var acquiredCount = 0;
            try
            {
                for (var i = 0; i < sortedStripeIndices.Length; i++)
                {
                    if (!this.stripes[sortedStripeIndices[i]].TryEnter(timeout))
                    {
                        // Release any locks acquired before failure (in reverse order)
                        for (var j = acquiredCount - 1; j >= 0; j--)
                        {
                            this.stripes[sortedStripeIndices[j]].Exit();
                        }

                        return null;
                    }

                    acquiredCount++;
                }

                return new LockAcquisition(this.stripes, sortedStripeIndices);
            }
            catch
            {
                // Release any locks acquired before failure (in reverse order)
                for (var i = acquiredCount - 1; i >= 0; i--)
                {
                    this.stripes[sortedStripeIndices[i]].Exit();
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Represents acquired locks that should be released together.
    /// Implements IDisposable for use in using statements.
    /// </summary>
    internal sealed class LockAcquisition : IDisposable
    {
        private readonly Lock[] stripes;
        private readonly int[] stripeIndices;
        private bool disposed;

        internal LockAcquisition(Lock[] stripes, int[] stripeIndices)
        {
            this.stripes = stripes;
            this.stripeIndices = stripeIndices;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            // Release locks in reverse order
            for (var i = this.stripeIndices.Length - 1; i >= 0; i--)
            {
                this.stripes[this.stripeIndices[i]].Exit();
            }
        }
    }
}
