// <copyright file="TransactionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Bogus;
    using Database;

    public static class TransactionExtensions
    {
        public static Faker Faker(this ITransaction @this) => @this.Database.Services.Get<Faker>();
    }
}
