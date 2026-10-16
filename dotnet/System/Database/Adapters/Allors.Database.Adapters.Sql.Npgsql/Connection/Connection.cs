// <copyright file="Connection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Data;
    using global::Npgsql;

    public class Connection : IConnection
    {
        internal readonly Database Database;

        internal Connection(Database database) => this.Database = database;

        internal NpgsqlConnection NpgsqlConnection { get; private set; }

        internal NpgsqlTransaction NpgsqlTransaction { get; private set; }

        public ICommand CreateCommand()
        {
            this.EnsureOpen();

            var sqlCommand = this.NpgsqlConnection.CreateCommand();
            sqlCommand.Transaction = this.NpgsqlTransaction;
            if (this.Database.CommandTimeout.HasValue)
            {
                sqlCommand.CommandTimeout = this.Database.CommandTimeout.Value;
            }

            return this.CreateCommand((Mapping)this.Database.Mapping, sqlCommand);
        }

        public void Commit() => this.Commit(null);

        internal void Commit(Action committed)
        {
            try
            {
                if (this.NpgsqlTransaction != null)
                {
                    this.NpgsqlTransaction.Commit();
                    committed?.Invoke();
                }
            }
            finally
            {
                this.NpgsqlTransaction = null;

                if (this.NpgsqlConnection != null)
                {
                    this.NpgsqlConnection?.Close();
                }

                this.NpgsqlConnection = null;
            }
        }

        public void Rollback()
        {
            try
            {
                if (this.NpgsqlTransaction != null)
                {
                    this.NpgsqlTransaction?.Rollback();
                }
            }
            finally
            {
                this.NpgsqlTransaction = null;

                if (this.NpgsqlConnection != null)
                {
                    this.NpgsqlConnection?.Close();
                }

                this.NpgsqlConnection = null;
            }
        }

        internal PopulationMetadata OpenWrite()
        {
            this.EnsureOpen();
            return new PopulationMetadataStore(this.Database).Read(this.NpgsqlConnection, this.NpgsqlTransaction, fence: true);
        }

        internal PopulationMetadata OpenRead(out Action verifySnapshot)
        {
            this.EnsureOpen();
            var connection = this.NpgsqlConnection;
            var transaction = this.NpgsqlTransaction;
            verifySnapshot = () =>
            {
                if (!ReferenceEquals(connection, this.NpgsqlConnection) || !ReferenceEquals(transaction, this.NpgsqlTransaction)
                    || connection.State != ConnectionState.Open || !ReferenceEquals(transaction.Connection, connection)
                    || (transaction.IsolationLevel != IsolationLevel.RepeatableRead && transaction.IsolationLevel != IsolationLevel.Snapshot))
                {
                    throw new InvalidOperationException("The protected SQL snapshot has ended, changed, or has unsupported isolation. Discard this read scope and open a new one.");
                }
            };
            verifySnapshot();
            return new PopulationMetadataStore(this.Database).Read(connection, transaction);
        }

        private void EnsureOpen()
        {
            if (this.NpgsqlConnection == null)
            {
                this.NpgsqlConnection = new NpgsqlConnection(this.Database.ConnectionString);
                this.NpgsqlConnection.Open();
                this.NpgsqlTransaction = this.NpgsqlConnection.BeginTransaction(this.Database.IsolationLevel ?? Database.DefaultIsolationLevel);
            }
        }

        private Command CreateCommand(Mapping mapping, NpgsqlCommand sqlCommand) => new Command(mapping, sqlCommand);
    }
}
