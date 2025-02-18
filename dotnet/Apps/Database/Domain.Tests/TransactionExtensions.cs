// <copyright file="TransactionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Database.Security;
    using Database.Services;

    public static class TransactionExtensions
    {
        public static IUser GetUser(this ITransaction @this) => @this.Services.Get<IUserService>().User;

        public static void SetUser(this ITransaction @this, User user) => @this.Services.Get<IUserService>().User = user;
    }
}
