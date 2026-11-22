// <copyright file="Roles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Roles
    {
        public static readonly Guid WorkerId = new Guid("de35c400-e90f-47a8-8488-8773b2fb0e33");

        public Role Worker => this.Cache[WorkerId];

        // A merge finds the security role by its unique id, or creates it when the database has none.
        protected override void CustomSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(WorkerId, v => v.Name = "Worker");
        }
    }
}
