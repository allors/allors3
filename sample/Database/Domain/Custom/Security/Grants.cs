// <copyright file="Grants.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Grants
    {
        public static readonly Guid WorkersId = new Guid("5d168fec-21f6-4b02-a235-b916eca8b7dc");

        public Grant Workers => this.Cache[WorkersId];

        // The security role and the user group of the grant have to exist before the merge.
        public Grant MergeWorkers() => this.Cache.Merger().Action()(WorkersId, v =>
        {
            v.Role = new Roles(this.Transaction).Worker;
            v.AddSubjectGroup(new UserGroups(this.Transaction).Workers);
        });

        // Core makes the setup of grants run after the setup of security roles and user groups.
        protected override void CustomSetup(Setup setup)
        {
            if (setup.Config.SetupSecurity)
            {
                this.MergeWorkers();
            }
        }
    }
}
