// <copyright file="Roles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Roles
    {
        public static readonly Guid EmployeeId = new Guid("A084F8C0-A130-4D2F-8404-8A11D3D93F14");

        public Role Employee => this.Cache[EmployeeId];

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(EmployeeId, v => v.Name = "Employee");
        }
    }
}
