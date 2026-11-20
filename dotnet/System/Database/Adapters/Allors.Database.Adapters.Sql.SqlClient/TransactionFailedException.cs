// <copyright file="TransactionFailedException.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Data.Common;

    /// <summary>
    /// Thrown by subsequent statements and commits when the driver reports a failure while executing a
    /// statement, reading a row or reading a column value, or when SQL Server has ended the database
    /// transaction. The inner exception is the first failure reported by the driver, if any.
    /// Roll back and redo the work.
    /// </summary>
    public sealed class TransactionFailedException : DbException
    {
        internal TransactionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
