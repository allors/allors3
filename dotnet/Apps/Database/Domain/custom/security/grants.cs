// <copyright file="Roles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Grants
    {
        public static readonly Guid CustomerContactsId = new Guid("3f2d49f7-d2c9-41e8-84cc-9dc81e532078");

        public Grant CustomerContacts => this.Cache[CustomerContactsId];

        protected override void CustomSetup(Setup setup)
        {
            if (setup.Config.SetupSecurity)
            {
                var merge = this.Cache.Merger().Action();

                var roles = new Roles(this.Transaction);
                var userGroups = new UserGroups(this.Transaction);

                merge(CustomerContactsId, v =>
                {
                    v.Role = roles.CustomerContact;
                    v.AddSubjectGroup(userGroups.CustomerContacts);
                });
            }
        }
    }
}
