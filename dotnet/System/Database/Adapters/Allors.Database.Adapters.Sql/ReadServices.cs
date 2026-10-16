// <copyright file="ReadServices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    internal sealed class ReadServices : ITransactionServices
    {
        private readonly ITransactionServices services;
        private readonly ReadGuard guard;

        internal ReadServices(ITransactionServices services, ReadGuard guard)
        {
            this.services = services;
            this.guard = guard;
        }

        public void OnInit(ITransaction transaction) => this.guard.Reject("Transaction-service reinitialization");

        public T Get<T>() => this.guard.Execute(this.services.Get<T>);

        public void Dispose() => this.guard.Reject("Caller transaction-service disposal");
    }
}
