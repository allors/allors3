// <copyright file="ReadScope.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    /// <summary>
    /// Owns one clean SQL snapshot, including its population identity and transaction services.
    /// </summary>
    public sealed class ReadScope : IDisposable
    {
        private readonly IConnection connection;
        private readonly ReadGuard guard;
        private readonly ITransactionServices services;
        private bool disposed;

        internal ReadScope(Database database, IConnection connection, PopulationMetadata population, Action verifySnapshot)
        {
            this.connection = connection;
            this.guard = new ReadGuard(verifySnapshot);
            this.Population = population;
            try
            {
                if (population == null || population.State != PopulationState.Ready)
                {
                    throw new InvalidOperationException("A protected read requires Ready population metadata. Complete offline enrollment or maintenance before serving reads.");
                }

                this.guard.EnsureValid();
                this.services = database.Services.CreateTransactionServices();
                this.Transaction = new Transaction(database, connection, this.services, this.guard);
                // Initialization may catch a prohibited write. Its failure remains sticky.
                this.EnsureValid();
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        public Transaction Transaction { get; }

        public PopulationMetadata Population { get; }

        public void EnsureValid() => this.guard.EnsureValid();

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            var wasTainted = this.guard.IsTainted;
            this.guard.Close();
            try
            {
                this.services?.Dispose();
                if (!wasTainted && this.guard.IsTainted)
                {
                    throw new InvalidOperationException("Transaction-service cleanup attempted to use the ended protected read. Discard its results.");
                }
            }
            finally
            {
                // Do not use Transaction.Rollback: it invalidates the shared writer cache
                // and leaves a legacy transaction available to start another snapshot.
                this.connection.Rollback();
            }
        }
    }
}
