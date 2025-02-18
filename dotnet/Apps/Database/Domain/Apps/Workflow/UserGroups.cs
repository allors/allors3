// <copyright file="UserGroups.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System;

    public partial class UserGroups
    {
        public static readonly Guid EmployeesId = new Guid("ED2D31E3-E18E-4C08-9AF3-F9D849D0F6B2");

        public UserGroup Employees => this.Cache[EmployeesId];

        protected override void AppsSetup(Setup setup)
        {
            var merge = this.cache.Merger().Action();

            merge(EmployeesId, v =>
            {
                v.Name = "Employees";
                v.IsSelectable = true;
            });

            merge(AdministratorsId, v =>
            {
                v.IsSelectable = true;
            });

            merge(GuestsId, v =>
            {
                v.IsSelectable = true;
            });

            merge(CreatorsId, v =>
            {
                v.IsSelectable = false;
            });
        }
    }
}
