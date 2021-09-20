// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using Domain;
    using Security;
    using Services;

    public class DatabaseAclsService : IDatabaseAclsService
    {
        public User User { get; }

        public DatabaseAclsService(User user) => this.User = user;

        public IAccessControl Create() => new DatabaseAccessControl(this.User);
    }
}
