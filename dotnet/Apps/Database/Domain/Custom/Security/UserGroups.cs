// <copyright file="UserGroups.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System;

    public partial class UserGroups
    {
        public static readonly Guid CustomerContactsId = new Guid("add6deb1-8498-4780-8b69-3a98aa0b63f7");

        public UserGroup CustomerContacts => this.Cache[CustomerContactsId];

        protected override void CustomSetup(Setup setup)
        {
            var merge = this.cache.Merger().Action();

            merge(CustomerContactsId, v =>
            {
                v.Name = "All customer Contacts";
                v.IsSelectable = false;
            });
        }
    }
}
