// <copyright file="TransactionService.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Security.Claims;
    using System.Linq;
    using Database;
    using Database.Domain;
    using Database.Services;

    public class TransactionService : ITransactionService, IDisposable
    {
        public TransactionService(IDatabaseService databaseService, IClaimsPrincipalService claimsPrincipalService)
        {
            this.Transaction = databaseService.Database.CreateTransaction();

            if (claimsPrincipalService.User != null)
            {
                var nameIdentifier = claimsPrincipalService.User.Claims
                    .FirstOrDefault(v => v.Type == ClaimTypes.NameIdentifier)
                    ?.Value;

                if (long.TryParse(nameIdentifier, out var userId))
                {
                    this.Transaction.Services.Get<IUserService>().User = (User)this.Transaction.Instantiate(userId);
                }
            }
            else
            {
                // TODO: move to base
                //this.Transaction.Services.Get<IUserService>().User = new AutomatedAgents(this.Transaction).Guest;
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
