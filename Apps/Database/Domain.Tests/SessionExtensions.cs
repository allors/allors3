// <copyright file="Fixture.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Database;
    using Domain;

    public static class SessionExtensions
    {
        public static User GetUser(this ISession @this) => @this.State().User;

        public static void SetUser(this ISession @this, User user) => @this.State().User = user;
    }
}
