// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The changes to the data that a new version needs, such as a value for a role that the new version requires,
    // or the objects that its setup creates. The command upgrade runs it after the load, and derives before it
    // commits.
    public class Upgrade
    {
        public Upgrade(ITransaction transaction) => this.Transaction = transaction;

        public ITransaction Transaction { get; }

        public void Execute()
        {
            // A population that an earlier version saved can lack the security role Worker, the user group Workers
            // and their grant on the default security token, which the setup creates. The merges of the setup create
            // what is missing, and leave the members of the user group as they are.
            new Roles(this.Transaction).MergeWorker();
            new UserGroups(this.Transaction).MergeWorkers();
            var workers = new Grants(this.Transaction).MergeWorkers();
            new SecurityTokens(this.Transaction).DefaultSecurityToken.AddGrant(workers);
        }
    }
}
