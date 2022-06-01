// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class Organisation
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var transaction = this.Strategy.Transaction;

            if (!this.IsInternalOrganisation)
            {
                var groupName = $"Customer contacts at {this.Name ?? "Unknown"} ({this.UniqueId})";

                if (!this.ExistContactsSecurityToken)
                {
                    this.ContactsSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistContactsUserGroup)
                {
                    this.ContactsUserGroup = new UserGroupBuilder(transaction).WithName(groupName).Build();
                    this.ContactsUserGroup.isSelectable = this.IsInternalOrganisation;
                }

                if (!this.ExistContactsGrant)
                {
                    var role = new Roles(transaction).CustomerContact;

                    this.ContactsGrant = new GrantBuilder(transaction)
                        .WithRole(role)
                        .WithSubjectGroup(this.ContactsUserGroup)
                        .Build();

                    this.ContactsSecurityToken.AddGrant(this.ContactsGrant);
                }

                this.ContactsUserGroup.Members = this.CurrentContacts.ToArray();

                foreach(var member in this.CurrentContacts)
                {
                    new UserGroups(transaction).CustomerContacts.AddMember(member);
                }

                foreach (var member in this.InactiveContacts)
                {
                    new UserGroups(transaction).CustomerContacts.RemoveMember(member);
                }
            }

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken, this.ContactsSecurityToken
            };
        }
    }
}
