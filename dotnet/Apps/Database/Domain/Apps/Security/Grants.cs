// <copyright file="Roles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Grants
    {
        public static readonly Guid EmployeesId = new Guid("C1D5C7A3-673E-41FD-BF5D-94438307A7E3");

        public Grant Employees => this.Cache[EmployeesId];

        protected override void AppsSetup(Setup setup)
        {
            if (setup.Config.SetupSecurity)
            {
                var merge = this.Cache.Merger().Action();

                var roles = new Roles(this.Transaction);
                var userGroups = new UserGroups(this.Transaction);

                merge(EmployeesId, v =>
                {
                    v.Role = roles.Employee;
                    v.AddSubjectGroup(userGroups.Employees);
                });
            }
        }
    }
}
