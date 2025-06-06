// <copyright file="Users.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Users
    {
        protected override void CustomPrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, this.M.Locale);
            setup.AddDependency(this.ObjectType, this.M.Singleton);
            setup.AddDependency(this.ObjectType, this.M.ContactMechanismPurpose);
        }
    }
}
