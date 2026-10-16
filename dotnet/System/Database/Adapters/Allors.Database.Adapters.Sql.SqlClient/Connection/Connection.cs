// <copyright file="Connection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data;
    using Microsoft.Data.SqlClient;

    public class Connection : IConnection
    {
        internal Database Database { get; }

        internal Connection(Database database) => this.Database = database;

        protected SqlConnection SqlConnection { get; private set; }

        protected SqlTransaction SqlTransaction { get; private set; }

        public ICommand CreateCommand()
        {
            this.EnsureOpen();

            var sqlCommand = this.SqlConnection.CreateCommand();
            sqlCommand.Transaction = this.SqlTransaction;
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
                if (this.SqlTransaction != null)
                {
                    this.SqlTransaction.Commit();
                    committed?.Invoke();
                }
            }
            finally
            {
                this.SqlTransaction = null;
                this.SqlConnection?.Close();
                this.SqlConnection = null;
            }
        }

        public void Rollback()
        {
            try
            {
                this.SqlTransaction?.Rollback();
            }
            finally
            {
                this.SqlTransaction = null;
                this.SqlConnection?.Close();
                this.SqlConnection = null;
            }
        }

        internal PopulationMetadata OpenWrite()
        {
            this.EnsureOpen();
            return new PopulationMetadataStore(this.Database).Read(this.SqlConnection, this.SqlTransaction, fence: true);
        }

        internal PopulationMetadata OpenRead(out Action verifySnapshot)
        {
            this.EnsureOpen();
            var connection = this.SqlConnection;
            var transaction = this.SqlTransaction;
            verifySnapshot = () =>
            {
                if (!ReferenceEquals(connection, this.SqlConnection) || !ReferenceEquals(transaction, this.SqlTransaction)
                    || connection.State != ConnectionState.Open || !ReferenceEquals(transaction.Connection, connection)
                    || (transaction.IsolationLevel != IsolationLevel.Snapshot))
                {
                    throw new InvalidOperationException("The protected SQL snapshot has ended, changed, or has unsupported isolation. Discard this read scope and open a new one.");
                }
            };
            verifySnapshot();
            return new PopulationMetadataStore(this.Database).Read(connection, transaction);
        }

        private void EnsureOpen()
        {
            if (this.SqlConnection == null)
            {
                this.SqlConnection = new SqlConnection(this.Database.ConnectionString);
                this.SqlConnection.Open();
                this.SqlTransaction = this.SqlConnection.BeginTransaction(IsolationLevel.Snapshot);
            }
        }

        protected ICommand CreateCommand(Mapping mapping, SqlCommand sqlCommand) => new Command(mapping, sqlCommand);
    }
}
