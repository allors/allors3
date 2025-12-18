// <copyright file="ConcurrencyException.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;

    /// <summary>
    /// Exception thrown when a concurrency conflict is detected during commit.
    /// This occurs when an object has been modified by another transaction
    /// between when this transaction read the object and when it tried to commit.
    /// </summary>
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(long objectId, long currentVersion, long expectedVersion)
            : base($"Concurrency conflict on object {objectId}: expected version {expectedVersion}, but current version is {currentVersion}")
        {
            this.ObjectId = objectId;
            this.CurrentVersion = currentVersion;
            this.ExpectedVersion = expectedVersion;
        }

        public long ObjectId { get; }

        public long CurrentVersion { get; }

        public long ExpectedVersion { get; }
    }
}
