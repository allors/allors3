// <copyright file="TransactionExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    public static class TransactionExtensions
    {
        public static User GetUser(this ITransaction @this) => @this.Services.Get<IUserService>().User;

        public static void SetUser(this ITransaction @this, User user) => @this.Services.Get<IUserService>().User = user;
    }
}
