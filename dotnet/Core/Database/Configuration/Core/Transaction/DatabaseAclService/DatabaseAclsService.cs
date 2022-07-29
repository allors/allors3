// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Database.Security;
    using Domain;
    using Services;

    public class DatabaseAclsService : IDatabaseAclsService
    {
        public User User { get; }

        public ISecurity Security { get; }

        public DatabaseAclsService(User user, ISecurity security)
        {
            this.User = user;
            this.Security = security;
        }

        public IAccessControl Create() => new DatabaseAccessControl(this.Security, this.User);
    }
}
