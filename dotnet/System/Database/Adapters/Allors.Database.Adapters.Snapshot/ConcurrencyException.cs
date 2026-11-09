// <copyright file="ConcurrencyException.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System.Data.Common;

    /// <summary>
    /// Thrown by a commit when another transaction changed or deleted an object that this transaction
    /// changed or deleted, after this transaction read it. It is a <see cref="DbException"/>, as the update
    /// conflict of a database server is, so callers treat both alike: roll back and redo the work.
    /// </summary>
    public sealed class ConcurrencyException : DbException
    {
        internal ConcurrencyException(string message)
            : base(message)
        {
        }
    }
}
