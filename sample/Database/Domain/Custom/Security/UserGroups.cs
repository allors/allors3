// <copyright file="UserGroups.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class UserGroups
    {
        public static readonly Guid WorkersId = new Guid("03910bbe-2843-47b6-bc80-ae1176bbba38");

        public UserGroup Workers => this.Cache[WorkersId];

        protected override void CustomSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(WorkersId, v => v.Name = "Workers");
        }
    }
}
