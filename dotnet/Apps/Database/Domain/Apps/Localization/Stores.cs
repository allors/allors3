// <copyright file="Stores.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Stores
    {
        protected override void AppsPrepare(Setup setup)
        {
            setup.AddDependency(this.ObjectType, this.M.BillingProcess);
            setup.AddDependency(this.ObjectType, this.M.InternalOrganisation);
        }
    }
}
