// <copyright file="ReadGuard.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    internal sealed class ReadGuard
    {
        private readonly Action verifySnapshot;
        private bool tainted;
        private bool closed;

        internal ReadGuard(Action verifySnapshot) => this.verifySnapshot = verifySnapshot;

        internal bool IsTainted => this.tainted;

        internal void EnsureValid()
        {
            if (this.closed)
            {
                this.tainted = true;
                throw new ObjectDisposedException(nameof(ReadScope), "The protected read has ended. Open a new read scope.");
            }

            if (this.tainted)
            {
                throw new InvalidOperationException("The protected read was invalidated. Discard its results and open a new read scope.");
            }

            try
            {
                this.verifySnapshot();
            }
            catch
            {
                this.tainted = true;
                throw;
            }
        }

        internal void Reject(string operation)
        {
            this.tainted = true;
            throw new InvalidOperationException($"{operation} is not permitted inside a protected read. Discard this read scope; perform writes in a separate transaction before opening a new read.");
        }

        internal T Execute<T>(Func<T> action)
        {
            this.EnsureValid();
            try
            {
                return action();
            }
            catch
            {
                this.tainted = true;
                throw;
            }
        }

        internal void Cleanup(Action action)
        {
            // Cleanup must still run after an earlier failure or owner closure.
            try
            {
                action();
            }
            catch
            {
                this.tainted = true;
                throw;
            }
        }

        internal void Close() => this.closed = true;
    }
}
