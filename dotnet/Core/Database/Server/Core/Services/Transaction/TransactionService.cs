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
    using Microsoft.Extensions.Configuration;

    public class TransactionService : ITransactionService, IDisposable
    {
        public TransactionService(IDatabaseService databaseService, IClaimsPrincipalService claimsPrincipalService, IConfiguration configuration)
        {
            this.Transaction = databaseService.Database.CreateTransaction();

            if (claimsPrincipalService.User?.Identity?.IsAuthenticated == true)
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
                // Anonymous access is opt-in and resolves to a real (guest) user by name — never null,
                // which the ACL cannot dereference. Absent config, the request stays user-less as before.
                var anonymousUserName = configuration["Security:AnonymousUserName"];
                if (!string.IsNullOrWhiteSpace(anonymousUserName))
                {
                    var m = this.Transaction.Database.Services.Get<Database.Meta.MetaPopulation>();
                    this.Transaction.Services.Get<IUserService>().User =
                        (User)new Users(this.Transaction).FindBy(m.User.UserName, anonymousUserName);
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
