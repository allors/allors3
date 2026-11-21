// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    // The changes to the data that a new version of the model needs, such as a value for a role that the new
    // version requires. The command upgrade runs it after the load, and derives before it commits.
    public class Upgrade
    {
        public Upgrade(ITransaction transaction) => this.Transaction = transaction;

        public ITransaction Transaction { get; }

        public void Execute()
        {
        }
    }
}
