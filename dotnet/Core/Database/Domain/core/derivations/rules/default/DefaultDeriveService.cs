// <copyright file="DefaultDerive.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Rules.Default
{
    using Database.Derivations;
    using Database.Services;


    public class DefaultDeriveService : IDeriveService
    {
        private readonly ITransaction transaction;

        public DefaultDeriveService(ITransaction transaction) => this.transaction = transaction;

        public IValidation Derive() => this.transaction.Derive();
    }
}
