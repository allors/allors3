// <copyright file="TransactionService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using Database;

    public class TransactionService : ITransactionService, IDisposable
    {
        public TransactionService(IDatabaseService databaseService) => this.Transaction = databaseService.Database.CreateTransaction();

        public ITransaction Transaction { get; private set; }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }
    }
}
