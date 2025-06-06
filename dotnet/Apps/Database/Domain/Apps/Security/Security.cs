// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using Meta;

    public partial class Security
    {
        public void GrantExceptEmployee(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.EmployeeId, objectType, excepts, operations);

        public void GrantExceptGuest(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.GuestId, objectType, excepts, operations);

        private void AppsOnPreSetup()
        {
        }

        private void AppsOnPostSetup()
        {
        }
    }
}
