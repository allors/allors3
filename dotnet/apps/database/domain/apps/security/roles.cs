// <copyright file="Roles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Roles
    {
        public static readonly Guid EmployeeId = new Guid("A084F8C0-A130-4D2F-8404-8A11D3D93F14");
        public static readonly Guid CustomerContactId = new Guid("ef339e89-d062-480e-85fa-43440876feb4");

        public Role Employee => this.Cache[EmployeeId];

        public Role CustomerContact => this.Cache[CustomerContactId];

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(EmployeeId, v => v.Name = "Employee");
            merge(CustomerContactId, v => v.Name = "Customer contact");
        }
    }
}
