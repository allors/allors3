// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Security
    {
        private void CustomOnPreSetup()
        {
        }

        // Core grants no permissions: an application gives each Role what its members may do.
        private void CustomOnPostSetup()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };
            foreach (ObjectType @class in m.DatabaseClasses)
            {
                this.GrantAdministrator(@class, full);
            }

            // A worker reads work items and marks them done, and reads the names of people.
            this.Grant(Roles.WorkerId, m.WorkItem, Operations.Read);
            this.Grant(Roles.WorkerId, m.WorkItem, m.WorkItem.Done, Operations.Write);
            this.Grant(Roles.WorkerId, m.Person, m.Person.FirstName, Operations.Read);
            this.Grant(Roles.WorkerId, m.Person, m.Person.LastName, Operations.Read);
        }
    }
}
