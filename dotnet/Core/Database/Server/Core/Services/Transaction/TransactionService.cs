// <copyright file="TransactionService.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using Database;
    using Database.Services;

    public class TransactionService : ITransactionService, IDisposable
    {
        public TransactionService(IDatabaseService databaseService, IClaimsPrincipalService claimsPrincipalService, IUserResolver userResolver)
        {
            this.Transaction = databaseService.Database.CreateTransaction();

            // The authentication plug-in tells who a signed-in principal is; an anonymous request has
            // no user, and the access control lists give it nothing.
            var principal = claimsPrincipalService.User;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var user = userResolver.Resolve(principal, this.Transaction);
                if (user != null)
                {
                    this.Transaction.Services.Get<IUserService>().User = user;
                }
            }
        }

        public ITransaction Transaction { get; private set; }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }
    }
}
