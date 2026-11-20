// <copyright file="Connection.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using Microsoft.Data.SqlClient;

    public class Connection : IConnection
    {
        // The failure of the first statement of the transaction that failed. On some errors SQL Server ends the
        // transaction, and then executes what follows outside a transaction. On others it ends the statement
        // only, and a flush has forgotten the changes that it handed to that statement. Either way only a
        // rollback is left.
        private Exception failure;

        internal Database Database { get; }

        internal Connection(Database database) => this.Database = database;

        protected SqlConnection SqlConnection { get; private set; }

        protected SqlTransaction SqlTransaction { get; private set; }

        public ICommand CreateCommand()
        {
            if (this.SqlConnection == null)
            {
                this.SqlConnection = new SqlConnection(this.Database.ConnectionString);
                this.SqlConnection.Open();
                this.SqlTransaction = this.SqlConnection.BeginTransaction(System.Data.IsolationLevel.Snapshot);
            }

            var sqlCommand = this.SqlConnection.CreateCommand();
            sqlCommand.Transaction = this.SqlTransaction;
            if (this.Database.CommandTimeout.HasValue)
            {
                sqlCommand.CommandTimeout = this.Database.CommandTimeout.Value;
            }

            return this.CreateCommand((Mapping)this.Database.Mapping, sqlCommand);
        }

        public void Commit()
        {
            this.AssertNotFailed(this.SqlTransaction);

            try
            {
                this.SqlTransaction?.Commit();
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
                this.failure = null;
            }
        }

        /// <summary>
        /// Refuses the statement or the commit that is about to run when the transaction has failed.
        /// </summary>
        internal void AssertNotFailed(SqlTransaction transaction)
        {
            // A command retained across a commit or rollback still belongs to its closed connection.
            if (transaction == null || transaction != this.SqlTransaction)
            {
                return;
            }

            if (this.failure != null)
            {
                throw new TransactionFailedException("A statement of this transaction failed, so the transaction takes no further statement and no commit. Roll back and redo the work. The statement failed with: " + this.failure.Message, this.failure);
            }

            // SQL Server can end the transaction without a failure that reaches this connection: the driver
            // drops an error that arrives while a reader is closed. A SqlTransaction that has ended has no
            // connection.
            if (transaction.Connection == null)
            {
                throw new TransactionFailedException("SQL Server has ended the database transaction, so this transaction takes no further statement and no commit. Roll back and redo the work.", null);
            }
        }

        /// <summary>
        /// Remembers the failure of a statement until the rollback.
        /// </summary>
        internal void OnStatementFailed(SqlTransaction transaction, Exception exception)
        {
            if (transaction != null && transaction == this.SqlTransaction)
            {
                this.failure ??= exception;
            }
        }

        protected ICommand CreateCommand(Mapping mapping, SqlCommand sqlCommand) => new Command(this, this.SqlTransaction, mapping, sqlCommand);
    }
}
