// <copyright file="WriteScope.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Owns one admitted write, its population fence and cumulative affected-object identities.
    /// </summary>
    public sealed class WriteScope : IDisposable
    {
        private readonly IConnection connection;
        private readonly Action<Action> commit;
        private readonly ITransactionServices services;
        private readonly Dictionary<long, AffectedObject> affected = new Dictionary<long, AffectedObject>();
        private bool allowed;
        private bool rejected;
        private bool committing;
        private bool disposing;
        private bool disposed;

        internal WriteScope(Database database, IConnection connection, PopulationMetadata population, Guid expectedIncarnation, Action<Action> commit)
        {
            this.connection = connection;
            this.commit = commit;
            this.Population = population;
            try
            {
                if (population == null || population.State != PopulationState.Ready || population.Incarnation != expectedIncarnation)
                {
                    throw new InvalidOperationException("The write population is unavailable or has changed. Establish a new scoped connection before writing.");
                }

                this.services = database.Services.CreateTransactionServices();
                this.Transaction = new Transaction(database, connection, this.services, writeScope: this);
                if (this.rejected)
                {
                    throw new InvalidOperationException("Transaction initialization attempted a write before admission. Discard this write scope.");
                }
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        public Transaction Transaction { get; }

        public PopulationMetadata Population { get; }

        public WriteCommitState CommitState { get; private set; }

        public IReadOnlyCollection<AffectedObject> AffectedObjects => this.affected.Values;

        public void AllowWrites()
        {
            this.EnsureReadable();
            if (this.rejected || this.CommitState != WriteCommitState.NotCommitted)
            {
                this.Reject("Write admission after an invalid operation or commit");
            }

            this.allowed = true;
        }

        public void IncludeSubmitted(IStrategy strategy)
        {
            this.AssertWritable("Submitted-object capture");
            if (strategy is not Strategy sqlStrategy || !ReferenceEquals(sqlStrategy.Transaction, this.Transaction))
            {
                throw new ArgumentException("Capture a submitted object from this write transaction.", nameof(strategy));
            }

            this.Capture(sqlStrategy, AffectedObjectKind.Submitted);
        }

        public void Commit()
        {
            this.AssertWritable("Write commit");
            this.committing = true;
            try
            {
                this.Transaction.Commit();
            }
            finally
            {
                this.committing = false;
            }
        }

        internal void CommitConnection()
        {
            this.AssertTransactionCommit();
            this.CommitState = WriteCommitState.Unknown;
            this.commit(() => this.CommitState = WriteCommitState.Committed);
        }

        internal void AssertTransactionCommit()
        {
            this.AssertWritable("Transaction commit");
            if (!this.committing)
            {
                this.Reject("Direct transaction commit; commit through the write scope");
            }
        }

        internal void AssertTransactionRollback()
        {
            if (!this.disposing)
            {
                this.Reject("Direct transaction rollback; dispose the write scope");
            }
        }

        internal void Capture(Strategy strategy, AffectedObjectKind kind)
        {
            if (this.affected.TryGetValue(strategy.ObjectId, out var value))
            {
                value.Kind |= kind;
            }
            else
            {
                // Class remains available after deletion through the adapter reference.
                this.affected.Add(strategy.ObjectId, new AffectedObject(strategy.ObjectId, strategy.Reference.Class, kind));
            }
        }

        internal void EnsureReadable()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(WriteScope), "The protected write has ended. Open a new write scope.");
            }

            if (this.CommitState != WriteCommitState.NotCommitted && !this.committing && !this.disposing)
            {
                throw new InvalidOperationException("The protected write has attempted its commit. Use a new protected read for post-commit values.");
            }
        }

        internal void AssertWritable(string operation)
        {
            this.EnsureReadable();
            if (!this.allowed || this.rejected || this.CommitState != WriteCommitState.NotCommitted)
            {
                this.Reject(operation);
            }
        }

        internal void Reject(string operation)
        {
            this.EnsureReadable();
            this.rejected = true;
            throw new InvalidOperationException(operation + " is not allowed in this protected write. Validate its scope and allow writes before mutation; commit and dispose through the write scope.");
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposing = true;
            try
            {
                this.Transaction?.Rollback();
            }
            finally
            {
                this.disposed = true;
                try
                {
                    this.services?.Dispose();
                }
                finally
                {
                    this.connection.Rollback();
                }
            }
        }
    }
}
