// <copyright file="TransactionFailedException.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data.Common;

    /// <summary>
    /// Thrown by a statement and by a commit of a transaction after one of its statements has failed, or after
    /// SQL Server has ended its database transaction: until its rollback the transaction stores nothing more.
    /// It is a <see cref="DbException"/>, as an error of SQL Server is, so callers treat both alike: roll back
    /// and redo the work. The inner exception is the failure of the statement, when there is one.
    /// </summary>
    public sealed class TransactionFailedException : DbException
    {
        internal TransactionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
