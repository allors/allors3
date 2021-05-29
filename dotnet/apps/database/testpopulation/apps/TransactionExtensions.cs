// <copyright file="TransactionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Bogus;
    using Database;
    using Database.Domain;

    public static class TransactionExtensions
    {
        public static Faker Faker(this ITransaction @this) => ((dynamic)@this.Database.Services()).Faker;
    }
}
