// <copyright file="Roles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Roles
    {
        public static readonly Guid CustomerContactId = new Guid("ef339e89-d062-480e-85fa-43440876feb4");

        public Role CustomerContact => this.Cache[CustomerContactId];

        protected override void CustomSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(CustomerContactId, v => v.Name = "Customer contact");
        }
    }
}
